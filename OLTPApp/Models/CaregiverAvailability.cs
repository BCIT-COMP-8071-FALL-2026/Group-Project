namespace OLTPApp.Models;

public class CaregiverAvailability
{
    public int AvailabilityID { get; set; }

    public int CaregiverID { get; set; }
    public Caregiver Caregiver { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }        // Sunday = 0 ... Saturday = 6, stored as int
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}