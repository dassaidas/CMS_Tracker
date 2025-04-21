using CMS_Tracker.Models;
using CMS_Tracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CMS_Tracker.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    [Authorize(Roles = "Admin,HR")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        [HttpGet("matrix")]
        public async Task<IActionResult> GetPermissionMatrix()
        {
            try
            {
                var matrix = await _permissionService.GetPermissionMatrixAsync();
                return Ok(matrix);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PermissionsController] Failed to get permission matrix");
                return StatusCode(500, "An error occurred while retrieving permission matrix.");
            }
        }
    }
}
