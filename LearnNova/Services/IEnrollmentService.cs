using LearnNova.Models.Entities;

namespace LearnNova.Services;

public interface IEnrollmentService
{
    Task<ServiceResult> EnrollAsync(string studentId, int courseId);
    Task<ServiceResult> UnenrollAsync(string studentId, int courseId);
    Task<IEnumerable<Enrollment>> GetStudentCoursesAsync(string studentId);
    Task<bool> IsEnrolledAsync(string studentId, int courseId);
}

