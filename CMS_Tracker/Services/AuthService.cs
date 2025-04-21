using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CMS_Tracker.Email;
using CMS_Tracker.Models;
using CMS_Tracker.Repositories.Interfaces;
using CMS_Tracker.Security;
using CMS_Tracker.DTOs;
using CMS_Tracker.Services.Interfaces;

namespace CMS_Tracker.Services
{
    public class AuthService : IAuthService
    {
        private readonly CMS_DBContext _context;
        private readonly ITokenRepository _tokenRepo;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly ILogger<IAuthService> _logger;
        private readonly IJwtTokenService _jwtService;

        public AuthService(
            CMS_DBContext context,
            ITokenRepository tokenRepo,
            IEmailService email,
            IConfiguration config,
            IJwtTokenService jwtService,
            ILogger<IAuthService> logger)
        {
            _context = context;
            _tokenRepo = tokenRepo;
            _email = email;
            _config = config;
            _logger = logger;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password?.Trim(), user.PasswordHash))
            {
                _logger.LogWarning("Invalid login attempt for email: {Email}", dto.Email);
                throw new Exception("Invalid credentials");
            }

            var jwt = _jwtService.GenerateToken(user);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                Expiry = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = jwt,
                RefreshToken = refreshToken.Token,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var tokenEntry = await _tokenRepo.GetByTokenAsync(token);
            if (tokenEntry == null)
                throw new Exception("Invalid or expired token");

            var user = await _context.Users.FindAsync(tokenEntry.UserId);
            if (user == null || user.IsActive != true)
                throw new Exception("User not found or inactive");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            await _tokenRepo.InvalidateTokenAsync(tokenEntry);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var tokenEntry = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.Expiry > DateTime.UtcNow);

            if (tokenEntry == null || tokenEntry.User == null)
                throw new Exception("Invalid or expired refresh token");

            var newAccessToken = _jwtService.GenerateToken(tokenEntry.User);
            var newRefreshToken = Guid.NewGuid().ToString();

            tokenEntry.Token = newRefreshToken;
            tokenEntry.Expiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Email = tokenEntry.User.Email,
                Role = tokenEntry.User.Role
            };
        }

        public async Task LogoutAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Invalid token");

            var blacklistedToken = new TokenBlacklist
            {
                Token = token,
                CreatedAt = DateTime.UtcNow
            };

            _context.TokenBlacklists.Add(blacklistedToken);
            await _context.SaveChangesAsync();
        }

        public async Task RequestPasswordResetAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email && x.IsActive == true);

            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent or inactive user: {Email}", email);
                return;
            }

            var oldTokens = _context.UserTokens
                .Where(t => t.UserId == user.Id && t.Type == "Reset");
            _context.UserTokens.RemoveRange(oldTokens);
            await _context.SaveChangesAsync();

            var token = await _tokenRepo.CreateResetTokenAsync(user);
            string fullResetLink = $"{_config["Frontend:BaseUrl"]}?token={token.Token}";

            var placeholders = new Dictionary<string, string>
            {
                { "UserName", $"{user.FirstName} {user.LastName}" },
                { "Link", fullResetLink }
            };

            try
            {
                await _email.SendEmailAsync(user.Email, "ResetPassword", placeholders);
                _logger.LogInformation("Password reset email sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
                throw;
            }
        }
    }
}
