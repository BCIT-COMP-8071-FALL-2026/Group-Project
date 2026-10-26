
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLTPApp.Data;
using OLTPApp.Models;

namespace OLTPApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSeniorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminSeniorController(ApplicationDbContext context)
        {
            _context = context;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int seniorId)
        {
            var senior = await _context.Senior
                .FirstOrDefaultAsync(s => s.SeniorID == seniorId);

            if (senior == null)
            {
                return NotFound();
            }

            if (senior.Status == SeniorStatus.Deactivated)
            {
                return RedirectToAction(nameof(Seniors));
            }

            senior.Status = SeniorStatus.Deactivated;
            senior.DeactivatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Seniors));
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

            return RedirectToAction(nameof(Seniors));
        }



    }
}
