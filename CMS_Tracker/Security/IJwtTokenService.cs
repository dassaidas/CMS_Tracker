using CMS_Tracker.Models;

namespace CMS_Tracker.Security
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
