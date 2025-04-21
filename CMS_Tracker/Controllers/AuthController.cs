using CMS_Tracker.Constants;
using CMS_Tracker.DTOs;
using CMS_Tracker.Models;
using CMS_Tracker.Repositories.Interfaces;
using CMS_Tracker.Security;
using CMS_Tracker.Services;
using CMS_Tracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace CMS_Tracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly CMS_DBContext _context;
        private readonly IJwtTokenService _jwt;
        private readonly IAuthService _auth;
        private readonly IUserService _userService;
        private readonly IBlacklistRepository _blacklistRepo;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            CMS_DBContext context,
            IJwtTokenService jwt,
            IAuthService auth,
            IUserService userService,
            IBlacklistRepository blacklistRepo,
            ILogger<AuthController> logger)
        {
            _context = context;
            _jwt = jwt;
            _auth = auth;
            _blacklistRepo = blacklistRepo;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _auth.LoginAsync(dto);
            _logger.LogInformation("Login successful for user: {Email}", dto.Email);
            return Ok(response);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _auth.RequestPasswordResetAsync(dto.Email);
            _logger.LogInformation("Forgot password requested for: {Email}", dto.Email);
            return Ok("Password reset link sent if the email exists.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _auth.ResetPasswordAsync(dto.Token, dto.NewPassword);
            _logger.LogInformation("Password reset success for token: {Token}", dto.Token);
            return Ok("Password has been updated.");
        }

        [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.HR)]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _userService.CreateUserAsync(dto);
            _logger.LogInformation("User created: {Email}", dto.Email);
            return Ok("User created and temporary password sent.");
        }

        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _userService.SetPasswordAsync(dto);
            _logger.LogInformation("Set password success for token: {Token}", dto.Token);
            return Ok("Password has been set. You can now log in.");
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token not found in header.");
                }

                var exp = DateTime.UtcNow.AddMinutes(15); // fallback

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var expUnix = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

                if (long.TryParse(expUnix, out var expSeconds))
                {
                    exp = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
                }

                await _blacklistRepo.AddToBlacklistAsync(token, exp);

                _logger.LogInformation("User logged out. Token blacklisted.");
                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed.");
                return StatusCode(500, "An error occurred during logout.");
            }
        }
    }
}
