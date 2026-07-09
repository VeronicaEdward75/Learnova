using LearnNova.Data;
using LearnNova.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(string studentId, int courseId) =>
        await _set.FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

    public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(string studentId) =>
        await _set
            .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
            .Where(e => e.StudentId == studentId && e.IsActive)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

    public async Task<LearnNova.Models.ViewModels.PagedResult<Enrollment>> GetStudentEnrollmentsPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.CourseFilterParameters filters)
    {
        var query = _set
            .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
            .Where(e => e.StudentId == studentId && e.IsActive);

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            query = query.Where(e => e.Course != null && (e.Course.Title.Contains(filters.SearchTerm) || (e.Course.Teacher != null && e.Course.Teacher.FullName != null && e.Course.Teacher.FullName.Contains(filters.SearchTerm))));
        }
        
        if (filters.StartDate.HasValue)
        {
            query = query.Where(e => e.EnrolledAt >= filters.StartDate.Value);
        }
        
        if (filters.EndDate.HasValue)
        {
            query = query.Where(e => e.EnrolledAt <= filters.EndDate.Value);
        }

        query = filters.SortBy switch
        {
            "oldest" => query.OrderBy(e => e.EnrolledAt),
            "name_asc" => query.OrderBy(e => e.Course!.Title),
            "name_desc" => query.OrderByDescending(e => e.Course!.Title),
            _ => query.OrderByDescending(e => e.EnrolledAt)
        };

        return await LearnNova.Extensions.QueryableExtensions.ToPagedResultAsync(query, filters);
    }

    public async Task<int> GetCourseEnrollmentCountAsync(int courseId) =>
        await _set.CountAsync(e => e.CourseId == courseId && e.IsActive);
}
