using System.Net;
using System.Text.Json;

namespace CMS_Tracker.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context); // proceed to the next middleware
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            // Default to 500 Internal Server Error
            response.StatusCode = ex switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var env = context.RequestServices.GetService<IWebHostEnvironment>();
            var result = JsonSerializer.Serialize(new
            {
                code = response.StatusCode,
                message = "An unexpected error occurred.",
                detail = env?.IsDevelopment() == true ? ex.Message : null
            });

            return response.WriteAsync(result);
        }
    }
}

