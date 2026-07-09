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

    public async Task<Dictionary<int, int>> GetCompletedContentCountsByCoursesAsync(string studentId, IEnumerable<int> courseIds)
    {
        var courseIdList = courseIds.Distinct().ToList();
        if (courseIdList.Count == 0)
            return new Dictionary<int, int>();

        return await _set
            .AsNoTracking()
            .Where(p => p.StudentId == studentId && p.IsCompleted && courseIdList.Contains(p.Content.CourseId))
            .GroupBy(p => p.Content.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);
    }
}
