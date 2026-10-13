using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models;

public class FamilyMember
{
    public int FamilyMemberID { get; set; }         // PK and FK to Person
    public Person Person { get; set; } = null!;

    [MaxLength(20)] public string? WorkPhone { get; set; }
}