using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models;

public enum VisitStatus { Scheduled, InProgress, Completed, Missed, Cancelled }

public class Visit
{
    public int VisitID { get; set; }

    public int SeniorID { get; set; }
    public Senior Senior { get; set; } = null!;

    public int ServiceID { get; set; }              // visit type
    public Service Service { get; set; } = null!;

    public int CaregiverID { get; set; }            // assigned caregiver
    public Caregiver Caregiver { get; set; } = null!;

    public int AgencyID { get; set; }               // snapshot of Caregiver.AgencyID at scheduling
    public Agency Agency { get; set; } = null!;

    [MaxLength(450)] public string CreatedByUserID { get; set; } = null!;   // agency manager who scheduled
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public DateTime? ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }

    public VisitStatus Status { get; set; } = VisitStatus.Scheduled;
    [MaxLength(500)] public string? CancelReason { get; set; }
    [MaxLength(300)] public string? DestinationAddress { get; set; }
}