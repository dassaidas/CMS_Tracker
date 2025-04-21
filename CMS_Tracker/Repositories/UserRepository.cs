using CMS_Tracker.Models;
using CMS_Tracker.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS_Tracker.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CMS_DBContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(CMS_DBContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            _logger.LogInformation("Checked if user exists: {Email} - Result: {Exists}", email, exists);
            return exists;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("New user created: {Email}", user.Email);
            return user;
        }
    }
}
