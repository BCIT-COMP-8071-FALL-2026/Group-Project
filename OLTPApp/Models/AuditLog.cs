using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace OLTPApp.Models;

public class AuditLog
{
    public int AuditLogID { get; set; }

    [MaxLength(450)] public string UserAccountID { get; set; } = null!;
    public IdentityUser UserAccount { get; set; } = null!;

    [MaxLength(50)] public string Action { get; set; } = null!;     //AdminPromotion, SensitiveRead, etc
    [MaxLength(100)] public string EntityName { get; set; } = null!;
    [MaxLength(50)] public string EntityID { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}