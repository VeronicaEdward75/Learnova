using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnNova.Controllers;

[Authorize(Roles = "Teacher")]
public class AssignmentController : Controller
{
    private readonly IAssignmentService _assignmentService;
    private readonly ICourseService _courseService;
    private readonly IAssignmentSubmissionService _submissionService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Services.DateTimeService.IDateTimeService _dateTimeService;

    public AssignmentController(
        IAssignmentService assignmentService,
        ICourseService courseService,
        IAssignmentSubmissionService submissionService,
        UserManager<ApplicationUser> userManager,
        Services.DateTimeService.IDateTimeService dateTimeService)
    {
        _assignmentService = assignmentService;
        _courseService = courseService;
        _submissionService = submissionService;
        _userManager = userManager;
        _dateTimeService = dateTimeService;
    }

    public async Task<IActionResult> Index(int courseId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var course = await _courseService.GetCourseByIdAsync(courseId);

        if (course == null || course.TeacherId != teacherId)
            return RedirectToAction("MyCourses", "Teacher");

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Title;

        var assignments = await _assignmentService.GetAssignmentsByCourseAsync(courseId, teacherId);
        return View(assignments);
    }

    public async Task<IActionResult> Create(int courseId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var course = await _courseService.GetCourseByIdAsync(courseId);

        if (course == null || course.TeacherId != teacherId)
            return RedirectToAction("MyCourses", "Teacher");

        ViewBag.CourseTitle = course.Title;

        var model = new AssignmentFormViewModel
        {
            CourseId = courseId,
            DueDate = _dateTimeService.ToLocal(_dateTimeService.UtcNow()).AddDays(7)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssignmentFormViewModel model)
    {
        var teacherId = _userManager.GetUserId(User)!;

        if (!ModelState.IsValid)
        {
            var course = await _courseService.GetCourseByIdAsync(model.CourseId);
            ViewBag.CourseTitle = course?.Title;
            return View(model);
        }

        var result = await _assignmentService.CreateAssignmentAsync(model.CourseId, model, teacherId);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "تم إضافة المهمة بنجاح.";
            return RedirectToAction("Index", new { courseId = model.CourseId });
        }

        TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var assignment = await _assignmentService.GetAssignmentByIdAsync(id, teacherId);

        if (assignment == null)
            return RedirectToAction("MyCourses", "Teacher");

        var course = await _courseService.GetCourseByIdAsync(assignment.CourseId);
        ViewBag.CourseTitle = course?.Title;

        var model = new AssignmentFormViewModel
        {
            Id = assignment.Id,
            CourseId = assignment.CourseId,
            Title = assignment.Title,
            Description = assignment.Description,
            DueDate = _dateTimeService.ToLocal(assignment.DueDate),
            MaxGrade = assignment.MaxGrade
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AssignmentFormViewModel model)
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

        var result = await _assignmentService.UpdateAssignmentAsync(id, model, teacherId);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "تم تعديل المهمة بنجاح.";
            return RedirectToAction("Index", new { courseId = model.CourseId });
        }

        TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _assignmentService.DeleteAssignmentAsync(id, teacherId);

        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم حذف المهمة بنجاح." : string.Join(" ", result.Errors);

        return RedirectToAction("Index", new { courseId });
    }

    public async Task<IActionResult> Submissions(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var assignment = await _assignmentService.GetAssignmentByIdAsync(id, teacherId);

        if (assignment == null)
            return RedirectToAction("MyCourses", "Teacher");

        ViewBag.AssignmentTitle = assignment.Title;
        ViewBag.MaxGrade = assignment.MaxGrade;
        ViewBag.CourseId = assignment.CourseId;

        var submissions = await _submissionService.GetSubmissionsForAssignmentAsync(id, teacherId);
        return View(submissions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Grade(SubmissionGradeViewModel model)
    {
        var teacherId = _userManager.GetUserId(User)!;

        var result = await _submissionService.GradeSubmissionAsync(model, teacherId);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "تم حفظ التقييم بنجاح.";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }

        return RedirectToAction("Submissions", new { id = model.AssignmentId });
    }
}
