// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OLTPApp.Authorization;
using OLTPApp.Data;
using OLTPApp.Models;

namespace OLTPApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$",
                ErrorMessage = "The password must include an uppercase letter, a lowercase letter, a number and a symbol.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            // Account type chosen in step 1: Senior, Family or Staff.
            // The pattern stops a crafted request from posting a value like "Admin".
            [Required]
            [RegularExpression("^(Senior|Family|Staff)$", ErrorMessage = "Choose Senior, Family or Staff.")]
            [Display(Name = "Account type")]
            public string AccountType { get; set; } = "Senior";

            // Person fields (names match the Person entity)
            [Required]
            [MaxLength(100)]
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [MaxLength(100)]
            [Display(Name = "Middle name")]
            public string MiddleName { get; set; }

            [Required]
            [MaxLength(100)]
            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Required]
            [MaxLength(20)]
            [Display(Name = "Phone number")]
            public string Phone { get; set; }

            [Required]
            [MaxLength(200)]
            [Display(Name = "Street address")]
            public string Street { get; set; }

            [Required]
            [MaxLength(100)]
            public string City { get; set; }

            [Required]
            [MaxLength(100)]
            [Display(Name = "Province / State / Region")]
            public string Province { get; set; }

            [Required]
            [MaxLength(20)]
            [Display(Name = "Postal / ZIP code")]
            public string PostalCode { get; set; }

            [Required]
            [MaxLength(100)]
            public string Country { get; set; }

            // Senior only (OLTP-2): required when AccountType is "Senior".
            // Nullable so Family and Staff can leave them out. Check them in OnPostAsync.
            [Display(Name = "Birthday")]
            public DateOnly? Birthday { get; set; }

            [Display(Name = "Living alone")]
            public bool? IsLivingAlone { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Check Senior fields are filled if account type is Senior
            if (Input.AccountType == "Senior")
            {
                if (Input.Birthday is null)
                    ModelState.AddModelError("Input.Birthday", "Enter your birthday.");
                if (Input.IsLivingAlone is null)
                    ModelState.AddModelError("Input.IsLivingAlone", "Select yes or no.");
            }

            if (ModelState.IsValid)
            {
                // Check duplicate email before creation
                if (await _userManager.FindByEmailAsync(Input.Email) is not null)
                {
                    ModelState.AddModelError("Input.Email", "An account with this email already exists.");
                    return Page();
                }

                // One transaction to create ApplicationUser plus Person and Senior/FamilyMember
                await using var transaction = await _context.Database.BeginTransactionAsync();

                // Create person
                var person = new Person
                {
                    FirstName = Input.FirstName,
                    MiddleName = Input.MiddleName,
                    LastName = Input.LastName,
                    Phone = Input.Phone,
                    Street = Input.Street,
                    City = Input.City,
                    Province = Input.Province,
                    PostalCode = Input.PostalCode,
                    Country = Input.Country
                };
                _context.Person.Add(person);
                await _context.SaveChangesAsync();

                // Create ApplicationUser
                var user = CreateUser();
                user.PersonID = person.PersonID; // Fill in ApplicationUser's PersonID

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {   
                    if (Input.AccountType == "Senior") // Create Senior
                    {
                        _context.Senior.Add(new Senior
                        {
                            SeniorID = person.PersonID,
                            DateOfBirth = Input.Birthday.Value,
                            IsLivingAlone = Input.IsLivingAlone.Value
                        });
                    }
                    else if (Input.AccountType == "Family") // Create FamilyMember
                    {
                        _context.FamilyMember.Add(new FamilyMember { FamilyMemberID = person.PersonID });
                    }

                    // Add role to user
                    var role = Input.AccountType == "Senior" ? RoleNames.Senior
                            : Input.AccountType == "Family" ? RoleNames.FamilyMember
                            : null;

                    if (role is not null)
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, role);
                        if (!roleResult.Succeeded)
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return Page();
                        }
                    }

                    // Complete transaction
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    var isDuplicate = error.Code is "DuplicateUserName" or "DuplicateEmail";
                    var key = isDuplicate ? "Input.Email"
                            : error.Code.StartsWith("Password") ? "Input.Password"
                            : string.Empty;

                    ModelState.AddModelError(key,
                        isDuplicate ? "An account with this email already exists." : error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
