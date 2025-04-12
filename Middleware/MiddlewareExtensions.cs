using static FlightAlright.Middleware.PermissionController;

namespace FlightAlright.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UsePermission(this IApplicationBuilder _builder, UserTypes[] _allowedUsers)
        {
            return _builder.UseMiddleware<PermissionController>(_allowedUsers);
        }
    }
}
