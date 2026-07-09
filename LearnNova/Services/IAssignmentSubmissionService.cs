using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Models.ViewModels.Teacher;
using Microsoft.AspNetCore.Http;

namespace LearnNova.Services;

public interface IAssignmentSubmissionService
{
    Task<IEnumerable<AssignmentItemViewModel>> GetStudentAssignmentsWithStatusAsync(int courseId, string studentId);
    Task<IEnumerable<AssignmentItemViewModel>> GetAllStudentAssignmentsAsync(string studentId);
    Task<LearnNova.Models.ViewModels.PagedResult<AssignmentItemViewModel>> GetAllStudentAssignmentsPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.AssignmentFilterParameters filters);
    Task<AssignmentSubmitViewModel?> GetAssignmentForSubmissionAsync(int assignmentId, string studentId);
    Task<ServiceResult> SubmitAssignmentAsync(int assignmentId, string studentId, IFormFile? file, string? textAnswer, string webRootPath);
    
    Task<IEnumerable<AssignmentSubmission>> GetSubmissionsForAssignmentAsync(int assignmentId, string teacherId);
    Task<ServiceResult> GradeSubmissionAsync(SubmissionGradeViewModel model, string teacherId);
}
