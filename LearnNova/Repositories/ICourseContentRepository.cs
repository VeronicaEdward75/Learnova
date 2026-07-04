using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public interface ICourseContentRepository : IGenericRepository<CourseContent>
{
    Task<IEnumerable<CourseContent>> GetByCourseIdAsync(int courseId);
    Task<int> GetMaxOrderIndexAsync(int courseId);
}
