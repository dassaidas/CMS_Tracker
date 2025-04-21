using CMS_Tracker.Models;
using CMS_Tracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS_Tracker.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly CMS_DBContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(CMS_DBContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<object>> GetPermissionMatrixAsync()
        {
            try
            {
                var roles = await _context.RoleMenus
                    .Where(rm => !_context.Menus.Any(m => m.Id == rm.MenuId==true && m.IsDeleted==true))
                    .Include(rm => rm.Menu)
                    .GroupBy(rm => rm.Role)
                    .Select(g => new
                    {
                        role = g.Key,
                        menus = g.Select(rm => rm.Menu.Name).Distinct().ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation("[PermissionService] Matrix generated for {Count} roles", roles.Count);
                return roles.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PermissionService] Failed to generate permission matrix");
                throw;
            }
        }
    }
}
