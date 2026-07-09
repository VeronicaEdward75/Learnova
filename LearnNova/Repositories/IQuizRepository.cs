using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public interface IQuizRepository : IGenericRepository<Quiz>
{
    Task<IEnumerable<Quiz>> GetByCourseIdAsync(int courseId);
    Task<IEnumerable<Quiz>> GetByEnrolledStudentAsync(string studentId);
    Task<LearnNova.Models.ViewModels.PagedResult<LearnNova.Models.ViewModels.Student.MyQuizCardViewModel>> GetMyQuizzesDashboardPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.QuizFilterParameters filters);
    Task<Quiz?> GetByIdWithCourseAsync(int id);
}
