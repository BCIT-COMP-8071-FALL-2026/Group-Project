using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OLTPApp.Models;

public class ScheduleVisitViewModel
{
    [Required(ErrorMessage = "Choose a currentsenior.")]
    [Display(Name = "Senior")]
    public int? SeniorID { get; set; }

    [Required(ErrorMessage = "Choose a caregiver.")]
    [Display(Name = "Caregiver")]
    public int? CaregiverID { get; set; }

    [Required(ErrorMessage = "Choose a visit type.")]
    [Display(Name = "Visit type")]
    public int? ServiceID { get; set; }

    [Required(ErrorMessage = "Enter a date and time.")]
    [Display(Name = "Date and time (Vancouver time)")]
    public DateTime? ScheduledStart { get; set; }

    [MaxLength(300)]
    [Display(Name = "Destination address (optional)")]
    public string? DestinationAddress { get; set; }

    [BindNever]
    public List<SelectListItem> Seniors { get; set; } = new();
    [BindNever]
    public List<SelectListItem> Caregivers { get; set; } = new();
    [BindNever]
    public List<SelectListItem> Services { get; set; } = new();
}
