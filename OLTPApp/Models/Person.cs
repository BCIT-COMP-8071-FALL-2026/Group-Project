using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models;

public class Person
{
    public int PersonID { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100)] public string FirstName { get; set; } = null!;
    [MaxLength(100)] public string? MiddleName { get; set; }
    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100)] public string LastName { get; set; } = null!;
    [Required(ErrorMessage = "Street information is required")]
    [MaxLength(200)] public string Street { get; set; } = null!;
    [Required(ErrorMessage = "City information is required")]
    [MaxLength(100)] public string City { get; set; } = null!;
    [Required(ErrorMessage = "Province information is required")]
    [MaxLength(100)] public string Province { get; set; } = null!;
    [Required(ErrorMessage = "Country information is required")]
    [MaxLength(100)] public string Country { get; set; } = null!;
    [Required(ErrorMessage = "Postal code information is required")]
    [MaxLength(20)] public string PostalCode { get; set; } = null!;
    [Required(ErrorMessage = "Phone number is required")]
    [MaxLength(20, ErrorMessage ="Phone number cannot exceed 20 characters")] public string Phone { get; set; } = null!;

    public Senior? Senior { get; set; }
    public Caregiver? Caregiver { get; set; }
    public FamilyMember? FamilyMember { get; set; }
    public AgencyManager? AgencyManager { get; set; }
}