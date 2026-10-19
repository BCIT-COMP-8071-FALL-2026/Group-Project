namespace OLTPApp.Models;

public class FamilyMember
{
    public int FamilyMemberID { get; set; }         // PK and FK to Person
    public Person Person { get; set; } = null!;
}