using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models;

public class Agency
{
    public int AgencyID { get; set; }

    [MaxLength(200)] public string Name { get; set; } = null!;
    [MaxLength(200)] public string Street { get; set; } = null!;
    [MaxLength(100)] public string City { get; set; } = null!;
    [MaxLength(100)] public string Province { get; set; } = null!;
    [MaxLength(100)] public string Country { get; set; } = null!;
    [MaxLength(20)] public string PostalCode { get; set; } = null!;
    [MaxLength(20)] public string Phone { get; set; } = null!;
    [MaxLength(256)] public string Email { get; set; } = null!;

    public ICollection<Caregiver> Caregivers { get; set; } = new List<Caregiver>();
}