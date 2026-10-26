using OLTPApp.Services;

namespace OLTPApp.Models;

public class AgencyVisitListItem
{
    public int VisitID { get; set; }
    public DateTime ScheduledStart { get; set; } // Vancouver time for display
    public string SeniorName { get; set; } = null!;
    public string CaregiverName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public VisitDisplayStatus Status { get; set; }
}
