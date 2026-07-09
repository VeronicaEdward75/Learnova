using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public interface IAssignmentRepository : IGenericRepository<Assignment>
{
    Task<IEnumerable<Assignment>> GetByCourseIdAsync(int courseId);
    Task<IEnumerable<Assignment>> GetByEnrolledStudentAsync(string studentId);
    Task<LearnNova.Models.ViewModels.PagedResult<LearnNova.Models.ViewModels.Student.AssignmentItemViewModel>> GetStudentAssignmentsPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.AssignmentFilterParameters filters);
    Task<Assignment?> GetByIdWithCourseAsync(int id);
    Task<int> GetSubmissionsCountAsync(int assignmentId);
}
