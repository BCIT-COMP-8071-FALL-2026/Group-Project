using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OLTPApp.Models;

namespace OLTPApp.Data;

public static class DbSeeder
{
    // Hardcoded from docs/Roles-and-Permissions.md; swap for Thuy's role constants once merged
    public static readonly string[] Roles = { "Senior", "Family Member", "Caregiver", "Admin", "Agency Manager" };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager);
        // admin, agencies, services come next
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}