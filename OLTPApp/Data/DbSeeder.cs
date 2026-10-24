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
        await SeedDemoDataAsync(db, userManager);
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

    // #56: seniors, family, a no-role Staff login and visits for the OLTP pages, all or nothing.
    // Visit times are UTC offsets from seed time, so reset the database volume to refresh them (DOCKER_SETUP.md).
    private static async Task SeedDemoDataAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync("senior.wilson@seniorcare.test") != null)
            return;

        var now = DateTime.UtcNow;
        await using var tx = await db.Database.BeginTransactionAsync();

        var seniors = await SeedSeniorsAsync(db, userManager, now);
        await SeedFamilyAsync(db, userManager, seniors["wilson"]);

        // Staff waiting on Admin to assign a role (OLTP-2, OLTP-4)
        var staff = NewPerson("Nina", "Patel", "88 Kingsway", "Vancouver", "V5T 3H9", "604-555-0130");
        db.Person.Add(staff);
        await SeedLoginAsync(db, userManager, "staff.patel@seniorcare.test", staff, null);

        await SeedVisitsAsync(db, userManager, seniors, now);

        await tx.CommitAsync();
    }

    // DOBs cover all four OLAP age bands; Davis keeps the optional fields empty for #60
    private static async Task<Dictionary<string, Senior>> SeedSeniorsAsync(ApplicationDbContext db,
        UserManager<ApplicationUser> userManager, DateTime now)
    {
        var seeds = new[]
        {
            (Slug: "wilson", Person: NewPerson("Margaret", "Wilson", "1450 W 41st Ave", "Vancouver", "V6M 1X9", "604-555-0111"),
             Senior: new Senior { DateOfBirth = new DateOnly(1941, 3, 12), IsLivingAlone = false,
                CurrentDoctorName = "Dr. Alan Fraser", CurrentDoctorPhone = "604-555-0190",
                MedicalConditions = "Type 2 diabetes, mild arthritis", CurrentMedication = "Metformin 500 mg twice daily",
                EmergencyContactName = "Susan Wilson", EmergencyContactPhone = "604-555-0121", EmergencyContactRelationship = "Daughter" }),
            (Slug: "brown", Person: NewPerson("Harold", "Brown", "3021 Fraser St", "Vancouver", "V5V 4B6", "604-555-0112"),
             Senior: new Senior { DateOfBirth = new DateOnly(1950, 7, 4), IsLivingAlone = true,
                CurrentDoctorName = "Dr. Grace Wong", CurrentDoctorPhone = "604-555-0191",
                MedicalConditions = "Hypertension", CurrentMedication = "Amlodipine 5 mg daily",
                EmergencyContactName = "Peter Brown", EmergencyContactPhone = "604-555-0122", EmergencyContactRelationship = "Son" }),
            (Slug: "davis", Person: NewPerson("Evelyn", "Davis", "5120 Imperial St", "Burnaby", "V5J 1E3", "604-555-0113"),
             Senior: new Senior { DateOfBirth = new DateOnly(1963, 11, 20), IsLivingAlone = true }),
            (Slug: "taylor", Person: NewPerson("George", "Taylor", "6650 Royal Oak Ave", "Burnaby", "V5H 3P5", "604-555-0114"),
             Senior: new Senior { DateOfBirth = new DateOnly(1957, 2, 8), IsLivingAlone = false,
                MedicalConditions = "COPD", CurrentMedication = "Tiotropium inhaler daily" }),
            (Slug: "clark", Person: NewPerson("Ruth", "Clark", "2275 E 8th Ave", "Vancouver", "V5N 1V5", "604-555-0115"),
             Senior: new Senior { DateOfBirth = new DateOnly(1945, 9, 30), IsLivingAlone = true,
                MedicalConditions = "Osteoporosis" }),
            (Slug: "hall", Person: NewPerson("Walter", "Hall", "4388 Hastings St", "Burnaby", "V5C 2J9", "604-555-0116"),
             Senior: new Senior { DateOfBirth = new DateOnly(1939, 5, 17), IsLivingAlone = false,
                Status = SeniorStatus.Deactivated, DeactivatedAt = now.AddDays(-10) }),
        };

        var seniors = new Dictionary<string, Senior>();
        foreach (var s in seeds)
        {
            s.Senior.Person = s.Person;
            db.Senior.Add(s.Senior);
            var user = await SeedLoginAsync(db, userManager, $"senior.{s.Slug}@seniorcare.test", s.Person, RoleNames.Senior);

            // A deactivated senior cannot log in (OLTP-9)
            if (s.Senior.Status == SeniorStatus.Deactivated)
            {
                var lockResult = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                if (!lockResult.Succeeded)
                    throw new InvalidOperationException($"Locking out {user.Email} failed: {string.Join(", ", lockResult.Errors.Select(e => e.Description))}");
            }

            seniors[s.Slug] = s.Senior;
        }
        return seniors;
    }

    // One Family Member linked to Wilson (#66) and one unlinked for Admin to link (#64)
    private static async Task SeedFamilyAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager, Senior linkedSenior)
    {
        var linked = new FamilyMember { Person = NewPerson("Susan", "Wilson", "1450 W 41st Ave", "Vancouver", "V6M 1X9", "604-555-0121") };
        db.FamilyMember.Add(linked);
        db.SeniorFamilyMember.Add(new SeniorFamilyMember { Senior = linkedSenior, FamilyMember = linked });
        await SeedLoginAsync(db, userManager, "family.wilson@seniorcare.test", linked.Person, RoleNames.FamilyMember);

        var unlinked = new FamilyMember { Person = NewPerson("Mark", "Lee", "720 Robson St", "Vancouver", "V6Z 1A1", "604-555-0123") };
        db.FamilyMember.Add(unlinked);
        await SeedLoginAsync(db, userManager, "family.lee@seniorcare.test", unlinked.Person, RoleNames.FamilyMember);
    }

    // Mixed statuses per caregiver for #62, #63 and #65; Missed is computed (OLTP-17), never stored
    private static async Task SeedVisitsAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
        Dictionary<string, Senior> seniors, DateTime now)
    {
        var services = await db.Service.ToDictionaryAsync(s => s.ServiceName);
        var sunrise = await FindAgencyStaffAsync(db, userManager, "sunrise");
        var harbour = await FindAgencyStaffAsync(db, userManager, "harbour");
        const string vgh = "Vancouver General Hospital, 899 W 12th Ave, Vancouver";
        const string burnabyHospital = "Burnaby Hospital, 3935 Kincaid St, Burnaby";

        // Kept by name so the notes below don't depend on array order
        var wilsonHome = NewVisit(sunrise, seniors["wilson"], services["Home visit"], now.AddDays(-7), VisitStatus.Completed, checkInAfter: 5);
        var wilsonDoctor = NewVisit(sunrise, seniors["wilson"], services["Doctor"], now.AddDays(-3), VisitStatus.Completed, checkInAfter: 20, destination: vgh);   // Late
        var davisMissed = NewVisit(harbour, seniors["davis"], services["Home visit"], now.AddDays(-5), VisitStatus.Completed, checkInAfter: 75);   // Missed, check-in after +60

        var visits = new[]
        {
            wilsonHome,
            wilsonDoctor,
            NewVisit(sunrise, seniors["brown"], services["Bathing"], now.AddDays(-2), VisitStatus.Scheduled),   // Missed, no check-in
            NewVisit(sunrise, seniors["wilson"], services["Home visit"], now.AddHours(2), VisitStatus.Scheduled),
            NewVisit(sunrise, seniors["brown"], services["Doctor"], now.AddDays(7), VisitStatus.Scheduled, destination: vgh),
            NewVisit(sunrise, seniors["clark"], services["Home visit"], now.AddDays(10), VisitStatus.Scheduled),   // #67 cancels
            NewVisit(sunrise, seniors["brown"], services["Bathing"], now.AddDays(-6), VisitStatus.Cancelled),
            davisMissed,
            NewVisit(harbour, seniors["taylor"], services["Bathing"], now.AddDays(-4), VisitStatus.Cancelled),
            NewVisit(harbour, seniors["taylor"], services["Home visit"], now.AddMinutes(-25), VisitStatus.InProgress, checkInAfter: 5),
            NewVisit(harbour, seniors["davis"], services["Doctor"], now.AddDays(8), VisitStatus.Scheduled, destination: burnabyHospital),
            NewVisit(harbour, seniors["clark"], services["Bathing"], now.AddDays(12), VisitStatus.Scheduled),   // #67 cancels
            NewVisit(harbour, seniors["hall"], services["Home visit"], now.AddDays(-14), VisitStatus.Completed, checkInAfter: 0),
            NewVisit(harbour, seniors["hall"], services["Home visit"], now.AddDays(9), VisitStatus.Cancelled),   // cancelled by deactivation
        };
        db.Visit.AddRange(visits);

        // Two notes on Wilson for newest first (OLTP-15, #66), one on Davis
        db.VisitNote.AddRange(
            NewNote(wilsonHome, "Blood sugar 7.2 before lunch. Walked to the mailbox with her cane."),
            NewNote(wilsonDoctor, "Doctor adjusted Metformin timing to with meals. Remind her at each visit."),
            NewNote(davisMissed, "Arrived late due to traffic. Evelyn prefers visits after 10 am."));
        await db.SaveChangesAsync();
    }

    private static async Task<(Caregiver Caregiver, string ManagerUserId)> FindAgencyStaffAsync(ApplicationDbContext db,
        UserManager<ApplicationUser> userManager, string slug)
    {
        var caregiverUser = await userManager.FindByEmailAsync($"caregiver.{slug}@seniorcare.test")
            ?? throw new InvalidOperationException($"Seeded caregiver.{slug} not found");
        var manager = await userManager.FindByEmailAsync($"manager.{slug}@seniorcare.test")
            ?? throw new InvalidOperationException($"Seeded manager.{slug} not found");

        var caregiver = await db.Caregiver.SingleOrDefaultAsync(c => c.CaregiverID == caregiverUser.PersonID)
            ?? throw new InvalidOperationException($"Seeded caregiver.{slug} has no Caregiver row");
        return (caregiver, manager.Id);
    }

    // AgencyID is the caregiver's agency at scheduling; Completed visits check out 60 minutes after check-in
    private static Visit NewVisit((Caregiver Caregiver, string ManagerUserId) staff, Senior senior, Service service,
        DateTime start, VisitStatus status, int? checkInAfter = null, string? destination = null)
    {
        DateTime? checkIn = checkInAfter.HasValue ? start.AddMinutes(checkInAfter.Value) : null;
        return new Visit
        {
            Senior = senior, Service = service, Caregiver = staff.Caregiver, AgencyID = staff.Caregiver.AgencyID,
            CreatedByUserID = staff.ManagerUserId, ScheduledStart = start, ActualCheckIn = checkIn,
            ActualCheckOut = status == VisitStatus.Completed ? checkIn?.AddMinutes(60) : null,
            Status = status, DestinationAddress = destination,
        };
    }

    private static VisitNote NewNote(Visit visit, string text) =>
        new() { Visit = visit, Caregiver = visit.Caregiver, NoteText = text, CreatedAt = visit.ActualCheckOut!.Value };

    private static Person NewPerson(string first, string last, string street, string city, string postalCode, string phone) =>
        new() { FirstName = first, LastName = last, Street = street, City = city, Province = "BC", Country = "Canada",
            PostalCode = postalCode, Phone = phone };

    // Saves the pending Person row (fills PersonID), then creates its login; caller owns the transaction
    private static async Task<ApplicationUser> SeedLoginAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
        string email, Person person, string? role)
    {
        await db.SaveChangesAsync();
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, PersonID = person.PersonID };
        await CreateUserAsync(userManager, user, "Password123!", role);
        return user;
    }

    // Shared by all seeded accounts; role is null for a Staff login waiting on Admin (OLTP-2)
    private static async Task CreateUserAsync(UserManager<ApplicationUser> userManager,
        ApplicationUser user, string password, string? role)
    {
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Seeding {user.Email} failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        if (role == null)
            return;

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException($"Adding {user.Email} to {role} failed: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
    }
}