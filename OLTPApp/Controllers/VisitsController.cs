using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OLTPApp.Controllers;

// #61: shared by visit pages. Each action adds its own role policy:
// [Authorize(Policy = Policies.AgencyManager)] for scheduling (#61) and agency visits (#65),
// [Authorize(Policy = Policies.Caregiver)] for the visit list (#62) and visit page (#63).
[Authorize]
public class VisitsController : Controller
{
}
