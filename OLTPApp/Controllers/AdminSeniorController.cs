
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
