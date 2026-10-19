namespace OLTPApp.Models;

public class Caregiver
{
    public int CaregiverID { get; set; }            // PK and FK to Person
    public Person Person { get; set; } = null!;

    public int AgencyID { get; set; }               // exactly one agency
    public Agency Agency { get; set; } = null!;
}