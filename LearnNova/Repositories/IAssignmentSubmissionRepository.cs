using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public interface IAssignmentSubmissionRepository : IGenericRepository<AssignmentSubmission>
{
    Task<AssignmentSubmission?> GetSubmissionAsync(int assignmentId, string studentId);
    Task<IEnumerable<AssignmentSubmission>> GetSubmissionsByAssignmentIdAsync(int assignmentId);
    Task<AssignmentSubmission?> GetSubmissionByIdWithAssignmentAsync(int submissionId);
}
