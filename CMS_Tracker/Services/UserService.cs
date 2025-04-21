using CMS_Tracker.DTOs;
using CMS_Tracker.Email;
using CMS_Tracker.Helpers;
using CMS_Tracker.Models;
using CMS_Tracker.Repositories.Interfaces;
using CMS_Tracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CMS_Tracker.Services
{
    public class UserService : IUserService
    {
        private readonly CMS_DBContext _context;
        private readonly IUserRepository _userRepo;
        private readonly ITokenRepository _tokenRepo;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly ILogger<UserService> _logger;

        public UserService(CMS_DBContext context, IUserRepository userRepo, IConfiguration config, ITokenRepository tokenRepo, IEmailService email, ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _tokenRepo = tokenRepo;
            _email = email;
            _config = config;
            _context = context;
            _logger = logger;
        }

        public async Task CreateUserAsync(CreateUserDto dto)
        {
            try
            {
                if (await _userRepo.UserExistsAsync(dto.Email))
                {
                    _logger.LogWarning("Attempted to create duplicate user: {Email}", dto.Email);
                    throw new Exception("User already exists.");
                }

                var tempPassword = RandomGenerator.GeneratePassword();
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Role = dto.Role, // TODO: validate against allowed roles
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = hashedPassword
                };

                await _userRepo.CreateUserAsync(user);
                var token = await _tokenRepo.CreateActivationTokenAsync(user);

                var link = $"{_config["Frontend:BaseUrl"]}/set-password?token={token.Token}";

                var placeholders = new Dictionary<string, string>
                {
                    { "UserName", $"{user.FirstName} {user.LastName}" },
                    { "TempPassword", tempPassword },
                    { "Link", link }
                };

                await _email.SendEmailAsync(user.Email, "TemporaryPassword", placeholders);
                _logger.LogInformation("Activation email sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user: {Email}", dto.Email);
                throw;
            }
        }

        public async Task SetPasswordAsync(SetPasswordDto dto)
        {
            try
            {
                var tokenEntry = await _context.UserTokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t =>
                        t.Token == dto.Token &&
                        t.Type == "Activation" &&
                        t.Expiry > DateTime.UtcNow);

                if (tokenEntry == null)
                    throw new Exception("Invalid or expired token.");

                var user = tokenEntry.User;
                if (user == null || user.IsActive != true)
                    throw new Exception("User not found or inactive.");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                _context.Users.Update(user);
                _context.UserTokens.Remove(tokenEntry);

                await _context.SaveChangesAsync();
                _logger.LogInformation("Password successfully set for user: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set password using token: {Token}", dto.Token);
                throw;
            }
        }
    }
}
