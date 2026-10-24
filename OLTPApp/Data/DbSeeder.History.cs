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

        var now = DateTime.UtcNow;
        var random = new Random(56);   // fixed seed: same relative data on every fresh database
        await using var tx = await db.Database.BeginTransactionAsync();

        await SeedHistoryStaffAsync(db, userManager);
        await SeedHistorySeniorsAsync(db, random, now);

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

    // + 6 demo seniors = the spec's 150; demo seniors (S145-S150 in the spec) get no history visits
    private const int HistorySeniorCount = 144;

    // Spec scenario seniors: S001-S005 are the Query 4 positives with the Query 2 bands and living alone values,
    // S006 and S007 the Query 4 negatives. Age is at seed time.
    private static readonly (string First, string Last, int Age, bool IsLivingAlone)[] ScenarioSeniors =
    {
        ("Dorothy", "Fleming", 62, true),     // S001 Under 65
        ("Arthur", "Lambert", 70, false),     // S002 65-74
        ("Beatrice", "Holloway", 80, true),   // S003 75-84
        ("Frank", "Whitaker", 89, false),     // S004 85+
        ("Irene", "Caldwell", 78, true),      // S005 75-84
        ("Leonard", "Price", 74, true),       // S006
        ("Mabel", "Sutton", 83, false),       // S007
    };

    private static readonly string[] HistoryFirstNames =
    {
        "Agnes", "Albert", "Alice", "Barbara", "Bernard", "Carol", "Charles", "Doris", "Edward", "Eleanor",
        "Ernest", "Florence", "Gerald", "Gladys", "Henry", "Ida", "Jack", "Joan", "Kenneth", "Lillian",
        "Louis", "Marion", "Norman", "Patricia", "Raymond", "Shirley", "Stanley", "Thelma", "Vera", "Wallace",
    };

    private static readonly string[] HistoryLastNames =
    {
        "Anderson", "Bishop", "Campbell", "Chow", "Dhillon", "Edwards", "Ferguson", "Gagnon", "Grant", "Harper",
        "Ito", "Jensen", "Kowalski", "Leblanc", "MacDonald", "Morrison", "Nakamura", "Olsen", "Parker", "Quinn",
        "Robertson", "Sandhu", "Stewart", "Tremblay", "Turner", "Vasquez", "Walsh", "Yamamoto", "Young", "Zhang",
    };

    private static readonly string[] HistoryStreets =
    {
        "Main St", "Oak St", "Cambie St", "Kingsway", "Hastings St", "Marine Dr", "Granville St", "Victoria Dr",
    };

    private static readonly (string City, string PostalCode)[] HistoryCities =
    {
        ("Vancouver", "V5V 3N2"), ("Burnaby", "V5H 2A9"), ("Richmond", "V6X 1A4"), ("North Vancouver", "V7L 1B3"),
        ("Coquitlam", "V3J 4G2"), ("Surrey", "V3W 1H7"), ("New Westminster", "V3L 3C6"),
    };

    // Under 65, 65-74, 75-84 and 85+ at seed time
    private static readonly (int Min, int Max)[] HistoryAgeBands = { (58, 64), (65, 74), (75, 84), (85, 96) };

    // 144 seniors without logins, keyed by spec code S001-S144; about 40% live alone
    private static async Task<Dictionary<string, Senior>> SeedHistorySeniorsAsync(ApplicationDbContext db, Random random, DateTime now)
    {
        var usedNames = ScenarioSeniors.Select(s => (s.First, s.Last)).ToHashSet();
        var seniors = new Dictionary<string, Senior>();

        for (var i = 1; i <= HistorySeniorCount; i++)
        {
            var (first, last, age, isLivingAlone) = i <= ScenarioSeniors.Length
                ? ScenarioSeniors[i - 1]
                : NextHistorySenior(random, usedNames);

            var (city, postalCode) = HistoryCities[random.Next(HistoryCities.Length)];
            var street = $"{random.Next(100, 9999)} {HistoryStreets[random.Next(HistoryStreets.Length)]}";
            var senior = new Senior
            {
                Person = NewPerson(first, last, street, city, postalCode, $"604-555-{2000 + i}"),
                DateOfBirth = HistoryDateOfBirth(now, age, random),
                IsLivingAlone = isLivingAlone,
            };
            db.Senior.Add(senior);
            seniors[$"S{i:D3}"] = senior;
        }

        await db.SaveChangesAsync();
        return seniors;
    }

    // Random name not used yet, with an age from an evenly picked band
    private static (string First, string Last, int Age, bool IsLivingAlone) NextHistorySenior(Random random,
        HashSet<(string, string)> usedNames)
    {
        string first, last;
        do
        {
            first = HistoryFirstNames[random.Next(HistoryFirstNames.Length)];
            last = HistoryLastNames[random.Next(HistoryLastNames.Length)];
        } while (!usedNames.Add((first, last)));

        var (min, max) = HistoryAgeBands[random.Next(HistoryAgeBands.Length)];
        return (first, last, random.Next(min, max + 1), random.NextDouble() < 0.4);
    }

    // Last birthday 60 to 300 days before seed time, so the age band holds while the anchor moves (up to about +12 days)
    private static DateOnly HistoryDateOfBirth(DateTime now, int age, Random random) =>
        DateOnly.FromDateTime(now.AddYears(-age).AddDays(-random.Next(60, 301)));

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
