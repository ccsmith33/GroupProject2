using System.Net;
using System.Text.Json;

namespace StudentStudyAI.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Internal Server Error",
                message = "An unexpected error occurred",
                details = exception.Message,
                timestamp = DateTime.UtcNow,
                requestId = context.TraceIdentifier
            };

            switch (exception)
            {
                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        error = "Bad Request",
                        message = argEx.Message,
                        details = "Invalid argument provided",
                        timestamp = DateTime.UtcNow,
                        requestId = context.TraceIdentifier
                    };
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new
                    {
                        error = "Unauthorized",
                        message = "Access denied",
                        details = "You do not have permission to access this resource",
                        timestamp = DateTime.UtcNow,
                        requestId = context.TraceIdentifier
                    };
                    break;

                case FileNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new
                    {
                        error = "Not Found",
                        message = "The requested resource was not found",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow,
                        requestId = context.TraceIdentifier
                    };
                    break;

                case TimeoutException:
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response = new
                    {
                        error = "Request Timeout",
                        message = "The request timed out",
                        details = "Please try again later",
                        timestamp = DateTime.UtcNow,
                        requestId = context.TraceIdentifier
                    };
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
