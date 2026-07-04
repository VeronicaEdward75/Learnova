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

    public async Task<int> GetCourseEnrollmentCountAsync(int courseId) =>
        await _set.CountAsync(e => e.CourseId == courseId && e.IsActive);
}
