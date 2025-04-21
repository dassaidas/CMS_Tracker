using CMS_Tracker.Models;
using CMS_Tracker.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS_Tracker.Repositories
{
    public class BlacklistRepository : IBlacklistRepository
    {
        private readonly CMS_DBContext _context;
        private readonly ILogger<BlacklistRepository> _logger;

        public BlacklistRepository(CMS_DBContext context, ILogger<BlacklistRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsBlacklistedAsync(string token)
        {
            var isBlacklisted = await _context.TokenBlacklists.AnyAsync(b => b.Token == token);
            if (isBlacklisted)
            {
                _logger.LogInformation("Token is blacklisted.");
            }
            return isBlacklisted;
        }

        public async Task AddToBlacklistAsync(string token, DateTime expiry)
        {
            var alreadyBlacklisted = await _context.TokenBlacklists.AnyAsync(b => b.Token == token);
            if (alreadyBlacklisted)
            {
                _logger.LogWarning("Attempted to blacklist a token that already exists.");
                return;
            }

            var entry = new TokenBlacklist
            {
                Token = token,
                ExpiredAt = expiry
            };

            _context.TokenBlacklists.Add(entry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Token successfully blacklisted. Expiry: {Expiry}", expiry);
        }
    }
}
