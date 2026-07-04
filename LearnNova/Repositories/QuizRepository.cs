using LearnNova.Data;
using LearnNova.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
{
    public QuizRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Quiz>> GetByCourseIdAsync(int courseId)
    {
        return await _set.Where(q => q.CourseId == courseId).ToListAsync();
    }

    public async Task<Quiz?> GetByIdWithCourseAsync(int id)
    {
        return await _set.Include(q => q.Course).FirstOrDefaultAsync(q => q.Id == id);
    }
}
