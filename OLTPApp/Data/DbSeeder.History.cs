using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OLTPApp.Authorization;
using OLTPApp.Models;

namespace OLTPApp.Data;

// #56: history for the OLAP reports, per docs/olap/Seed-Specification.md (8 agencies, 30 caregivers, 150 seniors, 3,000 visits).
// Codes such as C001 are the spec's IDs; the spec maps them to the seeded names, not to database IDs.
public static partial class DbSeeder
{
    // First new manager doubles as the history guard, independent of the demo guard
    private const string HistoryGuardEmail = "manager.northshore@seniorcare.test";

    private static async Task SeedHistoryAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync(HistoryGuardEmail) != null)
            return;

        await using var tx = await db.Database.BeginTransactionAsync();

        await SeedHistoryStaffAsync(db, userManager);

        await tx.CommitAsync();
    }

    // 6 new agencies with a manager login each, plus 28 caregivers without logins across all 8 agencies.
    // Daniel Kim (C028) and Liam Chen (C029) from SeedAgenciesAsync make 30; history visits go only to these 28.
    private static async Task<Dictionary<string, Caregiver>> SeedHistoryStaffAsync(ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        var newAgencies = new[]
        {
            (Slug: "northshore", Agency: new Agency { Name = "North Shore Senior Care", Street = "125 Lonsdale Ave", City = "North Vancouver",
                Province = "BC", Country = "Canada", PostalCode = "V7M 2E8", Phone = "604-555-0300", Email = "info@northshore.test" },
             Manager: ("Olivia", "Martin"),
             Caregivers: new[] { ("C003", "Ethan", "Wright"), ("C011", "Hannah", "Scott"), ("C019", "Lucas", "Moreau"), ("C025", "Grace", "Park") }),
            (Slug: "riverside", Agency: new Agency { Name = "Riverside Home Health", Street = "6060 Minoru Blvd", City = "Richmond",
                Province = "BC", Country = "Canada", PostalCode = "V6Y 1Y2", Phone = "604-555-0310", Email = "info@riverside.test" },
             Manager: ("Ahmed", "Hassan"),
             Caregivers: new[] { ("C004", "Mei", "Lin"), ("C012", "Noah", "Fischer"), ("C020", "Isabel", "Cruz"), ("C026", "Ryan", "O'Neil") }),
            (Slug: "coquitlam", Agency: new Agency { Name = "Coquitlam Care Partners", Street = "1180 Pinetree Way", City = "Coquitlam",
                Province = "BC", Country = "Canada", PostalCode = "V3B 7L2", Phone = "604-555-0320", Email = "info@coquitlam.test" },
             Manager: ("Sofia", "Rossi"),
             Caregivers: new[] { ("C005", "Jacob", "Nguyen"), ("C013", "Chloe", "Dubois"), ("C021", "Arjun", "Mehta"), ("C027", "Emma", "Larsen") }),
            (Slug: "surrey", Agency: new Agency { Name = "Surrey Community Care", Street = "13450 104 Ave", City = "Surrey",
                Province = "BC", Country = "Canada", PostalCode = "V3T 1V8", Phone = "604-555-0330", Email = "info@surrey.test" },
             Manager: ("Gurpreet", "Gill"),
             Caregivers: new[] { ("C006", "Zoe", "Campbell"), ("C014", "Mateo", "Silva"), ("C022", "Leah", "Goldberg"), ("C030", "Samuel", "Osei") }),
            (Slug: "westside", Agency: new Agency { Name = "Westside Companion Care", Street = "2525 W Broadway", City = "Vancouver",
                Province = "BC", Country = "Canada", PostalCode = "V6K 2E9", Phone = "604-555-0340", Email = "info@westside.test" },
             Manager: ("Claire", "Bennett"),
             Caregivers: new[] { ("C007", "Victor", "Petrov"), ("C015", "Nora", "Kelly"), ("C023", "Ivan", "Novak") }),
            (Slug: "newwest", Agency: new Agency { Name = "New West Home Support", Street = "610 Columbia St", City = "New Westminster",
                Province = "BC", Country = "Canada", PostalCode = "V3M 1A5", Phone = "604-555-0350", Email = "info@newwest.test" },
             Manager: ("Marcus", "Reid"),
             Caregivers: new[] { ("C008", "Priyanka", "Rao"), ("C016", "Felix", "Wagner"), ("C024", "Amara", "Okafor") }),
        };

        // More caregivers for the two demo agencies, so #61's dropdown has options
        var existingAgencies = new[]
        {
            (Name: "Sunrise Home Care", Caregivers: new[] { ("C001", "Aisha", "Khan"), ("C009", "Tom", "Becker"), ("C017", "Rosa", "Mendes") }),
            (Name: "Harbour Care Services", Caregivers: new[] { ("C002", "Kenji", "Sato"), ("C010", "Fatima", "Ali"), ("C018", "Owen", "Murphy") }),
        };

        // Manager logins first: SeedLoginAsync saves, so caregivers then go in one save at the end
        foreach (var s in newAgencies)
        {
            db.Agency.Add(s.Agency);
            var (first, last) = s.Manager;
            var person = NewPerson(first, last, s.Agency.Street, s.Agency.City, s.Agency.PostalCode, s.Agency.Phone);
            db.AgencyManager.Add(new AgencyManager { Person = person, Agency = s.Agency });
            await SeedLoginAsync(db, userManager, $"manager.{s.Slug}@seniorcare.test", person, RoleNames.AgencyManager);
        }

        var caregivers = new Dictionary<string, Caregiver>();
        foreach (var s in newAgencies)
            AddCaregivers(db, s.Agency, s.Caregivers, caregivers);
        foreach (var e in existingAgencies)
            AddCaregivers(db, await db.Agency.SingleAsync(a => a.Name == e.Name), e.Caregivers, caregivers);

        await db.SaveChangesAsync();
        return caregivers;
    }

    // Caregivers share their agency's address and phone, like SeedStaffAsync
    private static void AddCaregivers(ApplicationDbContext db, Agency agency, (string Code, string First, string Last)[] names,
        Dictionary<string, Caregiver> caregivers)
    {
        foreach (var c in names)
        {
            var caregiver = new Caregiver { Person = NewPerson(c.First, c.Last, agency.Street, agency.City, agency.PostalCode, agency.Phone), Agency = agency };
            db.Caregiver.Add(caregiver);
            caregivers[c.Code] = caregiver;
        }
    }
}
