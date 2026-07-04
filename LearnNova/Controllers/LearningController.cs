using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnNova.Controllers;

[Authorize(Roles = "Student")]
public class LearningController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IProgressService _progressService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LearningController(
        ICourseService courseService,
        IEnrollmentService enrollmentService,
        IProgressService progressService,
        UserManager<ApplicationUser> userManager)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _progressService = progressService;
        _userManager = userManager;
    }

    // GET /Learning/Index/5?contentId=10
    public async Task<IActionResult> Index(int courseId, int? contentId)
    {
        var studentId = _userManager.GetUserId(User)!;

        // Rule: Student must be enrolled
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(studentId, courseId);
        if (!isEnrolled)
        {
            TempData["ErrorMessage"] = "غير مصرح لك بالوصول لهذا الكورس.";
            return RedirectToAction("MyCourses", "Enrollment");
        }

        var course = await _courseService.GetPublishedCourseByIdAsync(courseId);
        if (course == null)
            return NotFound();

        var contents = course.Contents.OrderBy(c => c.OrderIndex).ToList();
        var completedIds = await _progressService.GetCompletedContentIdsAsync(studentId, courseId);
        
        CourseContent? currentContent = null;
        if (contentId.HasValue)
        {
            currentContent = contents.FirstOrDefault(c => c.Id == contentId.Value);
        }
        
        if (currentContent == null && contents.Any())
        {
            // Continue learning: Find the first uncompleted lesson
            currentContent = contents.FirstOrDefault(c => !completedIds.Contains(c.Id)) ?? contents.Last();
        }

        var vm = new LearningViewModel
        {
            Course = course,
            Contents = contents,
            CurrentContent = currentContent,
            CompletedContentIds = completedIds,
            CourseProgressPercentage = await _progressService.GetCourseProgressPercentageAsync(studentId, courseId, contents.Count)
        };

        if (currentContent != null)
        {
            var currentIndex = contents.IndexOf(currentContent);
            if (currentIndex > 0)
                vm.PrevContentId = contents[currentIndex - 1].Id;
            if (currentIndex < contents.Count - 1)
                vm.NextContentId = contents[currentIndex + 1].Id;
        }

        ViewBag.ActiveNav = "my-courses";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkCompleted(int courseId, int contentId, int? nextContentId)
    {
        var studentId = _userManager.GetUserId(User)!;
        await _progressService.MarkAsCompletedAsync(studentId, contentId);

        if (nextContentId.HasValue)
        {
            return RedirectToAction("Index", new { courseId, contentId = nextContentId.Value });
        }

        return RedirectToAction("Index", new { courseId, contentId });
    }
}
