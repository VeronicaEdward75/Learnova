using LearnNova.Data;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

// Inherits GetByIdAsync, FindAsync, AddAsync, Update, Remove, SaveChangesAsync as-is from
// GenericRepository<ApplicationUser, string>; only overrides GetAllAsync (for ordering) and
// adds the User-specific query methods IUserRepository declares on top of the generic contract.
public class UserRepository : GenericRepository<ApplicationUser, string>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<ApplicationUser>> GetAllAsync() =>
        await _set.OrderByDescending(u => u.CreatedAt).ToListAsync();

    public Task<ApplicationUser?> GetByEmailAsync(string email) =>
        _set.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IEnumerable<ApplicationUser>> GetByRoleAsync(UserRole role) =>
        await _set.Where(u => u.Role == role).OrderByDescending(u => u.CreatedAt).ToListAsync();

    public async Task<IEnumerable<ApplicationUser>> SearchAsync(string? term, UserRole? role, bool? isActive)
    {
        var query = _set.AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(u => u.FullName.Contains(term) || u.Email!.Contains(term));
        }

        if (role is not null)
        {
            query = query.Where(u => u.Role == role);
        }

        if (isActive is not null)
        {
            query = query.Where(u => u.IsActive == isActive);
        }

        return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public Task<int> CountByRoleAsync(UserRole role) =>
        _set.CountAsync(u => u.Role == role);
}
