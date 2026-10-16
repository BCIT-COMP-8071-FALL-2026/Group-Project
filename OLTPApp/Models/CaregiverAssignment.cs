namespace OLTPApp.Models;

public class CaregiverAssignment
{
    public int AssignmentID { get; set; }

    public int SeniorID { get; set; }
    public Senior Senior { get; set; } = null!;

    public int CaregiverID { get; set; }
    public Caregiver Caregiver { get; set; } = null!;

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }          // null = ongoing
}