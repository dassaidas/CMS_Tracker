using CMS_Tracker.Models;

namespace CMS_Tracker.Repositories.Interfaces
{
    public interface ITokenRepository
    {
        Task<UserToken> CreateResetTokenAsync(User user);
        Task<UserToken?> GetByTokenAsync(string token);
        Task InvalidateTokenAsync(UserToken token);
        Task<UserToken> CreateActivationTokenAsync(User user);
    }
}
