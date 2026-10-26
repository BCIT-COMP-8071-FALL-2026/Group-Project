namespace OLTPApp.Models;

// One row of the caregiver's visit list (#62, OLTP-12)
public class CaregiverVisitListItem
{
    public int VisitID { get; set; }
    public DateTime ScheduledStart { get; set; }    // Vancouver time for display
    public string SeniorName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string ResidentAddress { get; set; } = null!;
    public string? DestinationAddress { get; set; }
}
