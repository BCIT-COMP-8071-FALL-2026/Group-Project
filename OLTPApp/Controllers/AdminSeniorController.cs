
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLTPApp.Data;
using OLTPApp.Models;
using OLTPApp.ViewModels.Admin;

namespace OLTPApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSeniorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminSeniorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> Seniors(string? search, string? status)
        {
            var query = _context.Senior
                .Include(s => s.Person)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.Person.FirstName.Contains(search) ||
                    s.Person.LastName.Contains(search) ||
                    s.Person.Phone.Contains(search));
            }


            var seniors = await query
                .OrderBy(s => s.Person.LastName)
                .ThenBy(s => s.Person.FirstName)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.TotalSeniors = await _context.Senior.CountAsync();

            return View("~/Views/Admin/Seniors.cshtml", seniors);
        }

        [HttpGet]
        public async Task<IActionResult> EditSenior(int id)
        {
            return RedirectToAction(
                "Index",
                "SeniorProfile",
                new { seniorID = id }
            );
        }

        [HttpGet]
        public async Task<IActionResult> CreateSenior()
        {
            ViewBag.createNewSenior = true;
            return View("~/Views/Profile/Senior/CreateSenior.cshtml", new Senior());
        }

        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateSenior(Senior senior)
		{
			if (senior.Person == null)
				ModelState.AddModelError("Person", "Personal information is required.");

			foreach (var error in ModelState)
			{
				foreach (var e in error.Value!.Errors)
				{
					Console.WriteLine(
						$"Field: {error.Key}, Error: {e.ErrorMessage}");
				}
			}
			if (senior.DateOfBirth >= DateOnly.FromDateTime(DateTime.Today))
			{
				ModelState.AddModelError(
					nameof(senior.DateOfBirth),
					"Date of birth must be before today.");
			}
			if (!ModelState.IsValid)
			{
                ViewBag.createNewSenior = true;
				return View("~/Views/Profile/Senior/CreateSenior.cshtml", senior);
			}

			var person = senior.Person!;

			_context.Person.Add(person);
			await _context.SaveChangesAsync();

			senior.SeniorID = person.PersonID;
			senior.Person = person;
			senior.Status = SeniorStatus.Active;

			_context.Senior.Add(senior);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Seniors));
		}

        [HttpPost]
        public async Task<IActionResult> OpenDeactivation(int seniorID)
        {
            var senior = await _context.Senior
               .Include(s => s.Person)
               .FirstOrDefaultAsync(s => s.SeniorID == seniorID);

            if (senior == null)
                return NotFound("Cannot find current senior");

            if (senior.Status != SeniorStatus.Active)
                return BadRequest("Only active senior profiles can be deactivated or archived.");

            return View("~/Views/Profile/Deactivate/DeactivateSenior.cshtml", senior);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContinueDeactivation(int seniorID)
        {
            var senior = await _context.Senior
                .FirstOrDefaultAsync(s => s.SeniorID == seniorID);

            ViewBag.senior = senior;

            if (senior == null)
                return NotFound();

            if (senior.Status != SeniorStatus.Active)
                return BadRequest("The senior profile is not active.");

            return RedirectToAction(
                nameof(ChooseDeactivation),
                new { seniorID });
        }

        [HttpGet]
        public async Task<IActionResult> ChooseDeactivation(int seniorID)
        {
            var senior = await _context.Senior
                .FirstOrDefaultAsync(s => s.SeniorID == seniorID);

            ViewBag.senior = senior;

            if (senior == null)
                return NotFound();

            if (senior.Status != SeniorStatus.Active)
                return BadRequest("The senior profile is not active.");

            var model = new DeactivationChoiceViewModel
            {
                SeniorID = seniorID
            };

            return View(
                "~/Views/Profile/Deactivate/ChooseDeactivation.cshtml",
                model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseDeactivation(
            DeactivationChoiceViewModel model)
        {
            var senior = await _context.Senior
                .FirstOrDefaultAsync(s => s.SeniorID == model.SeniorID);

            ViewBag.senior = senior;

            if (senior == null)
                return NotFound();

            if (senior.Status != SeniorStatus.Active)
                return BadRequest("The senior profile is not active.");

            if (model.ActionType != "Deactivate")
            {
                ModelState.AddModelError(
                    nameof(model.ActionType),
                    "Please select a valid action.");
            }

            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Profile/Deactivate/ChooseDeactivation.cshtml",
                    model);
            }

            return RedirectToAction(
                nameof(ConfirmDeactivation),
                new
                {
                    seniorID = model.SeniorID,
                    actionType = model.ActionType
                });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmDeactivation( int seniorID, string actionType)
        {
            var senior = await _context.Senior
                .Include(s => s.Person)
                .FirstOrDefaultAsync(s => s.SeniorID == seniorID);

            ViewBag.senior = senior;

            if (senior == null)
                return NotFound();

            if (senior.Status != SeniorStatus.Active)
                return BadRequest("The senior profile is not active.");

            if (actionType != "Deactivate")
            {
                return BadRequest("Invalid action.");
            }

            var model = new ConfirmDeactivationViewModel
            {
                SeniorID = seniorID,
                ActionType = actionType
            };

            return View(
                "~/Views/Profile/Deactivate/ConfirmDeactivation.cshtml",
                model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeactivation(
            ConfirmDeactivationViewModel model)
        {
            var senior = await _context.Senior
                .Include(s => s.Person)
                .FirstOrDefaultAsync(s => s.SeniorID == model.SeniorID);
            ViewBag.senior = senior;

            if (senior == null)
                return NotFound();

            if (senior.Status != SeniorStatus.Active)
                return BadRequest("The senior profile is not active.");

            if (model.ActionType != "Deactivate")
            {
                return BadRequest("Invalid action.");
            }
            string name = $"{senior.Person.FirstName} {senior.Person.LastName}";
            if (!string.Equals(
                model.ConfirmationUsername?.Trim(),
                name,
                StringComparison.Ordinal))
            {
                ModelState.AddModelError(
                    nameof(model.ConfirmationUsername),
                    "The username does not match");
            }

            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Profile/Deactivate/ConfirmDeactivation.cshtml",
                    model);
            }

            senior.DeactivatedAt = DateTime.UtcNow;
            senior.Status = SeniorStatus.Deactivated;

            // OLTP-9: future Scheduled visits are cancelled
            var futureVisits = await _context.Visit
                .Where(v => v.SeniorID == senior.SeniorID
                            && v.Status == VisitStatus.Scheduled
                            && v.ScheduledStart > DateTime.UtcNow)
                .ToListAsync();
            foreach (var visit in futureVisits)
                visit.Status = VisitStatus.Cancelled;

            await _context.SaveChangesAsync();

            // OLTP-9: a deactivated senior cannot log in (same lockout as DbSeeder)
            var seniorUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PersonID == senior.SeniorID);
            if (seniorUser != null)
            {
                var lockResult = await _userManager.SetLockoutEndDateAsync(seniorUser, DateTimeOffset.MaxValue);
                if (!lockResult.Succeeded)
                    return BadRequest("The senior was deactivated, but their login could not be locked.");
            }

            return RedirectToAction(
                            "Index",
                            "SeniorProfile",
                            new { seniorID = senior.SeniorID }
                        );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int seniorId)
        {
            var senior = await _context.Senior
                .FirstOrDefaultAsync(s => s.SeniorID == seniorId);

            if (senior == null)
            {
                return NotFound();
            }

            if (senior.Status == SeniorStatus.Active)
            {
                return RedirectToAction(nameof(Seniors));
            }

            senior.Status = SeniorStatus.Active;
            senior.DeactivatedAt = null;

            await _context.SaveChangesAsync();

            // Undo the OLTP-9 lockout so the senior can log in again
            var seniorUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PersonID == senior.SeniorID);
            if (seniorUser != null)
            {
                var unlockResult = await _userManager.SetLockoutEndDateAsync(seniorUser, null);
                if (!unlockResult.Succeeded)
                    return BadRequest("The senior was activated, but their login could not be unlocked.");
            }

            return RedirectToAction(
                            "Index",
                            "SeniorProfile",
                            new { seniorID = senior.SeniorID }
                        );
        }



    }
}
