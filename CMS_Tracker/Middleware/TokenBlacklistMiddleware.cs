using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using CMS_Tracker.Repositories.Interfaces;

namespace CMS_Tracker.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenBlacklistMiddleware> _logger;

        public TokenBlacklistMiddleware(RequestDelegate next, ILogger<TokenBlacklistMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IBlacklistRepository blacklistRepo)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Optional: Skip check for logout, refresh-token, swagger, favicon
            if (path is not null && (
                path.Contains("/api/auth/logout") ||
                path.Contains("/api/auth/refresh-token") ||
                path.Contains("/swagger") ||
                path.Contains("/favicon.ico")))
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var isBlacklisted = await blacklistRepo.IsBlacklistedAsync(token);
                if (isBlacklisted)
                {
                    _logger.LogWarning("Blocked request with blacklisted token.");

                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"code\":401,\"message\":\"Unauthorized\"}");
                    return;
                }
            }

            await _next(context);
        }
    }
}
