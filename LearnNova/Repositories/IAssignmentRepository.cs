using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public interface IAssignmentRepository : IGenericRepository<Assignment>
{
    Task<IEnumerable<Assignment>> GetByCourseIdAsync(int courseId);
    Task<IEnumerable<Assignment>> GetByEnrolledStudentAsync(string studentId);
    Task<Assignment?> GetByIdWithCourseAsync(int id);
    Task<int> GetSubmissionsCountAsync(int assignmentId);
}
