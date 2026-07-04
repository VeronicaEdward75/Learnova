using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public interface IQuizQuestionRepository : IGenericRepository<QuizQuestion>
{
    Task<IEnumerable<QuizQuestion>> GetByQuizIdAsync(int quizId);
    Task<QuizQuestion?> GetByIdWithQuizAsync(int id);
    Task<int> GetMaxOrderIndexAsync(int quizId);
}
