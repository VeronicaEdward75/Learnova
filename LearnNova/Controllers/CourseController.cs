using LearnNova.Models.ViewModels.Student;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LearnNova.Models.Entities;

namespace LearnNova.Controllers;

[Authorize(Roles = "Student")]
public class CourseController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CourseController(
        ICourseService courseService,
        IEnrollmentService enrollmentService,
        UserManager<ApplicationUser> userManager)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _userManager = userManager;
    }

    // GET: /Course/Catalog
    public async Task<IActionResult> Catalog(string? term, string? subject, string? stage)
    {
        ViewBag.ActiveNav = "catalog";

        var courses = await _courseService.SearchPublishedCoursesAsync(term, subject, stage);

        var vm = new CourseCatalogViewModel
        {
            Courses = courses,
            Term = term,
            Subject = subject,
            Stage = stage
        };

        return View(vm);
    }

    // GET: /Course/Details/5
    public async Task<IActionResult> Details(int id)
    {
        ViewBag.ActiveNav = "catalog";

        var course = await _courseService.GetPublishedCourseByIdAsync(id);
        if (course == null)
            return NotFound();

        var studentId = _userManager.GetUserId(User)!;
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(studentId, id);

        ViewBag.IsEnrolled = isEnrolled;
        
        return View(course);
    }
}
