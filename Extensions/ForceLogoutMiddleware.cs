using Microsoft.AspNetCore.Identity;
using VAYTIEN.Models;

public class ForceLogoutMiddleware
{
    private readonly RequestDelegate _next;
    public static bool IsFirstStartup = true;

    public ForceLogoutMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, SignInManager<ApplicationUser> signInManager)
    {
        if (IsFirstStartup && context.User.Identity.IsAuthenticated)
        {
            await signInManager.SignOutAsync();
        }

        IsFirstStartup = false; 
        await _next(context);
    }
}
