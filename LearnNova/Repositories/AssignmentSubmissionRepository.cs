using LearnNova.Data;
using LearnNova.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

public class AssignmentSubmissionRepository : GenericRepository<AssignmentSubmission>, IAssignmentSubmissionRepository
{
    public AssignmentSubmissionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<AssignmentSubmission?> GetSubmissionAsync(int assignmentId, string studentId) =>
        await _set.FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

    public async Task<IEnumerable<AssignmentSubmission>> GetSubmissionsByAssignmentIdAsync(int assignmentId) =>
        await _set
            .Include(s => s.Student)
            .Where(s => s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

    public async Task<AssignmentSubmission?> GetSubmissionByIdWithAssignmentAsync(int submissionId) =>
        await _set
            .Include(s => s.Assignment)
            .ThenInclude(a => a.Course)
            .FirstOrDefaultAsync(s => s.Id == submissionId);
}
