using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public interface IEnrollmentRepository : IGenericRepository<Enrollment>
{
    Task<Enrollment?> GetByStudentAndCourseAsync(string studentId, int courseId);
    Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(string studentId);
    Task<int> GetCourseEnrollmentCountAsync(int courseId);
}
