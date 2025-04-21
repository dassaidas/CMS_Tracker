using CMS_Tracker.DTOs;
using CMS_Tracker.Models;
using CMS_Tracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS_Tracker.Services
{
    public class MenuService : IMenuService
    {
        private readonly CMS_DBContext _context;
        private readonly ILogger<MenuService> _logger;

        public MenuService(CMS_DBContext context, ILogger<MenuService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MenuDto> CreateMenuAsync(MenuCreateDto dto)
        {
            try
            {
                var menu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name?.Trim(),
                    Path = dto.Path?.Trim(),
                    Icon = dto.Icon?.Trim(),
                    ParentId = dto.ParentId,
                    OrderIndex = dto.OrderIndex,
                    IsActive = dto.IsActive,
                    IsDeleted = false,
                    Type = dto.Type?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Menus.Add(menu);

                // Assign to Admin role by default
                _context.RoleMenus.Add(new RoleMenu
                {
                    Role = "Admin", // TODO: Replace with SystemRoles.Admin constant
                    MenuId = menu.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = "Seeder" // Should be replaced with actual user context if available
                });

                await _context.SaveChangesAsync();
                _logger.LogInformation("Menu created successfully: {Name}", menu.Name);

                return new MenuDto
                {
                    Id = menu.Id,
                    Name = menu.Name,
                    Path = menu.Path,
                    Icon = menu.Icon,
                    ParentId = menu.ParentId,
                    OrderIndex = menu.OrderIndex ?? 0,
                    IsActive = menu.IsActive,
                    Type = menu.Type,
                    Children = new List<MenuDto>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create menu: {Menu}", dto.Name);
                throw;
            }
        }

        public async Task<List<MenuDto>> GetMenusForRoleAsync(string role)
        {
            try
            {
                var rootMenus = await _context.Menus
                    .Include(m => m.RoleMenus)
                    .Where(m =>
                        m.IsActive == true &&
                        !m.IsDeleted == true &&
                        m.ParentId == null &&
                        m.RoleMenus.Any(rm => rm.Role == role))
                    .OrderBy(m => m.OrderIndex)
                    .ToListAsync();

                var result = new List<MenuDto>();
                foreach (var menu in rootMenus)
                {
                    var dto = await MapMenuAsync(menu, role);
                    result.Add(dto);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get menus for role: {Role}", role);
                throw;
            }
        }

        public async Task<List<MenuDropdownDto>> GetParentMenusAsync()
        {
            try
            {
                return await _context.Menus
                    .Where(m => m.ParentId == null && m.IsActive==true && !m.IsDeleted==true)
                    .OrderBy(m => m.OrderIndex)
                    .Select(m => new MenuDropdownDto
                    {
                        Id = m.Id,
                        Name = m.Name
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load parent menus");
                throw;
            }
        }

        private async Task<MenuDto> MapMenuAsync(Menu menu, string role)
        {
            var children = await _context.Menus
                .Include(m => m.RoleMenus)
                .Where(m =>
                    m.IsActive == true &&
                    !m.IsDeleted ==true &&
                    m.ParentId == menu.Id &&
                    m.RoleMenus.Any(rm => rm.Role == role))
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();

            return new MenuDto
            {
                Id = menu.Id,
                Name = menu.Name,
                Path = menu.Path,
                Icon = menu.Icon,
                ParentId = menu.ParentId,
                OrderIndex = menu.OrderIndex ?? 0,
                IsActive = menu.IsActive,
                Type = menu.Type,
                Children = children.Select(c => new MenuDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Path = c.Path,
                    Icon = c.Icon,
                    ParentId = c.ParentId,
                    OrderIndex = c.OrderIndex ?? 0,
                    IsActive = c.IsActive,
                    Type = c.Type,
                    Children = new List<MenuDto>()
                }).ToList()
            };
        }
    }
}
