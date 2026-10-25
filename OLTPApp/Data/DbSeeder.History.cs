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

        var caregivers = await SeedHistoryStaffAsync(db, userManager);
        var seniors = await SeedHistorySeniorsAsync(db, random, now);
        await SeedHistoryVisitsAsync(db, random, now, caregivers, seniors);

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

    // Spec volumes 2025-01 to 2026-03, oldest first, mapped onto the 15 months ending now; the last is the current, partial month
    private static readonly int[] HistoryMonthlyVolumes = { 170, 185, 195, 205, 220, 230, 240, 210, 190, 180, 185, 200, 220, 225, 145 };

    private enum HistoryKind { OnTime, Late, Missed, Cancelled }

    // Spec visit distribution, 3,000 in all
    private static readonly (HistoryKind Kind, int Target)[] HistoryTargets =
    {
        (HistoryKind.OnTime, 2250), (HistoryKind.Late, 360), (HistoryKind.Missed, 240), (HistoryKind.Cancelled, 150),
    };

    // Query 4 positives with the spec's missed appointment types
    private static readonly (string Senior, string[] Services)[] ScenarioMisses =
    {
        ("S001", new[] { "Doctor", "Home visit", "Bathing" }),
        ("S002", new[] { "Home visit", "Home visit", "Bathing" }),
        ("S003", new[] { "Doctor", "Doctor", "Home visit" }),
        ("S004", new[] { "Bathing", "Doctor", "Bathing" }),
        ("S005", new[] { "Home visit", "Bathing", "Doctor" }),
    };

    // Query 3 higher late-rate profiles: more Late visits, and later ones
    private static readonly string[] HighLateCaregivers = { "C001", "C002", "C003", "C004", "C005" };

    // Query 3 caregiver with no Late visits
    private const string NoLateCaregiver = "C030";

    private static readonly string[] HistoryClinics =
    {
        "Vancouver General Hospital, 899 W 12th Ave, Vancouver",
        "Burnaby Hospital, 3935 Kincaid St, Burnaby",
        "Richmond Hospital, 7000 Westminster Hwy, Richmond",
    };

    // 3,000 visits for the history caregivers and S001-S144, saved in one go after the spec checks pass
    private static async Task SeedHistoryVisitsAsync(ApplicationDbContext db, Random random, DateTime now,
        Dictionary<string, Caregiver> caregivers, Dictionary<string, Senior> seniors)
    {
        var services = await db.Service.ToDictionaryAsync(s => s.ServiceName);
        // Each visit is created by its caregiver's agency manager
        var managerIds = await db.AgencyManager
            .Join(db.Users, m => (int?)m.AgencyManagerID, u => u.PersonID, (m, u) => new { m.AgencyID, u.Id })
            .ToDictionaryAsync(x => x.AgencyID, x => x.Id);

        var builder = new HistoryVisitBuilder(random, now, caregivers, seniors, services, managerIds);
        builder.AddScenarioVisits();
        builder.AddRandomVisits();
        CheckHistoryVisits(builder.Visits, caregivers, seniors, now);

        db.Visit.AddRange(builder.Visits);
        await db.SaveChangesAsync();
    }

    // Valid for history only, where every visit is past +60: Missed and Late per OLAP_Queries.md
    private static HistoryKind HistoryKindOf(Visit v) =>
        v.Status == VisitStatus.Cancelled ? HistoryKind.Cancelled
        : v.ActualCheckIn is not { } checkIn || checkIn > v.ScheduledStart.AddMinutes(60) ? HistoryKind.Missed
        : checkIn > v.ScheduledStart.AddMinutes(10) ? HistoryKind.Late
        : HistoryKind.OnTime;

    // Stops startup if the history breaks the spec totals or the Query 3 and Query 4 scenarios
    private static void CheckHistoryVisits(List<Visit> visits, Dictionary<string, Caregiver> caregivers,
        Dictionary<string, Senior> seniors, DateTime now)
    {
        var problems = new List<string>();
        var counts = visits.GroupBy(HistoryKindOf).ToDictionary(g => g.Key, g => g.Count());
        foreach (var (kind, target) in HistoryTargets)
            if (counts.GetValueOrDefault(kind) != target)
                problems.Add($"{kind} is {counts.GetValueOrDefault(kind)}, expected {target}");

        var lateCaregivers = visits.Where(v => HistoryKindOf(v) == HistoryKind.Late).Select(v => v.Caregiver).ToHashSet();
        foreach (var (code, caregiver) in caregivers)
            if (lateCaregivers.Contains(caregiver) == (code == NoLateCaregiver))
                problems.Add(code == NoLateCaregiver ? $"{code} has a Late visit" : $"{code} has no Late visit");

        // Query 3: C001-C005 have the 5 highest AvgMinutesLate
        var highLate = HighLateCaregivers.Select(c => caregivers[c]).ToHashSet();
        var topLate = visits.Where(v => HistoryKindOf(v) == HistoryKind.Late)
            .GroupBy(v => v.Caregiver)
            .OrderByDescending(g => g.Average(v => (v.ActualCheckIn!.Value - v.ScheduledStart).TotalMinutes))
            .Take(highLate.Count).Select(g => g.Key);
        if (!highLate.SetEquals(topLate))
            problems.Add("C001-C005 are not the 5 highest AvgMinutesLate");

        // Query 4 at seed time and once the last demo visit (about +12 days) is the anchor
        var positives = ScenarioMisses.Select(s => seniors[s.Senior]).ToHashSet();
        foreach (var anchor in new[] { now, now.AddDays(12) })
        {
            var flagged = visits
                .Where(v => HistoryKindOf(v) == HistoryKind.Missed && v.ScheduledStart >= anchor.AddDays(-90) && v.ScheduledStart <= anchor)
                .GroupBy(v => v.Senior).Where(g => g.Count() >= 3).Select(g => g.Key);
            if (!positives.SetEquals(flagged))
                problems.Add($"Query 4 at {anchor:u} does not flag exactly S001-S005");
        }

        if (problems.Count > 0)
            throw new InvalidOperationException($"History seed breaks the spec: {string.Join("; ", problems)}");
    }

    // Whole-minute daytime starts per month, up to 4 hours ago so every check-out is in the past.
    // The current month's volume is scaled to its elapsed time, then all months are scaled to 3,000.
    private static List<DateTime> HistorySlots(Random random, DateTime now)
    {
        var end = now.AddHours(-4);
        var current = new DateTime(end.Year, end.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        // On the 1st the current month has no full day yet, so the 15 months end with the previous one
        if (end.Date == current)
            current = current.AddMonths(-1);
        var last = HistoryMonthlyVolumes.Length - 1;
        var weights = HistoryMonthlyVolumes.Select(v => (double)v).ToArray();
        weights[last] *= Math.Min(1, (end - current).TotalDays / DateTime.DaysInMonth(current.Year, current.Month));
        var counts = Apportion(weights, HistoryTargets.Sum(t => t.Target));

        var slots = new List<DateTime>();
        for (var m = 0; m <= last; m++)
        {
            var from = current.AddMonths(m - last);
            var to = from.AddMonths(1) < end ? from.AddMonths(1) : end;
            for (var i = 0; i < counts[m]; i++)
                slots.Add(NextHistorySlot(random, from, to));
        }
        return slots;
    }

    // 16:00 to 01:00 UTC is 8 am to 5 pm PST and 9 am to 6 pm PDT, so starts stay in daytime hours all year
    private static DateTime NextHistorySlot(Random random, DateTime from, DateTime to)
    {
        var days = (int)Math.Ceiling((to - from).TotalDays);
        DateTime start;
        do
        {
            start = from.AddDays(random.Next(days)).AddHours(16).AddMinutes(random.Next(9 * 60 + 1));
        } while (start >= to);
        return start;
    }

    // Largest remainder: whole counts proportional to the weights that sum to total
    private static int[] Apportion(double[] weights, int total)
    {
        var sum = weights.Sum();
        var exact = weights.Select(w => w * total / sum).ToArray();
        var counts = exact.Select(e => (int)Math.Floor(e)).ToArray();
        var extras = Enumerable.Range(0, weights.Length).Where(i => weights[i] > 0)
            .OrderByDescending(i => exact[i] - counts[i]).Take(total - counts.Sum()).ToList();
        foreach (var i in extras)
            counts[i]++;
        return counts;
    }

    // Draws starts from the monthly slots and builds each visit, counting recent misses so Query 4 flags only S001-S005
    private sealed class HistoryVisitBuilder
    {
        // Misses at or after this count toward every Query 4 window the moving anchor can reach, with margin
        private const int RecentMissDays = 102;
        private const int MaxRecentMisses = 2;

        private readonly Random _random;
        private readonly DateTime _now;
        private readonly DateTime _recentFrom;
        private readonly Dictionary<string, Caregiver> _caregivers;
        private readonly Dictionary<string, Senior> _seniors;
        private readonly Dictionary<string, Service> _services;
        private readonly Dictionary<int, string> _managerIds;
        private readonly List<DateTime> _slots;
        private readonly string[] _allCodes;
        private readonly string[] _codesWithLate;     // every caregiver but C030
        private readonly string[] _normalLateCodes;   // every caregiver but C001-C005 and C030
        private readonly string[] _weightedLateCodes; // C001-C005 three times each
        private readonly Senior[] _allSeniors;
        private readonly Senior[] _otherSeniors;      // S008-S144, the only seniors with random misses
        private readonly Dictionary<Senior, int> _recentMisses = new();

        public List<Visit> Visits { get; } = new();

        public HistoryVisitBuilder(Random random, DateTime now, Dictionary<string, Caregiver> caregivers,
            Dictionary<string, Senior> seniors, Dictionary<string, Service> services, Dictionary<int, string> managerIds)
        {
            _random = random;
            _now = now;
            _recentFrom = now.AddDays(-RecentMissDays);
            _caregivers = caregivers;
            _seniors = seniors;
            _services = services;
            _managerIds = managerIds;
            _slots = HistorySlots(random, now);
            _allCodes = caregivers.Keys.Order().ToArray();
            _codesWithLate = _allCodes.Where(c => c != NoLateCaregiver).ToArray();
            _normalLateCodes = _codesWithLate.Where(c => !HighLateCaregivers.Contains(c)).ToArray();
            _weightedLateCodes = _codesWithLate.SelectMany(c => Enumerable.Repeat(c, HighLateCaregivers.Contains(c) ? 3 : 1)).ToArray();
            _allSeniors = Enumerable.Range(1, HistorySeniorCount).Select(i => seniors[$"S{i:D3}"]).ToArray();
            _otherSeniors = _allSeniors.Skip(ScenarioSeniors.Length).ToArray();
        }

        // Fixed visits first; they come out of the same monthly slots, so the totals still hold
        public void AddScenarioVisits()
        {
            // Query 4 positives: 3 misses each, 30 to 75 days back, inside every window the anchor can reach
            foreach (var (code, serviceNames) in ScenarioMisses)
                foreach (var name in serviceNames)
                    AddMissed(RandomCode(_codesWithLate), _seniors[code], _services[name], TakeSlot(-75, -30));

            // Query 4 negatives: S006 has 1 miss; S007 has 2 inside the window and 1 before it
            AddMissed(RandomCode(_codesWithLate), _seniors["S006"], RandomService(), TakeSlot(-75, -30));
            AddMissed(RandomCode(_codesWithLate), _seniors["S007"], RandomService(), TakeSlot(-130, -110));
            AddMissed(RandomCode(_codesWithLate), _seniors["S007"], RandomService(), TakeSlot(-75, -30));
            AddMissed(RandomCode(_codesWithLate), _seniors["S007"], RandomService(), TakeSlot(-75, -30));

            // Query 3: C030's one visit that is not on time is a check-in after +60, so Missed and not Late
            var slot = TakeSlot();
            Add(NoLateCaregiver, SeniorForMiss(slot), RandomService(), slot, VisitStatus.Completed, Seconds(3660, 7200));

            // Timestamp boundaries: +10:00 on time, +10:01 and +60:00 Late, +60:00.4 Missed.
            // Each Late one stays inside its caregiver's late range: +10:01 for others, +60:00 for C001-C005.
            Add(RandomCode(_codesWithLate), RandomSenior(), RandomService(), TakeSlot(), VisitStatus.Completed, TimeSpan.FromMinutes(10));
            Add(RandomCode(_normalLateCodes), RandomSenior(), RandomService(), TakeSlot(), VisitStatus.Completed, TimeSpan.FromSeconds(601));
            Add(RandomCode(HighLateCaregivers), RandomSenior(), RandomService(), TakeSlot(), VisitStatus.Completed, TimeSpan.FromMinutes(60));
            slot = TakeSlot();
            Add(RandomCode(_codesWithLate), SeniorForMiss(slot), RandomService(), slot, VisitStatus.Completed,
                TimeSpan.FromMinutes(60).Add(TimeSpan.FromMilliseconds(400)));
        }

        // Shuffled categories for the slots left, topped up to the spec totals
        public void AddRandomVisits()
        {
            var counted = Visits.GroupBy(HistoryKindOf).ToDictionary(g => g.Key, g => g.Count());
            var kinds = HistoryTargets
                .SelectMany(t => Enumerable.Repeat(t.Kind, t.Target - counted.GetValueOrDefault(t.Kind))).ToArray();
            if (kinds.Length != _slots.Count)
                throw new InvalidOperationException($"History has {_slots.Count} slots left for {kinds.Length} visits");
            _random.Shuffle(kinds);

            // One Late visit for each caregiver but C030 before the weighted picks
            var lateFirst = new Queue<string>(_codesWithLate);
            for (var i = 0; i < kinds.Length; i++)
            {
                var start = _slots[i];
                switch (kinds[i])
                {
                    case HistoryKind.OnTime:
                        Add(RandomCode(_allCodes), RandomSenior(), RandomService(), start, VisitStatus.Completed, Seconds(-300, 599));
                        break;
                    case HistoryKind.Late:
                        var code = lateFirst.Count > 0 ? lateFirst.Dequeue() : RandomCode(_weightedLateCodes);
                        // C001-C005 at +45 to +59:59, everyone else at +11 to +44:59: the ranges never overlap,
                        // so C001-C005 top Query 3's AvgMinutesLate on any seed date
                        var lateOffset = HighLateCaregivers.Contains(code) ? Seconds(2700, 3599) : Seconds(660, 2699);
                        Add(code, RandomSenior(), RandomService(), start, VisitStatus.Completed, lateOffset);
                        break;
                    case HistoryKind.Missed:
                        AddMissed(RandomCode(_codesWithLate), SeniorForMiss(start), RandomService(), start);
                        break;
                    case HistoryKind.Cancelled:
                        Add(RandomCode(_codesWithLate), RandomSenior(), RandomService(), start, VisitStatus.Cancelled, null);
                        break;
                }
            }
            _slots.Clear();
        }

        // 80% never checked in; 20% checked in after +60, which stays Missed even as Completed
        private void AddMissed(string code, Senior senior, Service service, DateTime start)
        {
            if (_random.NextDouble() < 0.2)
                Add(code, senior, service, start, VisitStatus.Completed, Seconds(3660, 7200));
            else
                Add(code, senior, service, start, VisitStatus.Scheduled, null);
        }

        // AgencyID and creator come from the caregiver; check-out is 45 to 90 minutes after check-in
        private void Add(string code, Senior senior, Service service, DateTime start, VisitStatus status, TimeSpan? checkInAfter)
        {
            var caregiver = _caregivers[code];
            DateTime? checkIn = start + checkInAfter;
            var visit = new Visit
            {
                Senior = senior, Service = service, Caregiver = caregiver, AgencyID = caregiver.AgencyID,
                CreatedByUserID = _managerIds[caregiver.AgencyID], ScheduledStart = start, ActualCheckIn = checkIn,
                ActualCheckOut = status == VisitStatus.Completed ? checkIn?.AddMinutes(_random.Next(45, 91)) : null,
                Status = status,
                DestinationAddress = service.ServiceName == "Doctor" ? HistoryClinics[_random.Next(HistoryClinics.Length)] : null,
            };
            Visits.Add(visit);

            if (HistoryKindOf(visit) == HistoryKind.Missed && start >= _recentFrom)
                _recentMisses[senior] = _recentMisses.GetValueOrDefault(senior) + 1;
        }

        // Removes a random slot between the given days back from now, or any slot
        private DateTime TakeSlot(int? fromDays = null, int? toDays = null)
        {
            var from = fromDays.HasValue ? _now.AddDays(fromDays.Value) : DateTime.MinValue;
            var to = toDays.HasValue ? _now.AddDays(toDays.Value) : DateTime.MaxValue;
            var matches = Enumerable.Range(0, _slots.Count).Where(i => _slots[i] >= from && _slots[i] < to).ToList();
            if (matches.Count == 0)
                throw new InvalidOperationException($"No history slot between {from:u} and {to:u}");

            var index = matches[_random.Next(matches.Count)];
            var slot = _slots[index];
            _slots.RemoveAt(index);
            return slot;
        }

        // S008-S144 senior who stays under 3 misses in any window the anchor can reach
        private Senior SeniorForMiss(DateTime start)
        {
            Senior senior;
            do
            {
                senior = _otherSeniors[_random.Next(_otherSeniors.Length)];
            } while (start >= _recentFrom && _recentMisses.GetValueOrDefault(senior) >= MaxRecentMisses);
            return senior;
        }

        private Senior RandomSenior() => _allSeniors[_random.Next(_allSeniors.Length)];

        private string RandomCode(string[] codes) => codes[_random.Next(codes.Length)];

        // 40% Home visit, 35% Bathing, 25% Doctor
        private Service RandomService() => _random.Next(100) switch
        {
            < 40 => _services["Home visit"],
            < 75 => _services["Bathing"],
            _ => _services["Doctor"],
        };

        // Check-in offset in whole seconds, both ends included
        private TimeSpan Seconds(int min, int max) => TimeSpan.FromSeconds(_random.Next(min, max + 1));
    }
}
