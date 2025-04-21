using CMS_Tracker.Constants;
using CMS_Tracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class AdminSeeder
{
    private readonly CMS_DBContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(CMS_DBContext context, IConfiguration config, ILogger<AdminSeeder> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            var email = _config["AdminUser:Email"];
            var password = _config["AdminUser:Password"];

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = _config["AdminUser:FirstName"],
                    LastName = _config["AdminUser:LastName"],
                    Email = email,
                    Role = SystemRoles.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin user created successfully.");
            }
            else
            {
                _logger.LogInformation("Admin user already exists.");
            }

            // Assign all active + non-deleted menus to Admin role
            var allMenuIds = await _context.Menus
                .Where(m => m.IsActive==true && !m.IsDeleted == true)
                .Select(m => m.Id)
                .ToListAsync();

            var existingAssignments = await _context.RoleMenus
                .Where(rm => rm.Role == SystemRoles.Admin)
                .Select(rm => rm.MenuId)
                .ToListAsync();

            var newAssignments = allMenuIds
                .Except(existingAssignments)
                .Select(menuId => new RoleMenu
                {
                    Role = SystemRoles.Admin,
                    MenuId = menuId,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = "Seeder"
                });

            if (newAssignments.Any())
            {
                _context.RoleMenus.AddRange(newAssignments);
                await _context.SaveChangesAsync();
                _logger.LogInformation("{Count} new menu(s) assigned to Admin.", newAssignments.Count());
            }
            else
            {
                _logger.LogInformation("All menus already assigned to Admin.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed during AdminSeeder execution.");
        }
    }
}
