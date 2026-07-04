using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnNova.Controllers;

[Authorize(Roles = "Teacher")]
public class QuizController : Controller
{
    private readonly IQuizService _quizService;
    private readonly ICourseService _courseService;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuizController(
        IQuizService quizService,
        ICourseService courseService,
        UserManager<ApplicationUser> userManager)
    {
        _quizService = quizService;
        _courseService = courseService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int courseId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var course = await _courseService.GetCourseByIdAsync(courseId);

        if (course == null || course.TeacherId != teacherId)
            return RedirectToAction("MyCourses", "Teacher");

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Title;

        var quizzes = (await _quizService.GetQuizzesForCourseAsync(courseId, teacherId)).ToList();
        
        var attemptService = HttpContext.RequestServices.GetRequiredService<LearnNova.Services.IQuizAttemptService>();
        var attemptRepo = HttpContext.RequestServices.GetRequiredService<LearnNova.Repositories.IQuizAttemptRepository>();
        
        var statsDict = new Dictionary<int, (int TotalAttempts, double AverageScore, double PassRate)>();
        
        foreach (var quiz in quizzes)
        {
            var stats = await attemptRepo.GetQuizAggregateStatsAsync(quiz.Id);
            double passRate = stats.TotalAttempts > 0 ? Math.Round(((double)stats.PassedCount / stats.TotalAttempts) * 100, 2) : 0;
            statsDict[quiz.Id] = (stats.TotalAttempts, Math.Round(stats.AverageScore, 2), passRate);
        }
        
        ViewBag.Stats = statsDict;
        return View(quizzes);
    }

    public async Task<IActionResult> Create(int courseId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var course = await _courseService.GetCourseByIdAsync(courseId);

        if (course == null || course.TeacherId != teacherId)
            return RedirectToAction("MyCourses", "Teacher");

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Title;

        var model = new QuizFormViewModel { CourseId = courseId };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuizFormViewModel model)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var course = await _courseService.GetCourseByIdAsync(model.CourseId);

        if (course == null || course.TeacherId != teacherId)
            return RedirectToAction("MyCourses", "Teacher");

        if (!ModelState.IsValid)
        {
            ViewBag.CourseId = model.CourseId;
            ViewBag.CourseTitle = course.Title;
            return View(model);
        }

        var result = await _quizService.CreateQuizAsync(model, teacherId);
        
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "تم إضافة الاختبار بنجاح.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        ModelState.AddModelError("", string.Join(" ", result.Errors));
        ViewBag.CourseId = model.CourseId;
        ViewBag.CourseTitle = course.Title;
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var quiz = await _quizService.GetQuizByIdAsync(id, teacherId);

        if (quiz == null)
            return RedirectToAction("MyCourses", "Teacher");

        ViewBag.CourseId = quiz.CourseId;
        ViewBag.CourseTitle = quiz.Course.Title;

        var model = new QuizFormViewModel
        {
            Id = quiz.Id,
            CourseId = quiz.CourseId,
            Title = quiz.Title,
            Description = quiz.Description,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            PassingScore = quiz.PassingScore,
            MaxAttempts = quiz.MaxAttempts
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(QuizFormViewModel model)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var course = await _courseService.GetCourseByIdAsync(model.CourseId);

        if (course == null || course.TeacherId != teacherId)
            return RedirectToAction("MyCourses", "Teacher");

        if (!ModelState.IsValid)
        {
            ViewBag.CourseId = model.CourseId;
            ViewBag.CourseTitle = course.Title;
            return View(model);
        }

        var result = await _quizService.UpdateQuizAsync(model, teacherId);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "تم تعديل الاختبار بنجاح.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        ModelState.AddModelError("", string.Join(" ", result.Errors));
        ViewBag.CourseId = model.CourseId;
        ViewBag.CourseTitle = course.Title;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _quizService.DeleteQuizAsync(id, teacherId);

        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم حذف الاختبار بنجاح." : string.Join(" ", result.Errors);

        return RedirectToAction(nameof(Index), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish(int id, int courseId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _quizService.TogglePublishStatusAsync(id, teacherId);

        if (!result.Succeeded)
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        else
            TempData["SuccessMessage"] = "تم تحديث حالة نشر الاختبار بنجاح.";

        return RedirectToAction(nameof(Index), new { courseId });
    }

    // Sprint 9.5 Analytics
    public async Task<IActionResult> Analytics(int id, string searchTerm = "", string sortBy = "Newest")
    {
        var teacherId = _userManager.GetUserId(User)!;
        
        // IQuizAttemptService is not injected in QuizController yet. I'll need to inject it or fetch it.
        // I will use HttpContext.RequestServices to avoid modifying the constructor if there are many dependencies, or I will inject it.
        var attemptService = HttpContext.RequestServices.GetRequiredService<LearnNova.Services.IQuizAttemptService>();
        
        var result = await attemptService.GetQuizAnalyticsAsync(id, teacherId, searchTerm, sortBy);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
            return RedirectToAction("Dashboard", "Teacher");
        }

        return View(result.Data);
    }
}
