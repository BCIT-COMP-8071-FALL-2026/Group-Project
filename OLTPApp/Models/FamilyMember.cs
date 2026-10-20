namespace OLTPApp.Models;

public class FamilyMember
{
    public int FamilyMemberID { get; set; }         // PK and FK to Person
    public Person Person { get; set; } = null!;

    public ICollection<SeniorFamilyMember> SeniorFamilyMembers { get; set; } = new List<SeniorFamilyMember>();
}