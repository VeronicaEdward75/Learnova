using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnNova.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly ICourseService _courseService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(IUserService userService, ICourseService courseService, UserManager<ApplicationUser> userManager)
    {
        _userService = userService;
        _courseService = courseService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "لوحة التحكم";
        ViewBag.ActiveNav = "dashboard";

        var courses = (await _courseService.GetAllCoursesAsync()).ToList();
        var vm = new AdminDashboardViewModel
        {
            TotalStudents = (await _userService.GetUsersByRoleAsync(UserRole.Student)).Count(),
            TotalTeachers = (await _userService.GetUsersByRoleAsync(UserRole.Teacher)).Count(),
            TotalAdmins = (await _userService.GetUsersByRoleAsync(UserRole.Admin)).Count(),
            PendingTeachersCount = (await _userService.SearchUsersAsync(null, UserRole.Teacher, false)).Count(),
            TotalCourses = courses.Count,
            PublishedCoursesCount = courses.Count(c => c.IsPublished),
        };

        return View(vm);
    }

    public async Task<IActionResult> Users(string? term, UserRole? role, bool? isActive)
    {
        ViewData["Title"] = "المستخدمون";
        ViewBag.ActiveNav = "users";
        ViewBag.Term = term ?? "";
        ViewBag.RoleFilter = role?.ToString() ?? "";
        ViewBag.IsActiveFilter = isActive is null ? "" : isActive.Value ? "true" : "false";
        ViewBag.CurrentUserId = _userManager.GetUserId(User);

        var users = await _userService.SearchUsersAsync(term, role, isActive);
        return View(users);
    }

    public async Task<IActionResult> UserDetails(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "المستخدم غير موجود";
            return RedirectToAction(nameof(Users));
        }

        ViewData["Title"] = "تفاصيل المستخدم";
        ViewBag.ActiveNav = "users";
        ViewBag.CurrentUserId = _userManager.GetUserId(User);
        return View(user);
    }

    [HttpGet]
    public IActionResult CreateAdmin()
    {
        ViewData["Title"] = "إنشاء مدير جديد";
        ViewBag.ActiveNav = "create-admin";
        return View(new CreateAdminViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(CreateAdminViewModel model)
    {
        ViewData["Title"] = "إنشاء مدير جديد";
        ViewBag.ActiveNav = "create-admin";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            Email = model.Email,
        };

        var result = await _userService.CreateUserAsync(user, model.Password, UserRole.Admin);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        TempData["SuccessMessage"] = $"تم إنشاء حساب المدير \"{model.FullName}\" بنجاح.";
        return RedirectToAction(nameof(Users), new { role = UserRole.Admin });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id, bool targetStatus, string? returnUrl)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (id == currentUserId)
        {
            TempData["ErrorMessage"] = "لا يمكنك تغيير حالة حسابك الخاص.";
            return LocalRedirectOrDefault(returnUrl, nameof(Users));
        }

        var result = await _userService.SetActiveStatusAsync(id, targetStatus);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded
                ? (targetStatus ? "تم تفعيل الحساب بنجاح." : "تم إيقاف الحساب بنجاح.")
                : string.Join(" ", result.Errors);

        return LocalRedirectOrDefault(returnUrl, nameof(Users));
    }

    public async Task<IActionResult> Courses(string? term, bool? isPublished)
    {
        ViewData["Title"] = "الكورسات";
        ViewBag.ActiveNav = "courses";
        ViewBag.Term = term ?? "";
        ViewBag.IsPublishedFilter = isPublished is null ? "" : isPublished.Value ? "true" : "false";

        var courses = await _courseService.SearchCoursesAsync(term, isPublished);
        return View(courses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id, string? returnUrl)
    {
        var result = await _courseService.SetPublishedStatusAsync(id, true);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم نشر الكورس بنجاح." : string.Join(" ", result.Errors);

        return LocalRedirectOrDefault(returnUrl, nameof(Courses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(int id, string? returnUrl)
    {
        var result = await _courseService.SetPublishedStatusAsync(id, false);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم إلغاء نشر الكورس." : string.Join(" ", result.Errors);

        return LocalRedirectOrDefault(returnUrl, nameof(Courses));
    }

    private IActionResult LocalRedirectOrDefault(string? returnUrl, string defaultAction)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(defaultAction);
    }
}
