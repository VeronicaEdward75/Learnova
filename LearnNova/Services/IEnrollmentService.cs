using LearnNova.Models.Entities;

namespace LearnNova.Services;

public interface IEnrollmentService
{
    Task<ServiceResult> EnrollAsync(string studentId, int courseId);
    Task<ServiceResult> UnenrollAsync(string studentId, int courseId);
    Task<IEnumerable<Enrollment>> GetStudentCoursesAsync(string studentId);
    Task<LearnNova.Models.ViewModels.PagedResult<Enrollment>> GetStudentCoursesPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.CourseFilterParameters filters);
    Task<bool> IsEnrolledAsync(string studentId, int courseId);
}

