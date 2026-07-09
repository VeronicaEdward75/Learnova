using LearnNova.Models.Entities;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LearnNova.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IActionResult> MyCourses([FromQuery] LearnNova.Models.ViewModels.Student.Filters.CourseFilterParameters filters)
    {
        ViewData["Title"] = "كورساتي";
        ViewBag.ActiveNav = "my-courses";

        var studentId = _userManager.GetUserId(User)!;
        
        filters.SortOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("الأحدث أولاً", "newest"),
            new("الأقدم أولاً", "oldest"),
            new("الاسم (أ-ي)", "name_asc"),
            new("الاسم (ي-أ)", "name_desc")
        };

        var enrollments = await _enrollmentService.GetStudentCoursesPagedAsync(studentId, filters);
        
        ViewBag.Filters = filters;

        return View(enrollments);
    }
}
