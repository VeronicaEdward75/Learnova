using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public interface IContentProgressRepository : IGenericRepository<ContentProgress>
{
    Task<ContentProgress?> GetProgressAsync(string studentId, int contentId);
    Task<IEnumerable<ContentProgress>> GetStudentProgressInCourseAsync(string studentId, int courseId);
}
