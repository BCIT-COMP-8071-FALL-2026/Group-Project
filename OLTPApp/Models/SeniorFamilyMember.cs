using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models;

public class SeniorFamilyMember
{
    public int SeniorID { get; set; }
    public Senior Senior { get; set; } = null!;

    public int FamilyMemberID { get; set; }
    public FamilyMember FamilyMember { get; set; } = null!;

    [MaxLength(50)] public string Relationship { get; set; } = null!;
    public bool IsEmergencyContact { get; set; }
    public bool IsPowerOfAttorney { get; set; }
    public DateTime? AccessApprovedAt { get; set; }   // senior approval (OLTP-11)
    public bool CanViewHealth { get; set; }
}