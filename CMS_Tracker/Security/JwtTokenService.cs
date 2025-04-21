using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CMS_Tracker.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace CMS_Tracker.Security
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(IConfiguration config, ILogger<JwtTokenService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public string GenerateToken(User user)
        {
            if (user == null)
            {
                _logger.LogWarning("Attempted to generate token for null user.");
                throw new ArgumentNullException(nameof(user));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var keyString = _config["Jwt:Secret"];
            if (string.IsNullOrEmpty(keyString))
            {
                _logger.LogError("JWT secret is not configured.");
                throw new InvalidOperationException("JWT secret is missing.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiryMinutes = int.TryParse(_config["Jwt:AccessTokenExpiryMinutes"], out var minutes)
                ? minutes : 30;

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("JWT generated for user {Email}", user.Email);

            return jwt;
        }
    }
}
