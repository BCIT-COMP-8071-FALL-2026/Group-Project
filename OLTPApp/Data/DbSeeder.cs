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
        await SeedAdminAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        const string email = "admin@seniorcare.test";
        if (await userManager.FindByEmailAsync(email) != null)
            return;

        var admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        await CreateUserAsync(userManager, admin, "Admin123!", "Admin");
    }

    // Shared by admin, manager and caregiver accounts
    private static async Task CreateUserAsync(UserManager<ApplicationUser> userManager,
        ApplicationUser user, string password, string role)
    {
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Seeding {user.Email} failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(user, role);
    }
}