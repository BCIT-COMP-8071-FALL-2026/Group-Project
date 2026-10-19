namespace OLTPApp.Models;

public class AgencyManager
{
    public int AgencyManagerID { get; set; }        // PK and FK to Person
    public Person Person { get; set; } = null!;

    public int AgencyID { get; set; }               // the agency they manage
    public Agency Agency { get; set; } = null!;
}