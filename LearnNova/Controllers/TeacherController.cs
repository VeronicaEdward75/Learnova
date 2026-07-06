using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LearnNova.Models.Entities;

namespace LearnNova.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly ICourseService _courseService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherController(ICourseService courseService, UserManager<ApplicationUser> userManager)
    {
        _courseService = courseService;
        _userManager = userManager;
    }

    // ─── AJAX helper ───────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult GetGradesByStage(string stage)
    {
        var grades = CourseFormViewModel.BuildGradeList(stage)
            .Select(item => new { num = item.Value, label = item.Text });
        return Json(grades);
    }

    // ─── Dashboard ─────────────────────────────────────────────────────────────

    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "لوحة المدرس";
        ViewBag.ActiveNav = "dashboard";

        var teacherId = _userManager.GetUserId(User)!;
        var courses = (await _courseService.GetTeacherCoursesAsync(teacherId)).ToList();

        ViewBag.TotalCourses    = courses.Count;
        ViewBag.PublishedCount  = courses.Count(c => c.IsPublished);
        ViewBag.UnpublishedCount = courses.Count(c => !c.IsPublished);

        return View();
    }

    // ─── My Courses ────────────────────────────────────────────────────────────

    public async Task<IActionResult> MyCourses()
    {
        ViewData["Title"] = "كورساتي";
        ViewBag.ActiveNav = "my-courses";

        var teacherId = _userManager.GetUserId(User)!;
        var courses = await _courseService.GetTeacherCoursesAsync(teacherId);
        return View(courses);
    }

    // ─── Create ────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult CreateCourse()
    {
        ViewData["Title"] = "إنشاء كورس جديد";
        ViewBag.ActiveNav = "my-courses";
        return View(new CourseFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CourseFormViewModel model)
    {
        ViewData["Title"] = "إنشاء كورس جديد";
        ViewBag.ActiveNav = "my-courses";

        // Rebuild grade list so the dropdown re-populates if we return the view
        model.GradeOptions = CourseFormViewModel.BuildGradeList(model.Stage ?? "");

        if (!ModelState.IsValid)
            return View(model);

        var teacherId = _userManager.GetUserId(User)!;
        var result = await _courseService.CreateCourseAsync(model, teacherId);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["SuccessMessage"] = $"تم إنشاء الكورس \"{model.Title}\" بنجاح.";
        return RedirectToAction(nameof(MyCourses));
    }

    // ─── Edit ──────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> EditCourse(int id)
    {
        ViewData["Title"] = "تعديل الكورس";
        ViewBag.ActiveNav = "my-courses";

        var teacherId = _userManager.GetUserId(User)!;
        var course = await _courseService.GetCourseByIdAsync(id);

        if (course is null || course.TeacherId != teacherId)
        {
            TempData["ErrorMessage"] = "الكورس غير موجود أو غير مصرح لك بتعديله.";
            return RedirectToAction(nameof(MyCourses));
        }

        var model = new CourseFormViewModel
        {
            Title       = course.Title,
            Description = course.Description,
            Subject     = course.Subject,
            Stage       = course.Stage,
            GradeLevel  = course.GradeLevel,
            Price       = course.Price,
            GradeOptions = CourseFormViewModel.BuildGradeList(course.Stage)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(int id, CourseFormViewModel model)
    {
        ViewData["Title"] = "تعديل الكورس";
        ViewBag.ActiveNav = "my-courses";

        model.GradeOptions = CourseFormViewModel.BuildGradeList(model.Stage ?? "");

        if (!ModelState.IsValid)
            return View(model);

        var teacherId = _userManager.GetUserId(User)!;
        var result = await _courseService.UpdateCourseAsync(id, model, teacherId);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["SuccessMessage"] = "تم تحديث الكورس بنجاح.";
        return RedirectToAction(nameof(MyCourses));
    }

    // ─── Delete ────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _courseService.DeleteCourseAsync(id, teacherId);

        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم حذف الكورس بنجاح." : string.Join(" ", result.Errors);

        return RedirectToAction(nameof(MyCourses));
    }

    // ─── Publish / Unpublish ───────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _courseService.SetPublishedStatusAsync(id, true, teacherId);

        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم نشر الكورس بنجاح." : string.Join(" ", result.Errors);

        return RedirectToAction(nameof(MyCourses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _courseService.SetPublishedStatusAsync(id, false, teacherId);

        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم إلغاء نشر الكورس." : string.Join(" ", result.Errors);

        return RedirectToAction(nameof(MyCourses));
    }
}
