using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using OLTPApp.Data;
using OLTPApp.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OLTPApp.Controllers
{
	[Authorize(Roles = "Senior, Admin")]
	public class SeniorProfileController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		private static readonly JsonSerializerOptions MedicationJsonOptions = new()
		{
			Converters = { new JsonStringEnumConverter() }
		};
		private static readonly JsonSerializerOptions ConditionJsonOptions = new()
		{
			Converters = { new JsonStringEnumConverter() }
		};

		public SeniorProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		private async Task<Senior?> GetCurrentSeniorAsync(int? seniorID)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user?.PersonID != null)
			{
				seniorID = user.PersonID;
			}
			else if (!User.IsInRole("Admin"))
			{
				return null;
			}

			return await _context.Senior
						.Include(s => s.Person)
						.Include(s => s.SeniorFamilyMembers)
							.ThenInclude(sf => sf.FamilyMember)
								.ThenInclude(fm => fm.Person)
						.FirstOrDefaultAsync(s => s.SeniorID == seniorID);
		}

        [HttpGet]
        public async Task<IActionResult> Index(int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
			{
				return Forbid();
			}

			var senior = await GetCurrentSeniorAsync(seniorID);
            if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}

            ViewBag.CurrentSenior = senior;
            var seniorUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonID == senior.Person.PersonID);
            ViewBag.SeniorEmail = seniorUser?.Email;
            ViewBag.OriginalSeniorEmail = seniorUser?.Email;

            return View("~/Views/Profile/Senior/Index.cshtml", senior);
		}


        [HttpGet]
        public async Task<IActionResult> EmergencyContacts(int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
			{
				return Forbid();
			}
			var senior = await GetCurrentSeniorAsync(seniorID);
			ViewBag.CurrentSenior = senior;


			if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/EmergencyContact/EmergencyContacts.cshtml", senior);
		}

        [HttpGet]
        public async Task<IActionResult> Medications(int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
			{
				return Forbid();
			}
			var senior = await GetCurrentSeniorAsync(seniorID);
			ViewBag.CurrentSenior = senior;


			if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/Medicine/Medications.cshtml", senior);
		}

        [HttpGet]
        public async Task<IActionResult> Conditions(int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
			{
				return Forbid();
			}
			var senior = await GetCurrentSeniorAsync(seniorID);
			ViewBag.CurrentSenior = senior;


			if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}

			return View(
				"~/Views/Profile/Condition/Conditions.cshtml",
				senior);
		}

		public async Task<IActionResult> AddCondition(int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
			{
				return Forbid();
			}
			var senior = await GetCurrentSeniorAsync(seniorID);
			ViewBag.CurrentSenior = senior;

			if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/Condition/AddCondition.cshtml", new MedicalConditionItem());
		}
       
		[HttpGet]
        public async Task<IActionResult> EditCondition(string id, int? seniorID)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            if (User.IsInRole("Admin") && seniorID == null)
                return Forbid();

            var senior = await GetCurrentSeniorAsync(seniorID);

            if (senior == null)
                return NotFound();

            ViewBag.CurrentSenior = senior;

            var conditions = new List<MedicalConditionItem>();

            if (!string.IsNullOrWhiteSpace(senior.MedicalConditions))
            {
                try
                {
                    conditions = JsonSerializer.Deserialize<List<MedicalConditionItem>>(
                        senior.MedicalConditions,
                        ConditionJsonOptions
                    ) ?? new List<MedicalConditionItem>();
                }
                catch (JsonException)
                {
                    return BadRequest("Unable to read the saved medical conditions.");
                }
            }

            var condition = conditions.FirstOrDefault(c => c.ID == id);

            if (condition == null)
                return NotFound();

            return View(
                "~/Views/Profile/Condition/EditCondition.cshtml",
                condition);
        }

        public async Task<IActionResult> EditMedicine(int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
			{
				return Forbid();
			}
			var senior = await GetCurrentSeniorAsync(seniorID);
			ViewBag.CurrentSenior = senior;


			if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/Medicine/EditMedicine.cshtml", senior);
		}

		public async Task<IActionResult> AddMedicine(int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
			{
				return Forbid();
			}
			var senior = await GetCurrentSeniorAsync(seniorID);
			ViewBag.CurrentSenior = senior;


			if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}
			return View("~/Views/Profile/Medicine/AddMedicine.cshtml", new MedicationItem());
		}

		[HttpGet]
		public async Task<IActionResult> EditEmergencyContact(int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
			{
				return Forbid();
			}
			var senior = await GetCurrentSeniorAsync(seniorID);
			ViewBag.CurrentSenior = senior;

			if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}

			return View(
				"~/Views/Profile/EmergencyContact/EditEmergencyContact.cshtml",senior
			);
		}

        [HttpGet]
        public async Task<IActionResult> AddEmergencyContact(int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
			{
				return Forbid();
			}
			var senior = await GetCurrentSeniorAsync(seniorID);

			ViewBag.CurrentSenior = senior;


			if (senior == null)
			{
				return View("~/Views/Profile/Senior/CreateSenior.cshtml");
			}


			return View("~/Views/Profile/EmergencyContact/AddEmergencyContact.cshtml", senior);
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


        [Authorize(Roles = "Senior,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPersonalInformation(int? seniorID)
        {
            bool isAdmin = User.IsInRole("Admin");

            if (isAdmin && seniorID == null)
                return Forbid();

            var senior = await GetCurrentSeniorAsync(seniorID);

            if (senior?.Person == null)
                return NotFound();

            var seniorUser = await _userManager.Users
                .FirstOrDefaultAsync(
                    u => u.PersonID == senior.Person.PersonID);

            if (seniorUser == null)
                return NotFound();

            string? originalEmail = seniorUser.Email;
            string newEmail = originalEmail ?? "";

            ViewBag.CurrentSenior = senior;
            ViewBag.OriginalSeniorEmail = originalEmail;
            ViewBag.SeniorEmail = originalEmail;
            ViewBag.KeepEditingPersonalInformation = false;

            // 1. Read and validate the email, but don't return yet.
            if (isAdmin)
            {
                newEmail = Request.Form["Email"].ToString().Trim();

                if (string.IsNullOrWhiteSpace(newEmail))
                {
                    ModelState.AddModelError(
                        "Email",
                        "Email is required.");
                }
                else
                {
                    var existingUser =
                        await _userManager.FindByEmailAsync(newEmail);

                    if (existingUser != null &&
                        existingUser.Id != seniorUser.Id)
                    {
                        ModelState.AddModelError(
                            "Email",
                            "This email is already used by another account.");
                    }
                }
            }

            bool updated;

            if (isAdmin)
            {
                updated = await TryUpdateModelAsync(
                    senior.Person,
                    "Person",
                    p => p.FirstName,
                    p => p.MiddleName,
                    p => p.LastName,
                    p => p.Phone,
                    p => p.Street,
                    p => p.City,
                    p => p.Province,
                    p => p.PostalCode,
                    p => p.Country
                );
            }
            else
            {
                updated = await TryUpdateModelAsync(
                    senior.Person,
                    "Person",
                    p => p.Phone,
                    p => p.Street,
                    p => p.City,
                    p => p.Province,
                    p => p.PostalCode,
                    p => p.Country
                );
            }

            if (!updated || !ModelState.IsValid)
            {
                ViewBag.CurrentSenior = senior;
                ViewBag.SeniorEmail = newEmail;
                ViewBag.OriginalSeniorEmail = originalEmail;
                ViewBag.KeepEditingPersonalInformation = true;

                return View("~/Views/Profile/Senior/Index.cshtml", senior);
            }

            if (isAdmin &&
                !string.Equals(
                    originalEmail,
                    newEmail,
                    StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await _userManager.SetEmailAsync(
                    seniorUser, newEmail);

                if (!emailResult.Succeeded)
                {
                    ModelState.AddModelError(
                        "Email",
                        string.Join(
                            " ",
                            emailResult.Errors.Select(e => e.Description)));

                    ViewBag.CurrentSenior = senior;
                    ViewBag.SeniorEmail = newEmail;
                    ViewBag.OriginalSeniorEmail = originalEmail;
                    ViewBag.KeepEditingPersonalInformation = true;

                    return View("~/Views/Profile/Senior/Index.cshtml", senior);
                }
            }

            await _context.SaveChangesAsync();

            if (isAdmin)
            {
                return RedirectToAction(
                    nameof(Index),
                    new { seniorID = senior.SeniorID });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditSeniorInformation(int? seniorID)
		{
            ViewBag.KeepEditingSeniorInformation = true;

            if (User.IsInRole("Admin") && seniorID == null)
				return Forbid();

			var senior = await GetCurrentSeniorAsync(seniorID);

			if (senior == null)
				return NotFound();

			bool isAdmin = User.IsInRole("Admin");
			bool updated;

			if (isAdmin)
			{
				updated = await TryUpdateModelAsync(
					senior,
					"",
					s => s.DateOfBirth,
					s => s.IsLivingAlone,
					s => s.CurrentDoctorName,
					s => s.CurrentDoctorPhone
                );
			}
			else
			{
				updated = await TryUpdateModelAsync(
					senior,
					"",
					s => s.IsLivingAlone,
					s => s.CurrentDoctorName,
					s => s.CurrentDoctorPhone
                );
			}

			if (!updated || !ModelState.IsValid)
			{
				ViewBag.CurrentSenior = senior;
				if (isAdmin)
				{
					return RedirectToAction(
						nameof(Index),
						new { seniorID = senior.SeniorID }
					);
				}
				return View("~/Views/Profile/Senior/Index.cshtml", senior);
			}

			await _context.SaveChangesAsync();
            ViewBag.KeepEditingSeniorInformation = false;

            if (isAdmin)
			{
				return RedirectToAction(
					nameof(Index),
					new { seniorID = senior.SeniorID }
				);
			}

			return RedirectToAction(nameof(Index));
		}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmergencyContact( Senior model, int? seniorID)
        {
            if (User.IsInRole("Admin") && seniorID == null)
                return Forbid();

            var senior = await GetCurrentSeniorAsync(seniorID);

            if (senior == null)
                return NotFound();

            ViewBag.CurrentSenior = senior;

            // Validate only the emergency contact fields.
            ModelState.Remove(nameof(Senior.SeniorID));
            ModelState.Remove(nameof(Senior.Person));

            if (string.IsNullOrWhiteSpace(model.EmergencyContactName))
            {
                ModelState.AddModelError(
                    nameof(Senior.EmergencyContactName),
                    "Contact name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.EmergencyContactPhone))
            {
                ModelState.AddModelError(
                    nameof(Senior.EmergencyContactPhone),
                    "Phone number is required.");
            }

            if (string.IsNullOrWhiteSpace(model.EmergencyContactRelationship))
            {
                ModelState.AddModelError(
                    nameof(Senior.EmergencyContactRelationship),
                    "Relationship is required.");
            }

            if (!ModelState.IsValid)
            {
                // Preserve the senior ID and display the submitted values.
                model.SeniorID = senior.SeniorID;

                return View(
                    "~/Views/Profile/EmergencyContact/AddEmergencyContact.cshtml",
                    model);
            }

            // Update the existing Senior record only.
            senior.EmergencyContactName = model.EmergencyContactName;
            senior.EmergencyContactPhone = model.EmergencyContactPhone;
            senior.EmergencyContactRelationship =
                model.EmergencyContactRelationship;

            await _context.SaveChangesAsync();

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(
                    nameof(EmergencyContacts),
                    new { seniorID = senior.SeniorID });
            }

            return RedirectToAction(nameof(EmergencyContacts));
        }

        [HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmergencyContact(Senior model, int? seniorID)
        {
            if (User.IsInRole("Admin") && seniorID == null)
                return Forbid();

            var senior = await GetCurrentSeniorAsync(seniorID);

            if (senior == null)
                return NotFound();

            ViewBag.CurrentSenior = senior;

            ModelState.Remove(nameof(Senior.SeniorID));
            ModelState.Remove(nameof(Senior.Person));

            if (string.IsNullOrWhiteSpace(model.EmergencyContactName))
            {
                ModelState.AddModelError(
                    nameof(Senior.EmergencyContactName),
                    "Contact name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.EmergencyContactPhone))
            {
                ModelState.AddModelError(
                    nameof(Senior.EmergencyContactPhone),
                    "Phone number is required.");
            }

            if (string.IsNullOrWhiteSpace(model.EmergencyContactRelationship))
            {
                ModelState.AddModelError(
                    nameof(Senior.EmergencyContactRelationship),
                    "Relationship is required.");
            }

            if (!ModelState.IsValid)
            {
                model.SeniorID = senior.SeniorID;

                return View(
                    "~/Views/Profile/EmergencyContact/EditEmergencyContact.cshtml",
                    model);
            }

            senior.EmergencyContactName = model.EmergencyContactName;
            senior.EmergencyContactPhone = model.EmergencyContactPhone;
            senior.EmergencyContactRelationship = model.EmergencyContactRelationship;

            await _context.SaveChangesAsync();

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(
                    nameof(EmergencyContacts),
                    new { seniorID = senior.SeniorID });
            }

            return RedirectToAction(nameof(EmergencyContacts));
        }

        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddMedicine(MedicationItem medication, int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
				return Forbid();
			var senior = await GetCurrentSeniorAsync(seniorID);
			if (senior == null)
				return NotFound();
			ViewBag.CurrentSenior = senior;

			if (!ModelState.IsValid)
			{
				return View(
					"~/Views/Profile/Medicine/AddMedicine.cshtml",
					medication
				);
			}

			List<MedicationItem> medications;
			try
			{
				medications = string.IsNullOrWhiteSpace(senior.CurrentMedication)
					? new List<MedicationItem>()
					: JsonSerializer.Deserialize<List<MedicationItem>>(
						senior.CurrentMedication,
						MedicationJsonOptions
					  ) ?? new List<MedicationItem>();
			}
			catch (JsonException)
			{
				ModelState.AddModelError(
					string.Empty,
					"The existing medication data could not be read. No changes were saved."
				);

				return View(
					"~/Views/Profile/Medicine/AddMedicine.cshtml",
					medication
				);
			}

			medication.ID = Guid.NewGuid().ToString();
			medications.Add(medication);
			senior.CurrentMedication = JsonSerializer.Serialize(
				medications,
				MedicationJsonOptions
			);

			await _context.SaveChangesAsync();

			if (User.IsInRole("Admin"))
			{
				return RedirectToAction(
					nameof(Medications),
					new { seniorID = senior.SeniorID }
				);
			}

			return RedirectToAction(nameof(Medications));
		}


		[HttpGet]
		public async Task<IActionResult> EditMedicine(string id, int? seniorID)
		{
			if (string.IsNullOrWhiteSpace(id))
				return BadRequest();

			if (User.IsInRole("Admin") && seniorID == null)
				return Forbid();

			var senior = await GetCurrentSeniorAsync(seniorID);

			if (senior == null)
				return NotFound();

			ViewBag.CurrentSenior = senior;

			List<MedicationItem> medications;

			try
			{
				medications = string.IsNullOrWhiteSpace(senior.CurrentMedication)
					? new List<MedicationItem>()
					: JsonSerializer.Deserialize<List<MedicationItem>>(
						senior.CurrentMedication,
						MedicationJsonOptions
					  ) ?? new List<MedicationItem>();
			}
			catch (JsonException)
			{
				return BadRequest("The medication data could not be read.");
			}

			var medication = medications.FirstOrDefault(m => m.ID == id);

			if (medication == null)
				return NotFound();

			return View(
				"~/Views/Profile/Medicine/EditMedicine.cshtml",
				medication
			);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditMedicine(
			MedicationItem medication,
			int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
				return Forbid();

			var senior = await GetCurrentSeniorAsync(seniorID);

			if (senior == null)
				return NotFound();

			ViewBag.CurrentSenior = senior;

			if (!ModelState.IsValid)
			{
				return View(
					"~/Views/Profile/MedicineInvalidOperationException: The view '~/Views/Profile/Medicine/EditMedicine.cshtml' was not found. The following locations were searched:\r\n/EditMedicine.cshtml",
					medication
				);
			}

			List<MedicationItem> medications;

			try
			{
				medications = string.IsNullOrWhiteSpace(senior.CurrentMedication)
					? new List<MedicationItem>()
					: JsonSerializer.Deserialize<List<MedicationItem>>(
						senior.CurrentMedication,
						MedicationJsonOptions
					  ) ?? new List<MedicationItem>();
			}
			catch (JsonException)
			{
				return BadRequest("The medication data could not be read.");
			}

			var existingMedication = medications.FirstOrDefault(
				m => m.ID == medication.ID
			);

			if (existingMedication == null)
				return NotFound();

			existingMedication.Name = medication.Name;
			existingMedication.Dose = medication.Dose;
			existingMedication.Frequency = medication.Frequency;
			existingMedication.PrescribedBy = medication.PrescribedBy;
			existingMedication.StartDate = medication.StartDate;
			existingMedication.NextRefillDate = medication.NextRefillDate;
			existingMedication.Status = medication.Status;

			senior.CurrentMedication = JsonSerializer.Serialize(
				medications,
				MedicationJsonOptions
			);

			await _context.SaveChangesAsync();

			if (User.IsInRole("Admin"))
			{
				return RedirectToAction(
					nameof(Medications),
					new { seniorID = senior.SeniorID }
				);
			}

			return RedirectToAction(nameof(Medications));
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddCondition(MedicalConditionItem condition, int? seniorID)
		{
			if (User.IsInRole("Admin") && seniorID == null)
				return Forbid();

			var senior = await GetCurrentSeniorAsync(seniorID);

			if (senior == null)
				return NotFound();

			ViewBag.CurrentSenior = senior;

			if (!ModelState.IsValid)
			{
				return View(
					"~/Views/Profile/Condition/AddCondition.cshtml",
					condition
				);
			}

			List<MedicalConditionItem> conditions;

			try
			{
				conditions = string.IsNullOrWhiteSpace(senior.MedicalConditions)
					? new List<MedicalConditionItem>()
					: JsonSerializer.Deserialize<List<MedicalConditionItem>>(
						senior.MedicalConditions,
						ConditionJsonOptions
					  ) ?? new List<MedicalConditionItem>();
			}
			catch (JsonException)
			{
				ModelState.AddModelError(
					string.Empty,
					"Existing medical condition data could not be read. No changes were saved."
				);

				return View(
					"~/Views/Profile/Condition/AddCondition.cshtml",
					condition
				);
			}

			condition.ID = Guid.NewGuid().ToString();

			conditions.Add(condition);

			senior.MedicalConditions = JsonSerializer.Serialize(
				conditions,
				ConditionJsonOptions
			);

			await _context.SaveChangesAsync();

			if (User.IsInRole("Admin"))
			{
				return RedirectToAction(
					nameof(Conditions),
					new { seniorID = senior.SeniorID }
				);
			}

			return RedirectToAction(nameof(Conditions));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditCondition(string id, MedicalConditionItem condition, int? seniorID)
		{
			if (string.IsNullOrWhiteSpace(id))
				return BadRequest();

			if (User.IsInRole("Admin") && seniorID == null)
				return Forbid();

			var senior = await GetCurrentSeniorAsync(seniorID);

			if (senior == null)
				return NotFound();

			ViewBag.CurrentSenior = senior;

			condition.ID = id;

			if (!ModelState.IsValid)
			{
				return View(
					"~/Views/Profile/Condition/EditCondition.cshtml",
					condition);
			}

			List<MedicalConditionItem> conditions;

			try
			{
				conditions = string.IsNullOrWhiteSpace(senior.MedicalConditions)
					? new List<MedicalConditionItem>()
					: JsonSerializer.Deserialize<List<MedicalConditionItem>>(
						senior.MedicalConditions,
						ConditionJsonOptions
					) ?? new List<MedicalConditionItem>();
			}
			catch (JsonException)
			{
				ModelState.AddModelError(
					string.Empty,
					"The existing medical condition data could not be read. No changes were saved.");

				return View(
					"~/Views/Profile/Condition/EditCondition.cshtml",
					condition);
			}

			var existingCondition = conditions.FirstOrDefault(c => c.ID == id);

			if (existingCondition == null)
				return NotFound();

			// Update only the selected condition.
			existingCondition.Name = condition.Name;
			existingCondition.DiagnosisDate = condition.DiagnosisDate;
			existingCondition.Description = condition.Description;
			existingCondition.Status = condition.Status;

			senior.MedicalConditions = JsonSerializer.Serialize(
				conditions,
				ConditionJsonOptions);

			await _context.SaveChangesAsync();

			if (User.IsInRole("Admin"))
			{
				return RedirectToAction(
					nameof(Conditions),
					new { seniorID = senior.SeniorID });
			}

			return RedirectToAction(nameof(Conditions));
		}
	}
}
