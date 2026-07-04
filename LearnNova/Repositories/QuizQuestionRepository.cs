using LearnNova.Data;
using LearnNova.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

public class QuizQuestionRepository : GenericRepository<QuizQuestion>, IQuizQuestionRepository
{
    public QuizQuestionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<QuizQuestion>> GetByQuizIdAsync(int quizId)
    {
        return await _set
            .Where(q => q.QuizId == quizId)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();
    }

    public async Task<QuizQuestion?> GetByIdWithQuizAsync(int id)
    {
        return await _set
            .Include(q => q.Quiz)
            .ThenInclude(q => q.Course)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<int> GetMaxOrderIndexAsync(int quizId)
    {
        var questions = await _set.Where(q => q.QuizId == quizId).ToListAsync();
        if (!questions.Any())
            return 0;

        return questions.Max(q => q.OrderIndex);
    }
}
