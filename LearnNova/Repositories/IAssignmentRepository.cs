using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public interface IAssignmentRepository : IGenericRepository<Assignment>
{
    Task<IEnumerable<Assignment>> GetByCourseIdAsync(int courseId);
    Task<Assignment?> GetByIdWithCourseAsync(int id);
    Task<int> GetSubmissionsCountAsync(int assignmentId);
}
