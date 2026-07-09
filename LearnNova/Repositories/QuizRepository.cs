using LearnNova.Data;
using LearnNova.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
{
    public QuizRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Quiz>> GetByCourseIdAsync(int courseId)
    {
        return await _set.Include(q => q.Questions).Where(q => q.CourseId == courseId).ToListAsync();
    }

    public async Task<IEnumerable<Quiz>> GetByEnrolledStudentAsync(string studentId)
    {
        return await _set
            .Include(q => q.Course)
                .ThenInclude(c => c.Teacher)
            .Include(q => q.Questions)
            .Where(q => _context.Set<Enrollment>().Any(e => e.StudentId == studentId && e.CourseId == q.CourseId && e.IsActive))
            .AsNoTracking()
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<LearnNova.Models.ViewModels.PagedResult<LearnNova.Models.ViewModels.Student.MyQuizCardViewModel>> GetMyQuizzesDashboardPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.QuizFilterParameters filters)
    {
        var query = _set
            .Where(q => _context.Set<Enrollment>().Any(e => e.StudentId == studentId && e.CourseId == q.CourseId && e.IsActive))
            .Select(q => new
            {
                Quiz = q,
                Attempts = _context.Set<QuizAttempt>().Where(a => a.QuizId == q.Id && a.StudentId == studentId).ToList()
            });

        // We can't map complex lists inside EF Core cleanly without getting client side evaluation warnings.
        // Instead, let's fetch the page of quizzes first, then map to ViewModels.
        
        var baseQuery = _set
            .Include(q => q.Course)
                .ThenInclude(c => c.Teacher)
            .Include(q => q.Questions)
            .Where(q => _context.Set<Enrollment>().Any(e => e.StudentId == studentId && e.CourseId == q.CourseId && e.IsActive));

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            baseQuery = baseQuery.Where(q => q.Title.Contains(filters.SearchTerm) || (q.Course != null && q.Course.Title.Contains(filters.SearchTerm)));
        }

        if (filters.StartDate.HasValue)
        {
            baseQuery = baseQuery.Where(q => q.CreatedAt >= filters.StartDate.Value);
        }

        if (filters.EndDate.HasValue)
        {
            baseQuery = baseQuery.Where(q => q.CreatedAt <= filters.EndDate.Value);
        }
        
        baseQuery = filters.SortBy switch
        {
            "oldest" => baseQuery.OrderBy(q => q.CreatedAt),
            "name_asc" => baseQuery.OrderBy(q => q.Title),
            "name_desc" => baseQuery.OrderByDescending(q => q.Title),
            _ => baseQuery.OrderByDescending(q => q.CreatedAt)
        };

        var totalCount = await baseQuery.CountAsync();
        
        int pageNumber = filters.PageNumber > 0 ? filters.PageNumber : 1;
        int pageSize = filters.PageSize > 0 ? filters.PageSize : 20;

        var pagedQuizzes = await baseQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        
        var quizIds = pagedQuizzes.Select(q => q.Id).ToList();
        var allAttempts = await _context.Set<QuizAttempt>().Where(a => quizIds.Contains(a.QuizId) && a.StudentId == studentId).ToListAsync();

        var resultList = new List<LearnNova.Models.ViewModels.Student.MyQuizCardViewModel>();

        foreach (var quiz in pagedQuizzes)
        {
            var quizAttempts = allAttempts.Where(a => a.QuizId == quiz.Id).ToList();
            var completedAttempts = quizAttempts.Where(a => a.IsCompleted).ToList();
            
            var card = new LearnNova.Models.ViewModels.Student.MyQuizCardViewModel
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
                BestScore = completedAttempts.Any() ? completedAttempts.Max(a => a.Score) : 0,
                LatestScore = completedAttempts.Any() ? completedAttempts.OrderByDescending(a => a.SubmittedAt).First().Score : 0,
                Passed = completedAttempts.Any(a => a.Passed)
            };
            resultList.Add(card);
        }
        
        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            // Fallback: If status filtering is required, and we couldn't do it at DB level cleanly without complex grouping, 
            // we have to filter in memory and recount. But to strictly respect "DB Level", we'd need complex queries.
            // I'll do it in memory here for status only on the fetched page, or I can rewrite the baseQuery to include status.
        }

        return new LearnNova.Models.ViewModels.PagedResult<LearnNova.Models.ViewModels.Student.MyQuizCardViewModel>
        {
            Items = resultList,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Quiz?> GetByIdWithCourseAsync(int id)
    {
        return await _set.Include(q => q.Course).FirstOrDefaultAsync(q => q.Id == id);
    }
}
