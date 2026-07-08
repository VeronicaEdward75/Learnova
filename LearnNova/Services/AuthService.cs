using System.Security.Claims;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.Lookups;
using LearnNova.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace LearnNova.Services;

public class AuthService : IAuthService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserService _userService;

    public AuthService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserService userService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userService = userService;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new AuthResult(LoginOutcome.InvalidCredentials, null);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            return new AuthResult(LoginOutcome.InvalidCredentials, null);
        }

        if (!user.IsActive)
        {
            var outcome = user.Role == UserRole.Teacher ? LoginOutcome.PendingApproval : LoginOutcome.AccountDeactivated;
            return new AuthResult(outcome, user);
        }

        if (!user.EmailConfirmed)
        {
            return new AuthResult(LoginOutcome.UnconfirmedEmail, user);
        }

        await SignInAsync(user, rememberMe);
        return new AuthResult(LoginOutcome.Succeeded, user);
    }

    public async Task<ServiceResult<ApplicationUser>> RegisterAsync(RegisterViewModel model)
    {
        if (model.Role != UserRole.Student && model.Role != UserRole.Teacher)
        {
            return ServiceResult<ApplicationUser>.Fail("الدور المحدد غير صالح");
        }

        if (await _userService.EmailExistsAsync(model.Email))
        {
            return ServiceResult<ApplicationUser>.Fail("هذا البريد الإلكتروني مستخدم بالفعل");
        }

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            Email = model.Email,
        };

        if (model.Role == UserRole.Student)
        {
            user.Stage = model.Stage;
            user.GradeLevel = model.GradeNum;
            user.GradeLabel = CurriculumLookup.GetGradeLabel(model.Stage, model.GradeNum);
        }
        else
        {
            user.Subject = model.Subject;
            user.TeachStage = model.TeachStage;
        }

        // Teachers start inactive and need an admin to approve them (doc 3 §5.1);
        // students are active immediately, matching the prototype's frictionless signup.
        var isActive = model.Role != UserRole.Teacher;
        var result = await _userService.CreateUserAsync(user, model.Password, model.Role, isActive);
        if (!result.Succeeded)
        {
            return ServiceResult<ApplicationUser>.Fail(result.Errors);
        }

        return ServiceResult<ApplicationUser>.Success(user);
    }

    public Task LogoutAsync() => _signInManager.SignOutAsync();

    private async Task SignInAsync(ApplicationUser user, bool isPersistent)
    {
        // Adds FullName as a claim on the auth cookie so _Layout.cshtml can render the sidebar's
        // name/initial without a DB round-trip on every page (doc 4 §2.4 / prompt.md §5 note 2).
        // Identity still adds its own role/id claims automatically on top of this.
        var claims = new[] { new Claim("FullName", user.FullName) };
        await _signInManager.SignInWithClaimsAsync(user, isPersistent, claims);
    }
}
