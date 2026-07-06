using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Repositories;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnNova.Controllers;

[Authorize(Roles = "Student")]
public class QuizEngineController : Controller
{
    private readonly IQuizAttemptService _attemptService;
    private readonly IQuizRepository _quizRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuizEngineController(
        IQuizAttemptService attemptService, 
        IQuizRepository quizRepository,
        IEnrollmentRepository enrollmentRepository,
        UserManager<ApplicationUser> userManager)
    {
        _attemptService = attemptService;
        _quizRepository = quizRepository;
        _enrollmentRepository = enrollmentRepository;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int courseId)
    {
        var studentId = _userManager.GetUserId(User)!;
        var enrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(studentId, courseId);
        if (enrollment == null)
            return RedirectToAction("MyCourses", "Enrollment");

        var quizzes = (await _quizRepository.GetByCourseIdAsync(courseId))
                        .Where(q => q.IsPublished)
                        .ToList();

        var viewModel = new StudentQuizListViewModel
        {
            CourseId = courseId,
            Quizzes = new List<StudentQuizCardViewModel>()
        };

        foreach (var q in quizzes)
        {
            int usedAttempts = await _attemptService.GetAttemptsCountAsync(q.Id, studentId);
            viewModel.Quizzes.Add(new StudentQuizCardViewModel
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                TimeLimitMinutes = q.TimeLimitMinutes,
                PassingScore = q.PassingScore,
                QuestionCount = q.Questions?.Count ?? 0,
                MaxAttempts = q.MaxAttempts,
                UsedAttempts = usedAttempts
            });
        }

        return View(viewModel);
    }

    public async Task<IActionResult> TakeQuiz(int quizId)
    {
        var studentId = _userManager.GetUserId(User)!;
        var result = await _attemptService.GetQuizForStudentAsync(quizId, studentId);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            if (quiz != null)
            {
                return RedirectToAction(nameof(Index), new { courseId = quiz.CourseId });
            }
            return RedirectToAction("MyCourses", "Enrollment");
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz(QuizSubmitViewModel model)
    {
        var studentId = _userManager.GetUserId(User)!;
        
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "خطأ في إرسال البيانات.";
            return RedirectToAction("MyCourses", "Enrollment");
        }

        var result = await _attemptService.SubmitQuizAsync(model, studentId);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
            return RedirectToAction("MyCourses", "Enrollment");
        }

        return RedirectToAction(nameof(Result), new { attemptId = result.Data });
    }

    public async Task<IActionResult> Result(int attemptId)
    {
        var studentId = _userManager.GetUserId(User)!;
        var result = await _attemptService.GetQuizResultAsync(attemptId, studentId);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
            return RedirectToAction("MyCourses", "Enrollment");
        }

        return View(result.Data);
    }

    public async Task<IActionResult> History(int quizId)
    {
        var studentId = _userManager.GetUserId(User)!;
        var result = await _attemptService.GetStudentHistoryAsync(quizId, studentId);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
            return RedirectToAction("MyCourses", "Enrollment");
        }

        return View(result.Data);
    }
}
