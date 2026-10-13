using System.ComponentModel.DataAnnotations;

namespace OLTPApp.Models;

public class Service
{
    public int ServiceID { get; set; }

    [MaxLength(100)] public string ServiceName { get; set; } = null!;
    public string? Description { get; set; }
}