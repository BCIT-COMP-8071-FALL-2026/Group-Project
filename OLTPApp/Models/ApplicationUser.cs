using Microsoft.AspNetCore.Identity;

namespace OLTPApp.Models;

public class ApplicationUser : IdentityUser
{
    public int? PersonID { get; set; }    // null until OLTP-2 creates the Person
    public Person? Person { get; set; }
}