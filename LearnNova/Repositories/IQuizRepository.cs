using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public interface IQuizRepository : IGenericRepository<Quiz>
{
    Task<IEnumerable<Quiz>> GetByCourseIdAsync(int courseId);
    Task<Quiz?> GetByIdWithCourseAsync(int id);
}
