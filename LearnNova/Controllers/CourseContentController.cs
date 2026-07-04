using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LearnNova.Models.Entities;

namespace LearnNova.Controllers;

[Authorize(Roles = "Teacher")]
public class CourseContentController : Controller
{
    private readonly ICourseContentService _contentService;
    private readonly ICourseService _courseService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CourseContentController(
        ICourseContentService contentService,
        ICourseService courseService,
        UserManager<ApplicationUser> userManager)
    {
        _contentService = contentService;
        _courseService = courseService;
        _userManager = userManager;
    }

    // GET: /CourseContent/Index/5
    public async Task<IActionResult> Index(int courseId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        
        // Ensure course exists and belongs to teacher
        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null || course.TeacherId != teacherId)
            return RedirectToAction("MyCourses", "Teacher");

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Title;

        var contents = await _contentService.GetContentsByCourseAsync(courseId, teacherId);
        return View(contents);
    }

    // GET: /CourseContent/Create/5
    public async Task<IActionResult> Create(int courseId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null || course.TeacherId != teacherId)
            return RedirectToAction("MyCourses", "Teacher");

        var model = new CourseContentFormViewModel
        {
            CourseId = courseId
        };
        
        ViewBag.CourseTitle = course.Title;
        return View(model);
    }

    // POST: /CourseContent/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseContentFormViewModel model)
    {
        var teacherId = _userManager.GetUserId(User)!;

        if (!ModelState.IsValid)
        {
            var course = await _courseService.GetCourseByIdAsync(model.CourseId);
            ViewBag.CourseTitle = course?.Title;
            return View(model);
        }

        var result = await _contentService.AddContentAsync(model.CourseId, model, teacherId);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "تم إضافة المحتوى بنجاح.";
            return RedirectToAction("Index", new { courseId = model.CourseId });
        }

        TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        return View(model);
    }

    // GET: /CourseContent/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var content = await _contentService.GetContentByIdAsync(id, teacherId);
        if (content == null)
            return RedirectToAction("MyCourses", "Teacher");

        var model = new CourseContentFormViewModel
        {
            Id = content.Id,
            CourseId = content.CourseId,
            Title = content.Title,
            FileUrl = content.FileUrl,
            DurationSec = content.DurationSec
        };

        var course = await _courseService.GetCourseByIdAsync(content.CourseId);
        ViewBag.CourseTitle = course?.Title;
        return View(model);
    }

    // POST: /CourseContent/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseContentFormViewModel model)
    {
        var teacherId = _userManager.GetUserId(User)!;

        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            var course = await _courseService.GetCourseByIdAsync(model.CourseId);
            ViewBag.CourseTitle = course?.Title;
            return View(model);
        }

        var result = await _contentService.UpdateContentAsync(id, model, teacherId);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "تم تعديل المحتوى بنجاح.";
            return RedirectToAction("Index", new { courseId = model.CourseId });
        }

        TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        return View(model);
    }

    // POST: /CourseContent/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _contentService.DeleteContentAsync(id, teacherId);
        
        if (result.Succeeded)
            TempData["SuccessMessage"] = "تم حذف المحتوى بنجاح.";
        else
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);

        return RedirectToAction("Index", new { courseId });
    }

    // POST: /CourseContent/Reorder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder(int courseId, [FromBody] List<int> orderedIds)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _contentService.ReorderContentAsync(courseId, orderedIds, teacherId);

        if (result.Succeeded)
            return Json(new { success = true });

        return Json(new { success = false, message = string.Join(" ", result.Errors) });
    }
}
