namespace OLTPApp.Models;

public class AgencyVisitsViewModel
{
    public string AgencyName { get; set; } = null!;
    public List<AgencyVisitListItem> Visits { get; set; } = new();
}