using LearnNova.Models.Entities;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnNova.Controllers;

[Authorize(Roles = "Student")]
public class EnrollmentController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public EnrollmentController(IEnrollmentService enrollmentService, UserManager<ApplicationUser> userManager)
    {
        _enrollmentService = enrollmentService;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var studentId = _userManager.GetUserId(User)!;
        var result = await _enrollmentService.EnrollAsync(studentId, courseId);

        if (!result.Succeeded)
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        else
            TempData["SuccessMessage"] = "تم التسجيل في الكورس بنجاح! 🎉";

        return RedirectToAction("Details", "Course", new { id = courseId });
    }

    public async Task<IActionResult> MyCourses()
    {
        ViewData["Title"] = "كورساتي";
        ViewBag.ActiveNav = "my-courses";

        var studentId = _userManager.GetUserId(User)!;
        var enrollments = await _enrollmentService.GetStudentCoursesAsync(studentId);
        return View(enrollments);
    }
}
