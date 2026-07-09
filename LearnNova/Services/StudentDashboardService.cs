using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Services;

public class StudentDashboardService : IStudentDashboardService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseContentRepository _courseContentRepository;
    private readonly IProgressService _progressService;
    private readonly IGenericRepository<Certificate> _certificateRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IGenericRepository<AssignmentSubmission> _assignmentSubmissionRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizAttemptRepository _quizAttemptRepository;
    private readonly IPaymentRepository _paymentRepository;

    public StudentDashboardService(
        IEnrollmentRepository enrollmentRepository,
        ICourseContentRepository courseContentRepository,
        IProgressService progressService,
        IGenericRepository<Certificate> certificateRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IAssignmentRepository assignmentRepository,
        IGenericRepository<AssignmentSubmission> assignmentSubmissionRepository,
        IQuizRepository quizRepository,
        IQuizAttemptRepository quizAttemptRepository,
        IPaymentRepository paymentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseContentRepository = courseContentRepository;
        _progressService = progressService;
        _certificateRepository = certificateRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _assignmentRepository = assignmentRepository;
        _assignmentSubmissionRepository = assignmentSubmissionRepository;
        _quizRepository = quizRepository;
        _quizAttemptRepository = quizAttemptRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<StudentDashboardViewModel> GetDashboardAsync(string studentId, string studentName)
    {
        var vm = new StudentDashboardViewModel
        {
            StudentName = studentName
        };

        var enrollments = await _enrollmentRepository.GetQueryable()
            .AsNoTracking()
            .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
            .Where(e => e.StudentId == studentId && e.IsActive)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        vm.EnrolledCoursesCount = enrollments.Count;
        var courseIds = enrollments.Select(e => e.CourseId).ToList();

        var contentCountsByCourse = courseIds.Count == 0
            ? new Dictionary<int, int>()
            : await _courseContentRepository.GetQueryable()
                .AsNoTracking()
                .Where(c => courseIds.Contains(c.CourseId))
                .GroupBy(c => c.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        var progressByCourse = await _progressService.GetCourseProgressPercentagesAsync(studentId, contentCountsByCourse);

        var lastActiveEnrollment = enrollments.FirstOrDefault();
        if (lastActiveEnrollment?.Course != null)
        {
            vm.LastActiveCourse = lastActiveEnrollment.Course;
            vm.LastActiveCourseProgress = progressByCourse.GetValueOrDefault(lastActiveEnrollment.CourseId, 0);
        }

        vm.CompletedCoursesCount = progressByCourse.Values.Count(p => p == 100);
        vm.OverallCompletionPercentage = enrollments.Count > 0
            ? (int)Math.Round(progressByCourse.Values.DefaultIfEmpty(0).Average())
            : 0;

        var certificatesQuery = _certificateRepository.GetQueryable()
            .AsNoTracking()
            .Include(c => c.Course)
            .Where(c => c.StudentId == studentId);

        vm.CertificatesCount = await certificatesQuery.CountAsync();
        vm.RecentCertificates = await certificatesQuery
            .OrderByDescending(c => c.IssueDate)
            .Take(3)
            .ToListAsync();

        var wallet = await _walletRepository.GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == studentId);

        if (wallet == null)
        {
            wallet = new Wallet { UserId = studentId, AvailableBalance = 0, PendingBalance = 0, TotalEarned = 0, TotalWithdrawn = 0 };
            await _walletRepository.AddAsync(wallet);
            await _walletRepository.SaveChangesAsync();
        }

        vm.WalletBalance = wallet.AvailableBalance;
        vm.RecentTransactions = await _walletTransactionRepository.GetQueryable()
            .AsNoTracking()
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new WalletTransactionViewModel
            {
                Id = t.Id,
                Date = t.CreatedAt,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Description = t.Description ?? string.Empty,
                Status = "Completed",
                CourseName = "N/A",
                StudentName = "N/A"
            })
            .ToListAsync();

        var now = DateTime.UtcNow;
        var assignmentItems = await BuildAssignmentItemsAsync(studentId, courseIds);

        var pendingAssignments = assignmentItems.Where(a => a.Submission == null).ToList();
        vm.PendingAssignmentsCount = pendingAssignments.Count;
        vm.UpcomingAssignments = pendingAssignments
            .Where(a => a.Assignment != null && a.Assignment.DueDate > now)
            .OrderBy(a => a.Assignment!.DueDate)
            .Select(a => a.Assignment!)
            .Take(3)
            .ToList();
        vm.TotalAssignmentsCompleted = assignmentItems.Count(a =>
            a.Submission != null &&
            (a.Submission.Status == SubmitStatus.Graded || a.Submission.Status == SubmitStatus.Submitted));

        var quizCards = await BuildQuizCardsAsync(studentId, courseIds);
        var availableQuizzes = quizCards
            .Where(q => q.TotalAttemptsMade == 0 || q.HasUncompletedAttempt)
            .ToList();

        vm.AvailableQuizzesCount = availableQuizzes.Count;
        vm.AvailableQuizzes = availableQuizzes.Take(3).ToList();
        vm.TotalQuizzesPassed = quizCards.Count(q => q.Passed);

        vm.TimelineEvents = await BuildTimelineEventsAsync(studentId, vm.RecentTransactions, certificatesQuery);

        return vm;
    }

    private async Task<List<AssignmentItemViewModel>> BuildAssignmentItemsAsync(string studentId, List<int> courseIds)
    {
        if (courseIds.Count == 0)
            return new List<AssignmentItemViewModel>();

        return await _assignmentRepository.GetQueryable()
            .AsNoTracking()
            .Where(a => courseIds.Contains(a.CourseId))
            .Select(a => new AssignmentItemViewModel
            {
                Assignment = a,
                Submission = _assignmentSubmissionRepository.GetQueryable()
                    .FirstOrDefault(s => s.AssignmentId == a.Id && s.StudentId == studentId)
            })
            .ToListAsync();
    }

    private async Task<List<MyQuizCardViewModel>> BuildQuizCardsAsync(string studentId, List<int> courseIds)
    {
        if (courseIds.Count == 0)
            return new List<MyQuizCardViewModel>();

        var quizzes = await _quizRepository.GetQueryable()
            .AsNoTracking()
            .Include(q => q.Course)
                .ThenInclude(c => c.Teacher)
            .Include(q => q.Questions)
            .Where(q => courseIds.Contains(q.CourseId))
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

        if (quizzes.Count == 0)
            return new List<MyQuizCardViewModel>();

        var quizIds = quizzes.Select(q => q.Id).ToList();
        var allAttempts = await _quizAttemptRepository.GetQueryable()
            .AsNoTracking()
            .Where(a => a.StudentId == studentId && quizIds.Contains(a.QuizId))
            .ToListAsync();

        var result = new List<MyQuizCardViewModel>(quizzes.Count);
        foreach (var quiz in quizzes)
        {
            var quizAttempts = allAttempts.Where(a => a.QuizId == quiz.Id).ToList();
            var completedAttempts = quizAttempts.Where(a => a.IsCompleted).ToList();

            result.Add(new MyQuizCardViewModel
            {
                QuizId = quiz.Id,
                CourseName = quiz.Course?.Title ?? "Unknown Course",
                InstructorName = quiz.Course?.Teacher?.FullName ?? "Unknown Instructor",
                QuizTitle = quiz.Title,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                QuestionCount = quiz.Questions?.Count ?? 0,
                PassingScore = quiz.PassingScore,
                CreatedAt = quiz.CreatedAt,
                MaxAttempts = quiz.MaxAttempts,
                TotalAttemptsMade = quizAttempts.Count,
                CompletedAttempts = completedAttempts.Count,
                HasUncompletedAttempt = quizAttempts.Any(a => !a.IsCompleted),
                BestScore = completedAttempts.Count > 0 ? completedAttempts.Max(a => a.Score) : 0,
                LatestScore = completedAttempts.Count > 0
                    ? completedAttempts.OrderByDescending(a => a.SubmittedAt).First().Score
                    : 0,
                Passed = completedAttempts.Any(a => a.Passed)
            });
        }

        return result;
    }

    private async Task<List<TimelineEventViewModel>> BuildTimelineEventsAsync(
        string studentId,
        List<WalletTransactionViewModel> recentTransactions,
        IQueryable<Certificate> certificatesQuery)
    {
        var timeline = new List<TimelineEventViewModel>();

        var purchases = await _paymentRepository.GetQueryable()
            .AsNoTracking()
            .Include(p => p.Course)
            .Where(p => p.StudentId == studentId && p.Status == PaymentStatus.Succeeded)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();

        timeline.AddRange(purchases.Select(o => new TimelineEventViewModel
        {
            Date = o.CreatedAt,
            Title = "تم شراء كورس",
            Description = o.Course?.Title ?? "N/A",
            EventType = "Purchase",
            IconClass = "bi-cart-check",
            ColorClass = "text-success",
            Url = $"/Course/Details/{o.CourseId}"
        }));

        var certificateEvents = await certificatesQuery
            .Select(c => new
            {
                c.IssueDate,
                CourseTitle = c.Course != null ? c.Course.Title : "N/A"
            })
            .ToListAsync();

        timeline.AddRange(certificateEvents.Select(c => new TimelineEventViewModel
        {
            Date = c.IssueDate,
            Title = "الحصول على شهادة",
            Description = c.CourseTitle,
            EventType = "Certificate",
            IconClass = "bi-award",
            ColorClass = "text-warning",
            Url = "/Certificate/MyCertificates"
        }));

        timeline.AddRange(recentTransactions
            .Where(t => t.Type == TransactionType.Refund.ToString())
            .Select(t => new TimelineEventViewModel
            {
                Date = t.Date,
                Title = "استرداد أموال",
                Description = t.Description,
                EventType = "Refund",
                IconClass = "bi-arrow-counterclockwise",
                ColorClass = "text-info",
                Url = "/Order/Wallet"
            }));

        return timeline.OrderByDescending(t => t.Date).Take(10).ToList();
    }
}
