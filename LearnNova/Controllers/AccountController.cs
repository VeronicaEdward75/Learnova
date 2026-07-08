using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels;
using LearnNova.Models.ViewModels.Account;
using LearnNova.Services;
using LearnNova.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnNova.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthService authService,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<AccountController> logger)
    {
        _authService = authService;
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null, bool deactivated = false)
    {
        ViewData["Title"] = "تسجيل الدخول";
        ViewData["ReturnUrl"] = returnUrl;

        if (deactivated)
        {
            ModelState.AddModelError(string.Empty, "تم إيقاف هذا الحساب. تواصل مع الإدارة لمزيد من المعلومات.");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["Title"] = "تسجيل الدخول";
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(model.Email, model.Password, model.RememberMe);

        switch (result.Outcome)
        {
            case LoginOutcome.Succeeded:
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return result.User!.Role == UserRole.Admin
                    ? RedirectToAction("Dashboard", "Admin")
                    : RedirectToAction("Index", "Home");

            case LoginOutcome.PendingApproval:
                ModelState.AddModelError(string.Empty, "حسابك كمدرس قيد المراجعة من الإدارة، برجاء الانتظار حتى تتم الموافقة.");
                break;

            case LoginOutcome.AccountDeactivated:
                ModelState.AddModelError(string.Empty, "تم إيقاف هذا الحساب. تواصل مع الإدارة لمزيد من المعلومات.");
                break;

            default:
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
                break;
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewData["Title"] = "حساب جديد";
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ViewData["Title"] = "حساب جديد";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RegisterAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        if (model.Role == UserRole.Teacher)
        {
            TempData["SuccessMessage"] = "تم إنشاء حسابك بنجاح، وهو الآن قيد المراجعة من الإدارة. سيتم إعلامك عند الموافقة.";
            return RedirectToAction(nameof(Login));
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "غير مصرح بالوصول";
        return View();
    }

    // ==========================================
    // FORGOT / RESET PASSWORD
    // ==========================================

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        ViewData["Title"] = "نسيت كلمة المرور";
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        _logger.LogInformation("ForgotPassword POST entered for email: {Email}", model.Email);
        ViewData["Title"] = "نسيت كلمة المرور";

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid.");
            return View(model);
        }

        _logger.LogInformation("Looking up user by email...");
        var user = await _userManager.FindByEmailAsync(model.Email);
        
        if (user == null)
        {
            _logger.LogWarning("User not found for email: {Email}. Returning early.", model.Email);
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }
        
        _logger.LogInformation("User found: {UserId}. Checking if email is confirmed...", user.Id);
        var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        
        if (!isEmailConfirmed)
        {
            _logger.LogWarning("User email is NOT confirmed. Proceeding anyway because email verification is not fully enforced in registration.");
            // return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        _logger.LogInformation("Generating password reset token...");
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        _logger.LogInformation("Token generated successfully.");
        
        _logger.LogInformation("Generating reset URL...");
        var callbackUrl = Url.Action(
            "ResetPassword", 
            "Account", 
            new { email = user.Email, token = token }, 
            protocol: Request.Scheme);
        _logger.LogInformation("Reset URL generated successfully: {Url}", callbackUrl);

        // Send email with premium HTML template
        var emailBody = $@"
        <div style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 40px;'>
            <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; padding: 40px; text-align: center; box-shadow: 0 4px 24px rgba(0,0,0,0.05);'>
                <h1 style='color: #8B5E3C; margin-bottom: 20px;'>منصة LearnNova</h1>
                <h3 style='color: #333;'>مرحباً {user.FullName}،</h3>
                <p style='color: #666; line-height: 1.6; font-size: 16px; margin-bottom: 30px;'>
                    لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك.
                    إذا كنت أنت من طلب ذلك، يرجى الضغط على الزر أدناه لإعادة تعيين كلمة المرور.
                </p>
                <a href='{callbackUrl}' style='display: inline-block; background-color: #8B5E3C; color: white; text-decoration: none; padding: 14px 32px; border-radius: 8px; font-weight: bold; font-size: 16px; margin-bottom: 30px;'>
                    إعادة تعيين كلمة المرور
                </a>
                <p style='color: #999; font-size: 14px; margin-bottom: 10px;'>
                    ملاحظة: هذا الرابط صالح لفترة محدودة فقط لأسباب أمنية.
                </p>
                <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;' />
                <p style='color: #aaa; font-size: 12px;'>
                    &copy; {DateTime.Now.Year} LearnNova. جميع الحقوق محفوظة.
                </p>
            </div>
        </div>";

        _logger.LogInformation("Calling EmailService.SendEmailAsync for email: {Email}", model.Email);
        await _emailService.SendEmailAsync(model.Email, "إعادة تعيين كلمة المرور - LearnNova", emailBody);
        _logger.LogInformation("EmailService.SendEmailAsync returned successfully.");

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        ViewData["Title"] = "تأكيد إرسال البريد";
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (email == null || token == null)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "إعادة تعيين كلمة المرور";
        var model = new ResetPasswordViewModel { Email = email, Token = token };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        ViewData["Title"] = "إعادة تعيين كلمة المرور";

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Security: don't reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        ViewData["Title"] = "تمت إعادة تعيين كلمة المرور";
        return View();
    }

}
