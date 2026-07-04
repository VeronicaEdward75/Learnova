using LearnNova.Data;
using LearnNova.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

public class CourseContentRepository : GenericRepository<CourseContent>, ICourseContentRepository
{
    public CourseContentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CourseContent>> GetByCourseIdAsync(int courseId) =>
        await _set.Where(c => c.CourseId == courseId)
                  .OrderBy(c => c.OrderIndex)
                  .ToListAsync();

    public async Task<int> GetMaxOrderIndexAsync(int courseId)
    {
        var hasContent = await _set.AnyAsync(c => c.CourseId == courseId);
        if (!hasContent)
            return 0;

        return await _set.Where(c => c.CourseId == courseId).MaxAsync(c => c.OrderIndex);
    }
}
