namespace CMS_Tracker.Repositories.Interfaces
{
    
        public interface IBlacklistRepository
        {
            Task<bool> IsBlacklistedAsync(string token);
            Task AddToBlacklistAsync(string token, DateTime expiry);
        }
}
