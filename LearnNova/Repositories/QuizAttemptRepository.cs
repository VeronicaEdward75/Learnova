using LearnNova.Data;
using LearnNova.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

public class QuizAttemptRepository : GenericRepository<QuizAttempt>, IQuizAttemptRepository
{
    public QuizAttemptRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<int> GetAttemptsCountAsync(int quizId, string studentId)
    {
        return await _set.CountAsync(a => a.QuizId == quizId && a.StudentId == studentId && a.IsCompleted);
    }

    public async Task<QuizAttempt?> GetLatestAttemptAsync(int quizId, string studentId)
    {
        return await _set
            .Where(a => a.QuizId == quizId && a.StudentId == studentId)
            .OrderByDescending(a => a.AttemptNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<QuizAttempt?> GetAttemptByIdAsync(int attemptId, string studentId)
    {
        return await _set
            .Include(a => a.Quiz)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.StudentId == studentId);
    }

    public async Task<IEnumerable<QuizAttempt>> GetStudentAttemptsHistoryAsync(int quizId, string studentId)
    {
        return await _set
            .AsNoTracking()
            .Where(a => a.QuizId == quizId && a.StudentId == studentId && a.IsCompleted)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<QuizAttempt>> GetTeacherQuizResultsAsync(int quizId, string searchTerm, string sortBy)
    {
        var query = _set
            .AsNoTracking()
            .Include(a => a.Student)
            .Where(a => a.QuizId == quizId && a.IsCompleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(a => a.Student != null && ((a.Student.FullName != null && a.Student.FullName.Contains(searchTerm)) || (a.Student.Email != null && a.Student.Email.Contains(searchTerm))));
        }

        query = sortBy switch
        {
            "Highest" => query.OrderByDescending(a => a.Percentage),
            "Lowest" => query.OrderBy(a => a.Percentage),
            "Oldest" => query.OrderBy(a => a.SubmittedAt),
            _ => query.OrderByDescending(a => a.SubmittedAt) // "Newest" by default
        };

        return await query.ToListAsync();
    }

    public async Task<(int TotalAttempts, int UniqueStudents, int HighestScore, int LowestScore, double AverageScore, int PassedCount, int FailedCount)> GetQuizAggregateStatsAsync(int quizId)
    {
        var completedAttempts = _set.AsNoTracking().Where(a => a.QuizId == quizId && a.IsCompleted);
        
        var totalAttempts = await completedAttempts.CountAsync();
        if (totalAttempts == 0) return (0, 0, 0, 0, 0, 0, 0);

        var uniqueStudents = await completedAttempts.Select(a => a.StudentId).Distinct().CountAsync();
        var highest = await completedAttempts.MaxAsync(a => a.Score);
        var lowest = await completedAttempts.MinAsync(a => a.Score);
        var avg = await completedAttempts.AverageAsync(a => (double)a.Percentage);
        var passedCount = await completedAttempts.CountAsync(a => a.Passed);
        var failedCount = totalAttempts - passedCount;

        return (totalAttempts, uniqueStudents, highest, lowest, avg, passedCount, failedCount);
    }
}
