using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models;

public enum SeniorStatus { Active, PendingDeactivation, Deactivated, Archived }

public class Senior
{
    public int SeniorID { get; set; }               // PK and FK to Person
    public Person Person { get; set; } = null!;

    public byte[] SINEncrypted { get; set; } = null!;
    [MaxLength(3)] public string SINLast3 { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public bool IsLivingAlone { get; set; }
    public string? AdditionalNotes { get; set; }
    [MaxLength(200)] public string? CurrentDoctorName { get; set; }
    [MaxLength(20)] public string? CurrentDoctorPhone { get; set; }
    [MaxLength(200)] public string? InsuranceCompany { get; set; }
    public byte[]? InsuranceAccountNoEncrypted { get; set; }
    public string? MedicalConditions { get; set; }

    public SeniorStatus Status { get; set; } = SeniorStatus.Active;
    public DateTime? DeactivationRequestedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
    public DateOnly? RetainUntil { get; set; }
}