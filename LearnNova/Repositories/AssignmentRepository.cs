using LearnNova.Data;
using LearnNova.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

public class AssignmentRepository : GenericRepository<Assignment>, IAssignmentRepository
{
    public AssignmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Assignment>> GetByCourseIdAsync(int courseId) =>
        await _set
            .Where(a => a.CourseId == courseId)
            .AsNoTracking()
            .OrderByDescending(a => a.DueDate)
            .ToListAsync();

    public async Task<Assignment?> GetByIdWithCourseAsync(int id) =>
        await _set
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<int> GetSubmissionsCountAsync(int assignmentId) =>
        await _context.Set<AssignmentSubmission>()
            .CountAsync(s => s.AssignmentId == assignmentId);
}
