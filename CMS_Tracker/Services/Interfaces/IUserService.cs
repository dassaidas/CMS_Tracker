using CMS_Tracker.DTOs;

namespace CMS_Tracker.Services.Interfaces
{
    public interface IUserService
    {
        Task CreateUserAsync(CreateUserDto dto);
        Task SetPasswordAsync(SetPasswordDto dto);
    }
}
