using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using OLTPApp.Data;
using OLTPApp.Models;
using System.Security.Claims;

namespace OLTPApp.Controllers
{
	public class SeniorProfileController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public SeniorProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		private async Task<Senior?> GetCurrentSeniorAsync()
		{
			var user = await _userManager.GetUserAsync(User);

			if (user?.PersonID == null)
				return null;

            return await _context.Senior
                        .Include(s => s.Person)
                        .Include(s => s.SeniorFamilyMembers)
                            .ThenInclude(sf => sf.FamilyMember)
                                .ThenInclude(fm => fm.Person)
                        .FirstOrDefaultAsync(s => s.SeniorID == user.PersonID);
        }

		public async Task<IActionResult> Index()
		{
			var senior = await GetCurrentSeniorAsync();
			ViewBag.CurrentSenior = senior;           


            if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/Senior/Index.cshtml", senior);
		}


		public async Task<IActionResult> EmergencyContacts()
		{
            var senior = await GetCurrentSeniorAsync();
			ViewBag.CurrentSenior = senior;           


            if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/EmergencyContact/EmergencyContacts.cshtml",senior);
		}

		public async Task<IActionResult> Medications()
		{
            
            var senior = await GetCurrentSeniorAsync();
			ViewBag.CurrentSenior = senior;           


            if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/Medicine/Medications.cshtml",senior);
		}

		public async Task<IActionResult> Conditions()
		{
            
            var senior = await GetCurrentSeniorAsync();
			ViewBag.CurrentSenior = senior;           


            if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/Condition/Conditions.cshtml",senior);
		}

		public async Task<IActionResult> AddCondition()
		{
            
            var senior = await GetCurrentSeniorAsync();
			ViewBag.CurrentSenior = senior;           

            if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/Condition/AddCondition.cshtml",senior);
		}
		public async Task<IActionResult> EditCondition()
		{
            
            var senior = await GetCurrentSeniorAsync();
			ViewBag.CurrentSenior = senior;           


            if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/Condition/EditCondition.cshtml",senior);
		}

		public async Task<IActionResult> EditMedicine()
		{
            
            var senior = await GetCurrentSeniorAsync();
			ViewBag.CurrentSenior = senior;           


            if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/Medicine/EditMedicine.cshtml",senior);
		}

		public async Task<IActionResult> AddMedicine()
		{
            
            var senior = await GetCurrentSeniorAsync();
			ViewBag.CurrentSenior = senior;           


            if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/Medicine/AddMedicine.cshtml",senior);
		}

        public async Task<IActionResult> EditEmergencyContact(int memberID)
        {
            var senior = await GetCurrentSeniorAsync();
            ViewBag.CurrentSenior = senior;

            if (senior == null)
            {
                return View("~/Views/Profile/Senior/CreateSenior.cshtml");
            }

            var contactLink = await _context.SeniorFamilyMember
                .Include(sf => sf.FamilyMember)
                    .ThenInclude(fm => fm.Person)
                .FirstOrDefaultAsync(sf =>
                    sf.SeniorID == senior.SeniorID &&
                    sf.FamilyMemberID == memberID);

            if (contactLink?.FamilyMember?.Person == null)
                return NotFound();

            ViewBag.Relationship = contactLink.Relationship;

            return View(
                "~/Views/Profile/EmergencyContact/EditEmergencyContact.cshtml",
                contactLink.FamilyMember.Person
            );
        }

        public async Task<IActionResult> AddEmergencyContact()
		{
            
            var senior = await GetCurrentSeniorAsync();
			ViewBag.CurrentSenior = senior;           


            if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}


