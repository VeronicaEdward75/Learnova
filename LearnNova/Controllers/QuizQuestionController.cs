using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnNova.Controllers;

[Authorize(Roles = "Teacher")]
public class QuizQuestionController : Controller
{
    private readonly IQuizQuestionService _questionService;
    private readonly IQuizService _quizService;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuizQuestionController(
        IQuizQuestionService questionService,
        IQuizService quizService,
        UserManager<ApplicationUser> userManager)
    {
        _questionService = questionService;
        _quizService = quizService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int quizId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var quiz = await _quizService.GetQuizByIdAsync(quizId, teacherId);

        if (quiz == null)
            return RedirectToAction("MyCourses", "Teacher");

        ViewBag.QuizId = quizId;
        ViewBag.QuizTitle = quiz.Title;
        ViewBag.CourseId = quiz.CourseId;

        var questions = await _questionService.GetQuestionsForQuizAsync(quizId, teacherId);
        return View(questions);
    }

    public async Task<IActionResult> Create(int quizId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var quiz = await _quizService.GetQuizByIdAsync(quizId, teacherId);

        if (quiz == null)
            return RedirectToAction("MyCourses", "Teacher");

        ViewBag.QuizId = quizId;
        ViewBag.QuizTitle = quiz.Title;

        var model = new QuestionFormViewModel { QuizId = quizId, Points = 1, Type = QuestionType.MultipleChoice };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuestionFormViewModel model)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var quiz = await _quizService.GetQuizByIdAsync(model.QuizId, teacherId);

        if (quiz == null)
            return RedirectToAction("MyCourses", "Teacher");

        if (!ModelState.IsValid)
        {
            ViewBag.QuizId = model.QuizId;
            ViewBag.QuizTitle = quiz.Title;
            return View(model);
        }

        var result = await _questionService.CreateQuestionAsync(model, teacherId);
        
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "تم إضافة السؤال بنجاح.";
            return RedirectToAction(nameof(Index), new { quizId = model.QuizId });
        }

        ModelState.AddModelError("", string.Join(" ", result.Errors));
        ViewBag.QuizId = model.QuizId;
        ViewBag.QuizTitle = quiz.Title;
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var question = await _questionService.GetQuestionByIdAsync(id, teacherId);

        if (question == null)
            return RedirectToAction("MyCourses", "Teacher");

        ViewBag.QuizId = question.QuizId;
        ViewBag.QuizTitle = question.Quiz.Title;

        var model = new QuestionFormViewModel
        {
            Id = question.Id,
            QuizId = question.QuizId,
            OrderIndex = question.OrderIndex,
            Text = question.Text,
            Type = question.Type,
            OptionA = question.OptionA,
            OptionB = question.OptionB,
            OptionC = question.OptionC,
            OptionD = question.OptionD,
            CorrectAnswer = question.CorrectAnswer,
            Points = question.Points
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(QuestionFormViewModel model)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var quiz = await _quizService.GetQuizByIdAsync(model.QuizId, teacherId);

        if (quiz == null)
            return RedirectToAction("MyCourses", "Teacher");

        if (!ModelState.IsValid)
        {
            ViewBag.QuizId = model.QuizId;
            ViewBag.QuizTitle = quiz.Title;
            return View(model);
        }

        var result = await _questionService.UpdateQuestionAsync(model, teacherId);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "تم تعديل السؤال بنجاح.";
            return RedirectToAction(nameof(Index), new { quizId = model.QuizId });
        }

        ModelState.AddModelError("", string.Join(" ", result.Errors));
        ViewBag.QuizId = model.QuizId;
        ViewBag.QuizTitle = quiz.Title;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int quizId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _questionService.DeleteQuestionAsync(id, teacherId);

        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم حذف السؤال بنجاح." : string.Join(" ", result.Errors);

        return RedirectToAction(nameof(Index), new { quizId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveUp(int id, int quizId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        await _questionService.MoveUpAsync(id, teacherId);
        return RedirectToAction(nameof(Index), new { quizId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveDown(int id, int quizId)
    {
        var teacherId = _userManager.GetUserId(User)!;
        await _questionService.MoveDownAsync(id, teacherId);
        return RedirectToAction(nameof(Index), new { quizId });
    }
}
