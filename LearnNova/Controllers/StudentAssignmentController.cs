using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnNova.Controllers;

[Authorize(Roles = "Student")]
public class StudentAssignmentController : Controller
{
    private readonly IAssignmentSubmissionService _submissionService;
    private readonly ICourseService _courseService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public StudentAssignmentController(
        IAssignmentSubmissionService submissionService,
        ICourseService courseService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _submissionService = submissionService;
        _courseService = courseService;
        _userManager = userManager;
        _env = env;
    }

    public async Task<IActionResult> Center()
    {
        var studentId = _userManager.GetUserId(User)!;
        ViewBag.ActiveNav = "assignments";

        var assignments = await _submissionService.GetAllStudentAssignmentsAsync(studentId);
        
        return View(assignments);
    }

    public async Task<IActionResult> Index(int courseId)
    {
        var studentId = _userManager.GetUserId(User)!;
        var course = await _courseService.GetCourseByIdAsync(courseId);

        if (course == null)
            return RedirectToAction("MyCourses", "Enrollment");

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Title;

        var assignments = await _submissionService.GetStudentAssignmentsWithStatusAsync(courseId, studentId);
        
        // If the collection is empty but the course exists, the student might not be enrolled
        if (!assignments.Any() && course != null)
        {
            // Validate enrollment strictly inside service, returning empty if not enrolled
            var checkEnrollment = assignments;
        }

        return View(assignments);
    }

    public async Task<IActionResult> Details(int assignmentId)
    {
        var studentId = _userManager.GetUserId(User)!;
        
        var model = await _submissionService.GetAssignmentForSubmissionAsync(assignmentId, studentId);
        if (model == null)
            return RedirectToAction("MyCourses", "Enrollment");

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int assignmentId, AssignmentSubmitViewModel model)
    {
        var studentId = _userManager.GetUserId(User)!;

        var result = await _submissionService.SubmitAssignmentAsync(
            assignmentId, 
            studentId, 
            model.File, 
            model.TextAnswer, 
            _env.WebRootPath);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "تم تسليم المهمة بنجاح.";
            return RedirectToAction(nameof(Details), new { assignmentId });
        }

        TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Details), new { assignmentId });
    }
}
