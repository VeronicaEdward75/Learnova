using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnNova.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = "تسجيل الدخول";
        ViewData["ReturnUrl"] = returnUrl;
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
}
