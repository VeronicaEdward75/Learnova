using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher;

namespace LearnNova.Services;

public interface IQuizQuestionService
{
    Task<IEnumerable<QuizQuestion>> GetQuestionsForQuizAsync(int quizId, string teacherId);
    Task<QuizQuestion?> GetQuestionByIdAsync(int id, string teacherId);
    Task<ServiceResult> CreateQuestionAsync(QuestionFormViewModel model, string teacherId);
    Task<ServiceResult> UpdateQuestionAsync(QuestionFormViewModel model, string teacherId);
    Task<ServiceResult> DeleteQuestionAsync(int id, string teacherId);
    Task<ServiceResult> MoveUpAsync(int id, string teacherId);
    Task<ServiceResult> MoveDownAsync(int id, string teacherId);
}
