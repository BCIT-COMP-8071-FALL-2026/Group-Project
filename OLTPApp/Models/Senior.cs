using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models;

public enum SeniorStatus { Active, Deactivated }

public class Senior
{
    public int SeniorID { get; set; }               // PK and FK to Person
    public Person Person { get; set; } = null!;

    [Required(ErrorMessage = "Date of birth is required")]
    public DateOnly DateOfBirth { get; set; }
    [Required(ErrorMessage = "Please select whether you live alone.")]
    public bool IsLivingAlone { get; set; }
    [MaxLength(200)] public string? CurrentDoctorName { get; set; }
    [MaxLength(20)] public string? CurrentDoctorPhone { get; set; }
    public string? MedicalConditions { get; set; }
    public string? CurrentMedication { get; set; }
    [MaxLength(200)] public string? EmergencyContactName { get; set; }
    [MaxLength(20)] public string? EmergencyContactPhone { get; set; }
    [MaxLength(50)] public string? EmergencyContactRelationship { get; set; }

    public SeniorStatus Status { get; set; } = SeniorStatus.Active;
    public DateTime? DeactivatedAt { get; set; }

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<SeniorFamilyMember> SeniorFamilyMembers { get; set; } = new List<SeniorFamilyMember>();
}