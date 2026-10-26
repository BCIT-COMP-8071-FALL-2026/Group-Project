#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OLTPApp.Areas.Identity.Pages.Account
{
    // Overrides the Identity UI page so a role check failure returns HTTP 403 (non-functional requirements)
    [AllowAnonymous]
    public class AccessDeniedModel : PageModel
    {
        public void OnGet()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
        }
    }
}
