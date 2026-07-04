using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher;

namespace LearnNova.Services;

public interface ICourseService
{
    // Admin / shared
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<IEnumerable<Course>> SearchCoursesAsync(string? term, bool? isPublished);
    Task<Course?> GetCourseByIdAsync(int id);
    Task<ServiceResult> SetPublishedStatusAsync(int id, bool isPublished, string? requestingTeacherId = null);

    // Teacher CRUD
    Task<IEnumerable<Course>> GetTeacherCoursesAsync(string teacherId);
    Task<ServiceResult> CreateCourseAsync(CourseFormViewModel model, string teacherId);
    Task<ServiceResult> UpdateCourseAsync(int id, CourseFormViewModel model, string teacherId);
    Task<ServiceResult> DeleteCourseAsync(int id, string teacherId);

    // Student Catalog
    Task<IEnumerable<Course>> SearchPublishedCoursesAsync(string? term, string? subject, string? stage);
    Task<Course?> GetPublishedCourseByIdAsync(int id);
}
