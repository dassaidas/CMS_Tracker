using CMS_Tracker.DTOs;
using CMS_Tracker.Models;

namespace CMS_Tracker.Services.Interfaces
{
    public interface IMenuService
    {
        Task<List<MenuDto>> GetMenusForRoleAsync(string role);
        Task<MenuDto> CreateMenuAsync(MenuCreateDto dto);
        Task<List<MenuDropdownDto>> GetParentMenusAsync();
    }
}