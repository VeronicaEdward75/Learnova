using System.Text.Json;
using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class QuizAttemptService : IQuizAttemptService
{
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizQuestionRepository _questionRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly Services.DateTimeService.IDateTimeService _dateTimeService;

    public QuizAttemptService(
        IQuizAttemptRepository attemptRepository,
        IQuizRepository quizRepository,
        IQuizQuestionRepository questionRepository,
        IEnrollmentRepository enrollmentRepository,
        Services.DateTimeService.IDateTimeService dateTimeService)
    {
        _attemptRepository = attemptRepository;
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
        _enrollmentRepository = enrollmentRepository;
        _dateTimeService = dateTimeService;
    }

    public async Task<int> GetAttemptsCountAsync(int quizId, string studentId)
    {
        return await _attemptRepository.GetAttemptsCountAsync(quizId, studentId);
    }

    public async Task<ServiceResult<QuizEngineViewModel>> GetQuizForStudentAsync(int quizId, string studentId)
    {
        var quiz = await _quizRepository.GetByIdWithCourseAsync(quizId);
        if (quiz == null || !quiz.IsPublished)
            return ServiceResult<QuizEngineViewModel>.Fail("الاختبار غير موجود أو غير متاح.");

        var enrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(studentId, quiz.CourseId);
        if (enrollment == null)
            return ServiceResult<QuizEngineViewModel>.Fail("غير مصرح لك بدخول هذا الاختبار لأنك لست مسجلاً في الكورس.");

        var attemptCount = await _attemptRepository.GetAttemptsCountAsync(quizId, studentId);
        if (attemptCount >= quiz.MaxAttempts)
            return ServiceResult<QuizEngineViewModel>.Fail("لقد استنفدت جميع محاولاتك لهذا الاختبار.");

        // Create an uncompleted attempt to track the start time
        var newAttempt = new QuizAttempt
        {
            QuizId = quizId,
            StudentId = studentId,
            AttemptNumber = attemptCount + 1,
            StartedAt = _dateTimeService.UtcNow(),
            IsCompleted = false
        };
        await _attemptRepository.AddAsync(newAttempt);
        await _attemptRepository.SaveChangesAsync();

        var questions = (await _questionRepository.GetByQuizIdAsync(quizId)).ToList();
        
        var vm = new QuizEngineViewModel
        {
            QuizId = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            PassingScore = quiz.PassingScore,
            MaxAttempts = quiz.MaxAttempts,
            AttemptId = newAttempt.Id,
            Questions = questions.Select(q => new QuizQuestionDto
            {
                Id = q.Id,
                OrderIndex = q.OrderIndex,
                Text = q.Text,
                Points = q.Points,
                Type = q.Type,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD
            }).ToList()
        };

        return ServiceResult<QuizEngineViewModel>.Success(vm);
    }

    public async Task<ServiceResult<int>> SubmitQuizAsync(QuizSubmitViewModel model, string studentId)
    {
        var attempt = await _attemptRepository.GetAttemptByIdAsync(model.AttemptId, studentId);
        if (attempt == null || attempt.IsCompleted)
            return ServiceResult<int>.Fail("المحاولة غير موجودة أو تم تسليمها مسبقاً.");

        if (attempt.QuizId != model.QuizId)
            return ServiceResult<int>.Fail("بيانات الاختبار غير متطابقة.");

        var quiz = await _quizRepository.GetByIdAsync(model.QuizId);
        if (quiz == null)
            return ServiceResult<int>.Fail("الاختبار غير موجود.");

        var questions = (await _questionRepository.GetByQuizIdAsync(model.QuizId)).ToList();
        
        int score = 0;
        int totalPoints = questions.Sum(q => q.Points);

        foreach (var question in questions)
        {
            if (model.Answers.TryGetValue(question.Id, out var studentAnswer))
            {
                if (string.Equals(studentAnswer?.Trim(), question.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    score += question.Points;
                }
            }
        }

        double percentage = totalPoints > 0 ? ((double)score / totalPoints) * 100 : 0;
        bool passed = percentage >= quiz.PassingScore;

        attempt.SubmittedAt = _dateTimeService.UtcNow();
        attempt.DurationSeconds = (int)(attempt.SubmittedAt - attempt.StartedAt).TotalSeconds;
        attempt.Score = score;
        attempt.TotalPoints = totalPoints;
        attempt.Percentage = Math.Round(percentage, 2);
        attempt.Passed = passed;
        attempt.IsCompleted = true;
        attempt.AnswersJson = JsonSerializer.Serialize(model.Answers);

        _attemptRepository.Update(attempt);
        await _attemptRepository.SaveChangesAsync();

        return ServiceResult<int>.Success(attempt.Id);
    }

    public async Task<ServiceResult<QuizResultViewModel>> GetQuizResultAsync(int attemptId, string studentId)
    {
        var attempt = await _attemptRepository.GetAttemptByIdAsync(attemptId, studentId);
        if (attempt == null || !attempt.IsCompleted)
            return ServiceResult<QuizResultViewModel>.Fail("لا توجد نتيجة لهذه المحاولة.");

        var vm = new QuizResultViewModel
        {
            QuizId = attempt.QuizId,
            QuizTitle = attempt.Quiz.Title,
            CourseId = attempt.Quiz.CourseId,
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            Percentage = attempt.Percentage,
            Passed = attempt.Passed,
            AttemptNumber = attempt.AttemptNumber,
            MaxAttempts = attempt.Quiz.MaxAttempts,
            DurationSeconds = attempt.DurationSeconds
        };

        return ServiceResult<QuizResultViewModel>.Success(vm);
    }

    public async Task<ServiceResult<StudentQuizHistoryViewModel>> GetStudentHistoryAsync(int quizId, string studentId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz == null)
            return ServiceResult<StudentQuizHistoryViewModel>.Fail("الاختبار غير موجود.");

        var attempts = await _attemptRepository.GetStudentAttemptsHistoryAsync(quizId, studentId);
        var questions = (await _questionRepository.GetByQuizIdAsync(quizId)).ToList();

        var vm = new StudentQuizHistoryViewModel
        {
            QuizId = quizId,
            QuizTitle = quiz.Title,
            CourseId = quiz.CourseId,
            ShowAnswers = quiz.ShowAnswers
        };

        foreach (var attempt in attempts)
        {
            var attemptDto = new StudentAttemptDetailsDto
            {
                AttemptId = attempt.Id,
                AttemptNumber = attempt.AttemptNumber,
                Score = attempt.Score,
                TotalPoints = attempt.TotalPoints,
                Percentage = attempt.Percentage,
                Passed = attempt.Passed,
                DurationSeconds = attempt.DurationSeconds,
                SubmittedAt = attempt.SubmittedAt
            };

            if (quiz.ShowAnswers)
            {
                var answers = string.IsNullOrWhiteSpace(attempt.AnswersJson) 
                    ? new Dictionary<int, string>() 
                    : JsonSerializer.Deserialize<Dictionary<int, string>>(attempt.AnswersJson);

                foreach (var q in questions)
                {
                    string studentAnswer = answers != null && answers.ContainsKey(q.Id) ? answers[q.Id] : "لم يجب";
                    bool isCorrect = string.Equals(studentAnswer?.Trim(), q.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);

                    attemptDto.AnswersBreakdown.Add(new StudentAnswerBreakdownDto
                    {
                        QuestionId = q.Id,
                        QuestionText = q.Text,
                        StudentAnswer = studentAnswer ?? "لم يجب",
                        CorrectAnswer = q.CorrectAnswer ?? "",
                        IsCorrect = isCorrect,
                        Points = isCorrect ? q.Points : 0,
                        MaxPoints = q.Points
                    });
                }
            }

            vm.Attempts.Add(attemptDto);
        }

        return ServiceResult<StudentQuizHistoryViewModel>.Success(vm);
    }

    public async Task<ServiceResult<QuizAnalyticsViewModel>> GetQuizAnalyticsAsync(int quizId, string teacherId, string searchTerm, string sortBy)
    {
        var quiz = await _quizRepository.GetByIdWithCourseAsync(quizId);
        if (quiz == null)
            return ServiceResult<QuizAnalyticsViewModel>.Fail("الاختبار غير موجود.");

        if (quiz.Course.TeacherId != teacherId)
            return ServiceResult<QuizAnalyticsViewModel>.Fail("غير مصرح لك باستعراض بيانات هذا الاختبار.");

        var stats = await _attemptRepository.GetQuizAggregateStatsAsync(quizId);
        var attempts = await _attemptRepository.GetTeacherQuizResultsAsync(quizId, searchTerm, sortBy);

        var vm = new QuizAnalyticsViewModel
        {
            QuizId = quizId,
            QuizTitle = quiz.Title,
            CourseId = quiz.CourseId,
            TotalAttempts = stats.TotalAttempts,
            UniqueStudentsAttempted = stats.UniqueStudents,
            HighestScore = stats.HighestScore,
            LowestScore = stats.LowestScore,
            AverageScore = Math.Round(stats.AverageScore, 2),
            PassRate = stats.TotalAttempts > 0 ? Math.Round(((double)stats.PassedCount / stats.TotalAttempts) * 100, 2) : 0,
            FailRate = stats.TotalAttempts > 0 ? Math.Round(((double)stats.FailedCount / stats.TotalAttempts) * 100, 2) : 0,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            StudentAttempts = attempts.Select(a => new QuizAttemptSummaryDto
            {
                AttemptId = a.Id,
                StudentId = a.StudentId,
                StudentName = a.Student?.FullName ?? "غير معروف",
                Score = a.Score,
                TotalPoints = a.TotalPoints,
                Percentage = a.Percentage,
                Passed = a.Passed,
                AttemptNumber = a.AttemptNumber,
                SubmittedAt = a.SubmittedAt
            }).ToList()
        };

        return ServiceResult<QuizAnalyticsViewModel>.Success(vm);
    }

    public async Task<MyQuizzesViewModel> GetMyQuizzesDashboardAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.QuizFilterParameters filters)
    {
        var pagedResult = await _quizRepository.GetMyQuizzesDashboardPagedAsync(studentId, filters);
        
        var viewModel = new MyQuizzesViewModel
        {
            Quizzes = pagedResult,
            TotalQuizzes = pagedResult.TotalCount
        };
        
        foreach (var card in pagedResult.Items)
        {
            if (card.Passed) viewModel.PassedCount++;
            else if (card.CompletedAttempts > 0 && card.RemainingAttempts == 0) viewModel.FailedCount++;
        }
        
        if (pagedResult.Items.Any(q => q.CompletedAttempts > 0))
        {
            var attemptedQuizzes = pagedResult.Items.Where(q => q.CompletedAttempts > 0).ToList();
            viewModel.AverageScore = attemptedQuizzes.Average(q => 
                q.QuestionCount > 0 ? ((double)q.BestScore / (q.QuestionCount * 10)) * 100 : 0);
        }
        
        return viewModel;
    }

    public async Task<ServiceResult<QuizDetailsViewModel>> GetQuizDetailsAsync(int quizId, string studentId)
    {
        var quiz = await _quizRepository.GetByIdWithCourseAsync(quizId);
        if (quiz == null || !quiz.IsPublished)
            return ServiceResult<QuizDetailsViewModel>.Fail("الاختبار غير موجود أو غير متاح.");

        var enrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(studentId, quiz.CourseId);
        if (enrollment == null)
            return ServiceResult<QuizDetailsViewModel>.Fail("غير مصرح لك بدخول هذا الاختبار لأنك لست مسجلاً في الكورس.");
            
        var attemptHistory = (await _attemptRepository.GetAllStudentAttemptsAsync(studentId))
            .Where(a => a.QuizId == quizId)
            .OrderByDescending(a => a.StartedAt)
            .ToList();
            
        var vm = new QuizDetailsViewModel
        {
            Quiz = quiz,
            AttemptHistory = attemptHistory,
            AttemptsUsed = attemptHistory.Count,
            HasUncompletedAttempt = attemptHistory.Any(a => !a.IsCompleted)
        };
        
        return ServiceResult<QuizDetailsViewModel>.Success(vm);
    }
}
