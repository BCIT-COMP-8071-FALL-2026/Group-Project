using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OLTPApp.Models;

public class VisitDetailsViewModel
{
    [BindNever] public int VisitID { get; set; }
    [BindNever] public string SeniorName { get; set; }
    [BindNever] public string ResidentAddress { get; set; }
    [BindNever] public string ServiceName { get; set; }
    [BindNever] public string? DestinationAddress { get; set; }
    [BindNever] public DateTime ScheduledStart { get; set; }
    [BindNever] public DateTime? ActualCheckIn { get; set; }
    [BindNever] public DateTime? ActualCheckOut { get; set; }
    [BindNever] public VisitStatus Status { get; set; }
    [BindNever] public bool IsMissed { get; set; }

    public bool CanCheckIn => Status == VisitStatus.Scheduled;
    public bool CanCheckOut => Status == VisitStatus.InProgress;

    [BindNever] public List<NoteItem> Notes { get; set; } = new();

    [Required(ErrorMessage = "Enter a note.")]
    [MaxLength(2000)]
    [Display(Name = "Add a note")]
    public string? NewNote { get; set; }

    public record NoteItem(DateTime CreatedAt, string AuthorName, string Text);
}