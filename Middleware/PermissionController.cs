namespace FlightAlright.Middleware
{
    public class PermissionController
    {
        private readonly RequestDelegate next;
        private readonly UserTypes[] allowedUserTypes;

        public enum UserTypes
        {
            Guest,
            Client,
            Employee,
            Administrator
        }

        public PermissionController(RequestDelegate _next, UserTypes[] _allowed)
        {
            next = _next;
            allowedUserTypes = _allowed;
        }

        public async Task InvokeAsync(HttpContext _context)
        {
            UserTypes userStatus = UserTypes.Guest;
            Enum.TryParse(_context.Session.GetString("UserType"), out userStatus);

            if (!allowedUserTypes.Contains(userStatus))
            {
                //Jeśli użytkownik nie jest na liście dozwolonych
                _context.Response.Redirect("/AccessDenied");
                return;
            }

            //Jeśli ma dostęp, przejdź dalej
            await next(_context);
        }
    }
}
