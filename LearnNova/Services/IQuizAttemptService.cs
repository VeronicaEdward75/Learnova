using LearnNova.Models.ViewModels.Student;
using LearnNova.Models.ViewModels.Teacher;

namespace LearnNova.Services;

public interface IQuizAttemptService
{
    Task<ServiceResult<QuizEngineViewModel>> GetQuizForStudentAsync(int quizId, string studentId);
    Task<ServiceResult<int>> SubmitQuizAsync(QuizSubmitViewModel model, string studentId);
    Task<ServiceResult<QuizResultViewModel>> GetQuizResultAsync(int attemptId, string studentId);

    // Sprint 9.5
    Task<ServiceResult<StudentQuizHistoryViewModel>> GetStudentHistoryAsync(int quizId, string studentId);
    Task<ServiceResult<QuizAnalyticsViewModel>> GetQuizAnalyticsAsync(int quizId, string teacherId, string searchTerm, string sortBy);
    Task<int> GetAttemptsCountAsync(int quizId, string studentId);
}
