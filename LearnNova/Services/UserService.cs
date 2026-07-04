using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Repositories;
using LearnNova.Services.DateTimeService;
using Microsoft.AspNetCore.Identity;

namespace LearnNova.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDateTimeService _dateTimeService;

    public UserService(IUserRepository userRepository, UserManager<ApplicationUser> userManager, IDateTimeService dateTimeService)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _dateTimeService = dateTimeService;
    }

    public Task<IEnumerable<ApplicationUser>> GetAllUsersAsync() => _userRepository.GetAllAsync();

    public Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(UserRole role) =>
        _userRepository.GetByRoleAsync(role);

    public Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string? term, UserRole? role, bool? isActive) =>
        _userRepository.SearchAsync(term, role, isActive);

    public Task<ApplicationUser?> GetUserByIdAsync(string id) => _userRepository.GetByIdAsync(id);

    public async Task<bool> EmailExistsAsync(string email) => await _userRepository.GetByEmailAsync(email) is not null;

    public async Task<ServiceResult> CreateUserAsync(ApplicationUser user, string password, UserRole role, bool isActive = true)
    {
        user.Role = role;
        user.UserName = user.Email;
        user.CreatedAt = _dateTimeService.UtcNow();
        user.IsActive = isActive;

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return ServiceResult.Fail(result.Errors.Select(e => e.Description));
        }

        await _userManager.AddToRoleAsync(user, role.ToString());
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetActiveStatusAsync(string id, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return ServiceResult.Fail("المستخدم غير موجود");
        }

        user.IsActive = isActive;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }
}
