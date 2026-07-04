using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher;

namespace LearnNova.Services;

public interface IAssignmentService
{
    Task<IEnumerable<Assignment>> GetAssignmentsByCourseAsync(int courseId, string teacherId);
    Task<Assignment?> GetAssignmentByIdAsync(int id, string teacherId);
    Task<ServiceResult> CreateAssignmentAsync(int courseId, AssignmentFormViewModel model, string teacherId);
    Task<ServiceResult> UpdateAssignmentAsync(int id, AssignmentFormViewModel model, string teacherId);
    Task<ServiceResult> DeleteAssignmentAsync(int id, string teacherId);
}
