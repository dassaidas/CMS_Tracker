using CMS_Tracker.Models;

namespace CMS_Tracker.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> UserExistsAsync(string email);
        Task<User> CreateUserAsync(User user);
    }
}
