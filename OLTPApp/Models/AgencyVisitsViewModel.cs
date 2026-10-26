namespace OLTPApp.Models;

public class AgencyVisitsViewModel
{
    public string AgencyName { get; set; } = null!;
    public List<AgencyVisitListItem> Visits { get; set; } = new();

    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}