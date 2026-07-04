using LearnNova.Data;
using LearnNova.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

// Inherits GenericRepository<Course> (int-keyed alias) exactly the way UserRepository inherits
// its base — see Document/02-Repository-Service-Pattern.md §6.
public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Course>> GetAllWithTeacherAsync() =>
        await _set.Include(c => c.Teacher).AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync();

    public async Task<IEnumerable<Course>> SearchAsync(string? term, bool? isPublished)
    {
        var query = _set.Include(c => c.Teacher).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(c => c.Title.Contains(term) || c.Teacher.FullName.Contains(term));
        }

        if (isPublished is not null)
        {
            query = query.Where(c => c.IsPublished == isPublished);
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetByTeacherIdAsync(string teacherId) =>
        await _set.Where(c => c.TeacherId == teacherId)
                  .OrderByDescending(c => c.CreatedAt)
                  .ToListAsync();

    public async Task<IEnumerable<Course>> SearchPublishedAsync(string? term, string? subject, string? stage)
    {
        var query = _set
            .Include(c => c.Teacher)
            .Where(c => c.IsPublished)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(c => c.Title.Contains(term) || c.Teacher.FullName.Contains(term));

        if (!string.IsNullOrWhiteSpace(subject))
            query = query.Where(c => c.Subject == subject);

        if (!string.IsNullOrWhiteSpace(stage))
            query = query.Where(c => c.Stage == stage);

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<Course?> GetPublishedByIdAsync(int id) =>
        await _set
            .Include(c => c.Teacher)
            .Include(c => c.Contents)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsPublished);
}
