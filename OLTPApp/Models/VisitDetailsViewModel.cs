using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OLTPApp.Services;

namespace OLTPApp.Models;

public class VisitDetailsViewModel
{
    [BindNever] public int VisitID { get; set; }
    [BindNever] public string SeniorName { get; set; } = null!;
    [BindNever] public string ResidentAddress { get; set; } = null!;
    [BindNever] public string ServiceName { get; set; } = null!;
    [BindNever] public string? DestinationAddress { get; set; }
    [BindNever] public DateTime ScheduledStart { get; set; }
    [BindNever] public DateTime? ActualCheckIn { get; set; }
    [BindNever] public DateTime? ActualCheckOut { get; set; }
    [BindNever] public VisitDisplayStatus Status { get; set; }

    public bool CanCheckIn => Status == VisitDisplayStatus.Scheduled;
    public bool CanCheckOut => Status == VisitDisplayStatus.InProgress;

    [BindNever] public List<NoteItem> Notes { get; set; } = new();

    [Required(ErrorMessage = "Enter a note.")]
    [MaxLength(2000)]
    [Display(Name = "Add a note")]
    public string? NewNote { get; set; }

    public record NoteItem(DateTime CreatedAt, string AuthorName, string Text);
}