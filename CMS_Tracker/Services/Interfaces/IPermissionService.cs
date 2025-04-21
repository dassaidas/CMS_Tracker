namespace CMS_Tracker.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<List<object>> GetPermissionMatrixAsync();
    }
}
