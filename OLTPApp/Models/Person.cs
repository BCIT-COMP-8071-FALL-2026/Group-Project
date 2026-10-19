using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models;

public class Person
{
    public int PersonID { get; set; }

    [MaxLength(100)] public string FirstName { get; set; } = null!;
    [MaxLength(100)] public string? MiddleName { get; set; }
    [MaxLength(100)] public string LastName { get; set; } = null!;
    [MaxLength(200)] public string Street { get; set; } = null!;
    [MaxLength(100)] public string City { get; set; } = null!;
    [MaxLength(100)] public string Province { get; set; } = null!;
    [MaxLength(100)] public string Country { get; set; } = null!;
    [MaxLength(20)] public string PostalCode { get; set; } = null!;
    [MaxLength(20)] public string Phone { get; set; } = null!;

    public Senior? Senior { get; set; }
    public Caregiver? Caregiver { get; set; }
    public FamilyMember? FamilyMember { get; set; }
    public AgencyManager? AgencyManager { get; set; }
}