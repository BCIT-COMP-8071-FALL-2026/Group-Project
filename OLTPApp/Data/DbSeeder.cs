using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OLTPApp.Authorization;
using OLTPApp.Models;

namespace OLTPApp.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager);
        await SeedAgenciesAsync(db, userManager);
        await SeedServicesAsync(db);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        const string email = "admin@seniorcare.test";
        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            // Heals a partial seed where the user was created but the role add failed
            if (!await userManager.IsInRoleAsync(existing, RoleNames.Admin))
            {
                var roleResult = await userManager.AddToRoleAsync(existing, RoleNames.Admin);
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"Adding {email} to {RoleNames.Admin} failed: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
            return;
        }

        var admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        await CreateUserAsync(userManager, admin, "Admin123!", RoleNames.Admin);
    }

    private static async Task SeedAgenciesAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        var seeds = new[]
        {
            (Slug: "sunrise", Agency: new Agency { Name = "Sunrise Home Care", Street = "100 Main St", City = "Vancouver",
                Province = "BC", Country = "Canada", PostalCode = "V5K 0A1", Phone = "604-555-0100", Email = "info@sunrise.test" },
             Manager: ("Maria", "Lopez"), Caregiver: ("Daniel", "Kim")),
            (Slug: "harbour", Agency: new Agency { Name = "Harbour Care Services", Street = "200 Water St", City = "Burnaby",
                Province = "BC", Country = "Canada", PostalCode = "V5H 0A2", Phone = "604-555-0200", Email = "info@harbour.test" },
             Manager: ("Priya", "Singh"), Caregiver: ("Liam", "Chen")),
        };

        foreach (var s in seeds)
        {
            var agency = await db.Agency.FirstOrDefaultAsync(a => a.Name == s.Agency.Name);
            if (agency == null)
            {
                agency = s.Agency;
                db.Agency.Add(agency);
                await db.SaveChangesAsync();
            }

            await SeedStaffAsync(db, userManager, agency, $"manager.{s.Slug}@seniorcare.test", RoleNames.AgencyManager, s.Manager);
            await SeedStaffAsync(db, userManager, agency, $"caregiver.{s.Slug}@seniorcare.test", RoleNames.Caregiver, s.Caregiver);
        }
    }

    // Person + AgencyManager/Caregiver row + login linked by PersonID, all or nothing
    private static async Task SeedStaffAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
        Agency agency, string email, string role, (string First, string Last) name)
    {
        if (await userManager.FindByEmailAsync(email) != null)
            return;

        await using var tx = await db.Database.BeginTransactionAsync();

        var person = new Person { FirstName = name.First, LastName = name.Last, Street = agency.Street, City = agency.City,
            Province = agency.Province, Country = agency.Country, PostalCode = agency.PostalCode, Phone = agency.Phone };

        if (role == RoleNames.AgencyManager)
            db.AgencyManager.Add(new AgencyManager { Person = person, Agency = agency });
        else
            db.Caregiver.Add(new Caregiver { Person = person, Agency = agency });
        await db.SaveChangesAsync();   // fills person.PersonID

        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, PersonID = person.PersonID };
        await CreateUserAsync(userManager, user, "Password123!", role);

        await tx.CommitAsync();
    }

    private static async Task SeedServicesAsync(ApplicationDbContext db)
    {
        string[] names = { "Doctor", "Home visit", "Bathing" };   // OLTP-16

        foreach (var name in names)
        {
            if (!await db.Service.AnyAsync(s => s.ServiceName == name))
                db.Service.Add(new Service { ServiceName = name });
        }
        await db.SaveChangesAsync();
    }

    // Shared by admin, manager and caregiver accounts
    private static async Task CreateUserAsync(UserManager<ApplicationUser> userManager,
        ApplicationUser user, string password, string role)
    {
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Seeding {user.Email} failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException($"Adding {user.Email} to {role} failed: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
    }
}