public static class ForceLogoutMiddlewareExtensions
{
    public static IApplicationBuilder UseForceLogoutOnStartup(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ForceLogoutMiddleware>();
    }
}
