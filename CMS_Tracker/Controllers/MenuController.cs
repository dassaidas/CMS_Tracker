using System.Net;
using System.Security.Claims;
using CMS_Tracker.DTOs;
using CMS_Tracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CMS_Tracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MenuController> _logger;

        public MenuController(IMenuService menuService, IHttpContextAccessor httpContextAccessor, ILogger<MenuController> logger)
        {
            _menuService = menuService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetMenus()
        {
            try
            {
                var role = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(role))
                {
                    _logger.LogWarning("Role not found in token for GetMenus.");
                    return Unauthorized("User role not found in token.");
                }

                var menus = await _menuService.GetMenusForRoleAsync(role);
                _logger.LogInformation("Fetched menus for role: {Role}", role);
                return Ok(menus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get menus");
                return StatusCode(500, "An error occurred while fetching menus.");
            }
        }

        [HttpGet("parents")]
        public async Task<IActionResult> GetParentMenus()
        {
            try
            {
                var parentMenus = await _menuService.GetParentMenusAsync();
                return Ok(parentMenus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch parent menus.");
                return StatusCode(500, "An error occurred while fetching parent menus.");
            }
        }

        [HttpPost("createMenu")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMenu([FromBody] MenuCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var menu = await _menuService.CreateMenuAsync(dto);
                _logger.LogInformation("Menu created by user {User}: {MenuName}", User.Identity?.Name ?? "Unknown", menu.Name);
                return Ok(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create menu: {MenuName}", dto.Name);
                return StatusCode(500, "An error occurred while creating the menu.");
            }
        }
    }
}
