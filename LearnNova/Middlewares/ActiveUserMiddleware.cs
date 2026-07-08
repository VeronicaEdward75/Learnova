using Microsoft.AspNetCore.Identity;
using LearnNova.Models.Entities;

namespace LearnNova.Middlewares;

public class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user == null || !user.IsActive)
            {
                // Sign out locally
                await signInManager.SignOutAsync();

                // Reject the cookie and short-circuit the request by redirecting to login
                context.Response.Redirect("/Account/Login?deactivated=true");
                return;
            }
        }

        await _next(context);
    }
}
