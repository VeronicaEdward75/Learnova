using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

// GetByIdAsync/GetAllAsync/FindAsync/AddAsync/Update/Remove/SaveChangesAsync all come from
// IGenericRepository<Course> (int-keyed convenience alias, doc 2 §2) — only Course-specific
// queries live here, same shape as IUserRepository.
public interface ICourseRepository : IGenericRepository<Course>
{
    Task<IEnumerable<Course>> GetAllWithTeacherAsync();
    Task<IEnumerable<Course>> SearchAsync(string? term, bool? isPublished);
    Task<IEnumerable<Course>> GetByTeacherIdAsync(string teacherId);
    Task<IEnumerable<Course>> SearchPublishedAsync(string? term, string? subject, string? stage);
    Task<Course?> GetPublishedByIdAsync(int id);
}