			return View("~/Views/Profile/EmergencyContact/AddEmergencyContact.cshtml", new Person());
		}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSenior(Senior senior)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (user.PersonID != null)
                return RedirectToAction(nameof(Index));

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

            user.PersonID = person.PersonID;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("~/Views/Profile/Senior/CreateSenior.cshtml", senior);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPersonalInformation()
        {
            var senior = await GetCurrentSeniorAsync();

            if (senior?.Person == null)
                return NotFound();

            var updated = await TryUpdateModelAsync(
                senior.Person,
                "Person",
                p => p.Phone,
                p => p.Street,
                p => p.City,
                p => p.Province,
                p => p.PostalCode,
                p => p.Country
            );

            if (!updated)
            {
                ViewBag.CurrentSenior = senior;
                return View("~/Views/Profile/Senior/Index.cshtml", senior);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSeniorInformation()
        {
            var senior = await GetCurrentSeniorAsync();

            if (senior == null)
                return NotFound();

            var updated = await TryUpdateModelAsync(
                senior,
                "",
                s => s.IsLivingAlone,
                s => s.CurrentDoctorName,
                s => s.MedicalConditions
            );

            if (!updated)
            {
                ViewBag.CurrentSenior = senior;
                return View("~/Views/Profile/Senior/Index.cshtml", senior);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmergencyContact(
            Person person, string relationship)
        {
            var senior = await GetCurrentSeniorAsync();

            if (senior == null)
                return NotFound();
            ViewBag.CurrentSenior = senior;

            ModelState.Remove(nameof(Person.PersonID));
            ModelState.Remove(nameof(Person.Senior));
            ModelState.Remove(nameof(Person.Caregiver));
            ModelState.Remove(nameof(Person.FamilyMember));
            ModelState.Remove(nameof(Person.AgencyManager));

            if (string.IsNullOrWhiteSpace(relationship))
            {
                ModelState.AddModelError(
                    "Relationship", "Relationship is required.");
            }

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine(
                            $"{entry.Key}: {error.ErrorMessage}");
                    }
                }
                return View(
                    "~/Views/Profile/EmergencyContact/AddEmergencyContact.cshtml",
                    person);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Person.Add(person);
                await _context.SaveChangesAsync();

                var familyMember = new FamilyMember
                {
                    FamilyMemberID = person.PersonID
                };

                _context.FamilyMember.Add(familyMember);
                await _context.SaveChangesAsync();

                var link = new SeniorFamilyMember
                {
                    SeniorID = senior.SeniorID,
                    FamilyMemberID = familyMember.FamilyMemberID,
                    Relationship = relationship
                };

                _context.SeniorFamilyMember.Add(link);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return RedirectToAction(nameof(EmergencyContacts));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmergencyContact( Person person, string relationship)
        {
            var senior = await GetCurrentSeniorAsync();

            if (senior == null)
                return NotFound();

            var contactLink = await _context.SeniorFamilyMember
                .Include(sf => sf.FamilyMember)
                    .ThenInclude(fm => fm.Person)
                .FirstOrDefaultAsync(sf =>
                    sf.SeniorID == senior.SeniorID &&
                    sf.FamilyMemberID == person.PersonID);

            if (contactLink?.FamilyMember?.Person == null)
                return NotFound();

            ModelState.Remove(nameof(Person.Senior));
            ModelState.Remove(nameof(Person.Caregiver));
            ModelState.Remove(nameof(Person.FamilyMember));
            ModelState.Remove(nameof(Person.AgencyManager));

            if (string.IsNullOrWhiteSpace(relationship))
            {
                ModelState.AddModelError(
                    "relationship",
                    "Relationship is required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Relationship = relationship;

                return View(
                    "~/Views/Profile/EmergencyContacts/EditEmergencyContact.cshtml",
                    person);
            }

            var existingPerson = contactLink.FamilyMember.Person;

            existingPerson.FirstName = person.FirstName;
            existingPerson.LastName = person.LastName;
            existingPerson.Phone = person.Phone;
            existingPerson.Street = person.Street;
            existingPerson.City = person.City;
            existingPerson.Province = person.Province;
            existingPerson.PostalCode = person.PostalCode;
            existingPerson.Country = person.Country;

            contactLink.Relationship = relationship.Trim();

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(EmergencyContacts));
        }



    }


}
