using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher;

namespace LearnNova.Services;

public interface IQuizService
{
    Task<IEnumerable<Quiz>> GetQuizzesForCourseAsync(int courseId, string teacherId);
    Task<Quiz?> GetQuizByIdAsync(int id, string teacherId);
    Task<ServiceResult> CreateQuizAsync(QuizFormViewModel model, string teacherId);
    Task<ServiceResult> UpdateQuizAsync(QuizFormViewModel model, string teacherId);
    Task<ServiceResult> DeleteQuizAsync(int id, string teacherId);
    Task<ServiceResult> TogglePublishStatusAsync(int id, string teacherId);
}
