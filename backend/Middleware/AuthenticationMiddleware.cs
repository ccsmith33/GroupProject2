using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace StudentStudyAI.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // TODO: Implement authentication middleware
            // - JWT token validation
            // - User context setup
            // - Authorization checks
            await _next(context);
        }
    }
}
