namespace OLTPApp.Models;

public class VisitNote
{
    public int NoteID { get; set; }

    public int VisitID { get; set; }
    public Visit Visit { get; set; } = null!;

    public int CaregiverID { get; set; }            // author
    public Caregiver Caregiver { get; set; } = null!;

    public string NoteText { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsArchived { get; set; }
}