using CMS_Tracker.Models;
using CMS_Tracker.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS_Tracker.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly CMS_DBContext _context;
        private readonly ILogger<TokenRepository> _logger;
        private readonly IConfiguration _config;

        public TokenRepository(CMS_DBContext context, ILogger<TokenRepository> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public async Task<UserToken> CreateResetTokenAsync(User user)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            int expiryHours = int.TryParse(_config["TokenExpiry:ResetHours"], out var hours) ? hours : 24;

            var userToken = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = token,
                Type = "Reset",
                Expiry = DateTime.UtcNow.AddHours(expiryHours),
                CreatedAt = DateTime.UtcNow
            };

            _context.UserTokens.Add(userToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reset token created for user {Email}", user.Email);
            return userToken;
        }

        public async Task<UserToken?> GetByTokenAsync(string token)
        {
            var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(t => t.Token == token && t.Type == "Reset" && t.Expiry > DateTime.UtcNow);

            if (userToken == null)
            {
                _logger.LogWarning("Reset token not found or expired.");
            }

            return userToken;
        }

        public async Task InvalidateTokenAsync(UserToken token)
        {
            _context.UserTokens.Remove(token);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Token invalidated for user {UserId}", token.UserId);
        }

        public async Task<UserToken> CreateActivationTokenAsync(User user)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            int expiryHours = int.TryParse(_config["TokenExpiry:ActivationHours"], out var hours) ? hours : 24;

            var userToken = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = token,
                Type = "Activation",
                Expiry = DateTime.UtcNow.AddHours(expiryHours),
                CreatedAt = DateTime.UtcNow
            };

            _context.UserTokens.Add(userToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Activation token created for user {Email}", user.Email);
            return userToken;
        }
    }
}
