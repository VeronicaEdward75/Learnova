using LearnNova.Models.Entities;
using LearnNova.Models.Enums;

namespace LearnNova.Services;

public record ServiceResult(bool Succeeded, IEnumerable<string> Errors)
{
    public static ServiceResult Success() => new(true, []);
    public static ServiceResult Fail(IEnumerable<string> errors) => new(false, errors);
    public static ServiceResult Fail(string error) => new(false, [error]);
}

public interface IUserService
{
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(UserRole role);
    Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string? term, UserRole? role, bool? isActive);
    Task<ApplicationUser?> GetUserByIdAsync(string id);
    Task<bool> EmailExistsAsync(string email);
    Task<ServiceResult> CreateUserAsync(ApplicationUser user, string password, UserRole role, bool isActive = true);
    Task<ServiceResult> SetActiveStatusAsync(string id, bool isActive);
}
