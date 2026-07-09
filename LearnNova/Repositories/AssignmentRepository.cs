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

    public async Task<IEnumerable<Assignment>> GetByEnrolledStudentAsync(string studentId) =>
        await _set
            .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
            .Where(a => _context.Set<Enrollment>().Any(e => e.StudentId == studentId && e.CourseId == a.CourseId && e.IsActive))
            .AsNoTracking()
            .OrderByDescending(a => a.DueDate)
            .ToListAsync();

    public async Task<LearnNova.Models.ViewModels.PagedResult<LearnNova.Models.ViewModels.Student.AssignmentItemViewModel>> GetStudentAssignmentsPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.AssignmentFilterParameters filters)
    {
        var query = _set
            .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
            .Where(a => _context.Set<Enrollment>().Any(e => e.StudentId == studentId && e.CourseId == a.CourseId && e.IsActive))
            .Select(a => new LearnNova.Models.ViewModels.Student.AssignmentItemViewModel
            {
                Assignment = a,
                Submission = _context.Set<AssignmentSubmission>().FirstOrDefault(s => s.AssignmentId == a.Id && s.StudentId == studentId)
            });

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            query = query.Where(x => x.Assignment.Title.Contains(filters.SearchTerm) || (x.Assignment.Course != null && x.Assignment.Course.Title.Contains(filters.SearchTerm)));
        }

        if (filters.StartDate.HasValue)
        {
            query = query.Where(x => x.Assignment.DueDate >= filters.StartDate.Value);
        }

        if (filters.EndDate.HasValue)
        {
            query = query.Where(x => x.Assignment.DueDate <= filters.EndDate.Value);
        }
        
        var now = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            query = filters.Status switch
            {
                "pending" => query.Where(x => x.Submission == null && x.Assignment.DueDate >= now),
                "submitted" => query.Where(x => x.Submission != null && !x.Submission.Grade.HasValue),
                "graded" => query.Where(x => x.Submission != null && x.Submission.Grade.HasValue),
                "late" => query.Where(x => x.Submission != null && x.Submission.SubmittedAt > x.Assignment.DueDate),
                "missing" => query.Where(x => x.Submission == null && x.Assignment.DueDate < now),
                _ => query
            };
        }

        query = filters.SortBy switch
        {
            "oldest" => query.OrderBy(x => x.Assignment.DueDate),
            "name_asc" => query.OrderBy(x => x.Assignment.Title),
            "name_desc" => query.OrderByDescending(x => x.Assignment.Title),
            _ => query.OrderByDescending(x => x.Assignment.DueDate)
        };

        return await LearnNova.Extensions.QueryableExtensions.ToPagedResultAsync(query, filters);
    }

    public async Task<Assignment?> GetByIdWithCourseAsync(int id) =>
        await _set
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<int> GetSubmissionsCountAsync(int assignmentId) =>
        await _context.Set<AssignmentSubmission>()
            .CountAsync(s => s.AssignmentId == assignmentId);
}
