using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels;

namespace LearnNova.Services;

public enum LoginOutcome
{
    Succeeded,
    InvalidCredentials,
    PendingApproval,
    AccountDeactivated
}

public record AuthResult(LoginOutcome Outcome, ApplicationUser? User);

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password, bool rememberMe);
    Task<ServiceResult> RegisterAsync(RegisterViewModel model);
    Task LogoutAsync();
}
