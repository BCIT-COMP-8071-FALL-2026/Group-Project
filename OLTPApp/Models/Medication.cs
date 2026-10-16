using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models;

public class Medication
{
    public int MedicationID { get; set; }

    public int SeniorID { get; set; }
    public Senior Senior { get; set; } = null!;

    [MaxLength(200)] public string Name { get; set; } = null!;
    [MaxLength(100)] public string Dosage { get; set; } = null!;
    [MaxLength(100)] public string Frequency { get; set; } = null!;
    [MaxLength(200)] public string PrescribedBy { get; set; } = null!;   // plain text
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly? RefillDate { get; set; }
    public bool IsActive { get; set; }
}