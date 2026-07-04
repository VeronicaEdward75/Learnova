using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher;

namespace LearnNova.Services;

public interface ICourseContentService
{
    Task<IEnumerable<CourseContent>> GetContentsByCourseAsync(int courseId, string teacherId);
    Task<CourseContent?> GetContentByIdAsync(int id, string teacherId);
    Task<ServiceResult> AddContentAsync(int courseId, CourseContentFormViewModel model, string teacherId);
    Task<ServiceResult> UpdateContentAsync(int id, CourseContentFormViewModel model, string teacherId);
    Task<ServiceResult> DeleteContentAsync(int id, string teacherId);
    Task<ServiceResult> ReorderContentAsync(int courseId, List<int> orderedContentIds, string teacherId);
}
