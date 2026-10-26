using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OLTPApp.Authorization;
using OLTPApp.Data;
using OLTPApp.Models;
using OLTPApp.Services;
using System.Security.Claims;

namespace OLTPApp.Controllers;

// #61: shared by visit pages. Each action adds its own role policy:
// [Authorize(Policy = Policies.AgencyManager)] for scheduling (#61) and agency visits (#65),
// [Authorize(Policy = Policies.Caregiver)] for the visit list (#62) and visit page (#63).
[Authorize]
public class VisitsController : Controller
{
    private readonly ApplicationDbContext _db;

    public VisitsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Policy = Policies.AgencyManager)]
    public async Task<IActionResult> Schedule()
    {
        var agencyId = await GetManagerAgencyIdAsync();
        if (agencyId == null)
            return Forbid();

        var model = new ScheduleVisitViewModel();
        await LoadScheduleOptionsAsync(model, agencyId.Value);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.AgencyManager)]
    public async Task<IActionResult> Schedule(ScheduleVisitViewModel model)
    {
        var agencyId = await GetManagerAgencyIdAsync();
        if (agencyId == null)
            return Forbid();

        if (!await _db.Senior.AnyAsync(s => s.SeniorID == model.SeniorID && s.Status == SeniorStatus.Active))
            ModelState.AddModelError(nameof(model.SeniorID), "Choose an active senior.");

        if (!await _db.Caregiver.AnyAsync(c => c.CaregiverID == model.CaregiverID && c.AgencyID == agencyId.Value))
            ModelState.AddModelError(nameof(model.CaregiverID), "Choose a caregiver from your agency.");

        if (!await _db.Service.AnyAsync(s => s.ServiceID == model.ServiceID))
            ModelState.AddModelError(nameof(model.ServiceID), "Choose a valid visit type.");

        DateTime? scheduledStartUtc = null;
        if (model.ScheduledStart.HasValue)
        {
            // datetime-local has no offset. Use the zone shown on the form, not the server's local zone.
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");
            var localStart = DateTime.SpecifyKind(model.ScheduledStart.Value, DateTimeKind.Unspecified);
            if (timeZone.IsInvalidTime(localStart) || timeZone.IsAmbiguousTime(localStart))
                ModelState.AddModelError(nameof(model.ScheduledStart), "This time is skipped or repeated by daylight saving time. Choose another time.");
            else
            {
                scheduledStartUtc = TimeZoneInfo.ConvertTimeToUtc(localStart, timeZone);
                if (scheduledStartUtc <= DateTime.UtcNow)
                    ModelState.AddModelError(nameof(model.ScheduledStart), "Choose a date and time in the future.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadScheduleOptionsAsync(model, agencyId.Value);
            return View(model);
        }

        var visit = new Visit
        {
            SeniorID = model.SeniorID!.Value,
            CaregiverID = model.CaregiverID!.Value,
            ServiceID = model.ServiceID!.Value,
            ScheduledStart = scheduledStartUtc!.Value,
            DestinationAddress = model.DestinationAddress,
            AgencyID = agencyId.Value,
            CreatedByUserID = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            Status = VisitStatus.Scheduled
        };
        _db.Visit.Add(visit);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Visit scheduled.";
        return RedirectToAction(nameof(Schedule));
    }

    [HttpGet]
    [Authorize(Policy = Policies.AgencyManager)]
    public async Task<IActionResult> AgencyVisits(int page = 1)
    {
        const int pageSize = 20;

        var agencyId = await GetManagerAgencyIdAsync();
        if (agencyId == null)
            return Forbid();
        
        var agencyName = await _db.Agency
            .Where(a => a.AgencyID == agencyId)
            .Select(a => a.Name)
            .SingleAsync();
    
        // Bound page number within available range
        var totalCount = await _db.Visit.CountAsync(v => v.AgencyID == agencyId.Value);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
        
        var agencyVisits = await _db.Visit
            .Where(v => v.AgencyID == agencyId.Value)
            .OrderByDescending(v => v.ScheduledStart).ThenByDescending(v => v.VisitID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new AgencyVisitListItem
            {
                VisitID = v.VisitID,
                ScheduledStart = v.ScheduledStart,
                SeniorName = v.Senior.Person.FirstName + " " + v.Senior.Person.LastName,
                CaregiverName = v.Caregiver.Person.FirstName + " " + v.Caregiver.Person.LastName,
                ServiceName = v.Service.ServiceName,
                Status = VisitStatusHelper.GetDisplayStatus(v)
            })
            .ToListAsync();

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");
        foreach (var visit in agencyVisits)
            visit.ScheduledStart = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(visit.ScheduledStart, DateTimeKind.Utc), timeZone);
        
        return View(new AgencyVisitsViewModel
        {
            AgencyName = agencyName,
            Visits = agencyVisits,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        });
    }

    [HttpGet]
    [Authorize(Policy = Policies.Caregiver)]
    public async Task<IActionResult> MyVisits()
    {
        var caregiverId = await GetCaregiverIdAsync();
        if (caregiverId == null)
            return Forbid();

        // Missed visits stay on the list: they are still Scheduled (OLTP-17 computes Missed for display only)
        var visits = await _db.Visit
            .Where(v => v.CaregiverID == caregiverId.Value
                        && v.Status != VisitStatus.Completed && v.Status != VisitStatus.Cancelled)
            .OrderBy(v => v.ScheduledStart).ThenBy(v => v.VisitID)
            .Select(v => new CaregiverVisitListItem
            {
                VisitID = v.VisitID,
                ScheduledStart = v.ScheduledStart,
                SeniorName = v.Senior.Person.FirstName + " " + v.Senior.Person.LastName,
                ServiceName = v.Service.ServiceName,
                ResidentAddress = v.Senior.Person.Street + ", " + v.Senior.Person.City + ", "
                                  + v.Senior.Person.Province + " " + v.Senior.Person.PostalCode,
                DestinationAddress = v.DestinationAddress
            })
            .ToListAsync();

        // Stored in UTC; show in the same zone the Schedule form uses
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");
        foreach (var visit in visits)
            visit.ScheduledStart = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(visit.ScheduledStart, DateTimeKind.Utc), timeZone);

        return View(visits);
    }

    // Shared by the visit list (#62) and visit page (#63); null when the user has no Caregiver row
    private async Task<int?> GetCaregiverIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await (from user in _db.Users
                      join caregiver in _db.Caregiver on user.PersonID equals caregiver.CaregiverID
                      where user.Id == userId
                      select (int?)caregiver.CaregiverID).SingleOrDefaultAsync();
    }

    private async Task<int?> GetManagerAgencyIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await (from user in _db.Users
                      join manager in _db.AgencyManager on user.PersonID equals manager.AgencyManagerID
                      where user.Id == userId
                      select (int?)manager.AgencyID).SingleOrDefaultAsync();
    }

    private async Task LoadScheduleOptionsAsync(ScheduleVisitViewModel model, int agencyId)
    {
        model.Seniors = await _db.Senior
            .Where(s => s.Status == SeniorStatus.Active)
            .OrderBy(s => s.Person.LastName).ThenBy(s => s.Person.FirstName)
            .Select(s => new SelectListItem { Value = s.SeniorID.ToString(), Text = s.Person.FirstName + " " + s.Person.LastName })
            .ToListAsync();
        model.Caregivers = await _db.Caregiver
            .Where(c => c.AgencyID == agencyId)
            .OrderBy(c => c.Person.LastName).ThenBy(c => c.Person.FirstName)
            .Select(c => new SelectListItem { Value = c.CaregiverID.ToString(), Text = c.Person.FirstName + " " + c.Person.LastName })
            .ToListAsync();
        model.Services = await _db.Service.OrderBy(s => s.ServiceName)
            .Select(s => new SelectListItem { Value = s.ServiceID.ToString(), Text = s.ServiceName })
            .ToListAsync();
    }
}
