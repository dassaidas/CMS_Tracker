using CMS_Tracker.DTOs;

namespace CMS_Tracker.Services.Interfaces
{
    public interface IAuthService
    {
        Task ResetPasswordAsync(string token, string newPassword);
        Task<AuthResponse> LoginAsync(LoginDto dto);
        Task RequestPasswordResetAsync(string email);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string token);
    }
}
