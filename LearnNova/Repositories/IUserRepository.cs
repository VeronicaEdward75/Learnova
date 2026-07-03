using LearnNova.Models.Entities;
using LearnNova.Models.Enums;

namespace LearnNova.Repositories;

// GetByIdAsync(string), GetAllAsync, FindAsync, AddAsync, Update, Remove, SaveChangesAsync
// all come from IGenericRepository<ApplicationUser, string> — only User-specific queries live here.
public interface IUserRepository : IGenericRepository<ApplicationUser, string>
{
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<IEnumerable<ApplicationUser>> GetByRoleAsync(UserRole role);
    Task<IEnumerable<ApplicationUser>> SearchAsync(string? term, UserRole? role, bool? isActive);
    Task<int> CountByRoleAsync(UserRole role);
}
