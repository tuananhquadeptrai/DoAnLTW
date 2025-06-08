using Microsoft.AspNetCore.Identity;
using VAYTIEN.Models;

namespace VAYTIEN.Extensions;

public class ForceLogoutMiddleware
{
    private readonly RequestDelegate _next;

    public ForceLogoutMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context,
                             SignInManager<ApplicationUser> signInManager,
                             StartupTrackerService tracker)
    {
        if (!tracker.HasForcedLogout() && context.User.Identity.IsAuthenticated)
        {
            await signInManager.SignOutAsync();
            tracker.MarkAsLoggedOut();
            context.Response.Redirect("/Account/Login?logout=true");
            return;
        }

        await _next(context);
    }
}
