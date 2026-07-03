using LearnNova.Models.Entities;

namespace LearnNova.Services;

public interface ICourseService
{
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<IEnumerable<Course>> SearchCoursesAsync(string? term, bool? isPublished);
    Task<Course?> GetCourseByIdAsync(int id);
    Task<ServiceResult> SetPublishedStatusAsync(int id, bool isPublished);
}
