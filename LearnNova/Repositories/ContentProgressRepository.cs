using LearnNova.Data;
using LearnNova.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

public class ContentProgressRepository : GenericRepository<ContentProgress>, IContentProgressRepository
{
    public ContentProgressRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ContentProgress?> GetProgressAsync(string studentId, int contentId) =>
        await _set.FirstOrDefaultAsync(p => p.StudentId == studentId && p.ContentId == contentId);

    public async Task<IEnumerable<ContentProgress>> GetStudentProgressInCourseAsync(string studentId, int courseId) =>
        await _set.Where(p => p.StudentId == studentId && p.Content.CourseId == courseId).ToListAsync();
}
