using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public interface IQuizAttemptRepository : IGenericRepository<QuizAttempt>
{
    Task<int> GetAttemptsCountAsync(int quizId, string studentId);
    Task<QuizAttempt?> GetLatestAttemptAsync(int quizId, string studentId);
    Task<QuizAttempt?> GetAttemptByIdAsync(int attemptId, string studentId);

    // Sprint 9.5 Analytics
    Task<IEnumerable<QuizAttempt>> GetStudentAttemptsHistoryAsync(int quizId, string studentId);
    Task<IEnumerable<QuizAttempt>> GetTeacherQuizResultsAsync(int quizId, string searchTerm, string sortBy);
    Task<(int TotalAttempts, int UniqueStudents, int HighestScore, int LowestScore, double AverageScore, int PassedCount, int FailedCount)> GetQuizAggregateStatsAsync(int quizId);
}
