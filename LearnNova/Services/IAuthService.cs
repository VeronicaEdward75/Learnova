using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels;

namespace LearnNova.Services;

public enum LoginOutcome
{
    Succeeded,
    InvalidCredentials,
    PendingApproval,
    AccountDeactivated,
    UnconfirmedEmail
}

public record AuthResult(LoginOutcome Outcome, ApplicationUser? User);

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password, bool rememberMe);
    Task<ServiceResult<ApplicationUser>> RegisterAsync(RegisterViewModel model);
    Task LogoutAsync();
}
