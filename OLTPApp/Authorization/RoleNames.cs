namespace OLTPApp.Authorization;

// Role names from docs/Roles-and-Permissions.md (OLTP-5); the only place role strings live
public static class RoleNames
{
    public const string Senior = "Senior";
    public const string FamilyMember = "Family Member";
    public const string Caregiver = "Caregiver";
    public const string AgencyManager = "Agency Manager";
    public const string Admin = "Admin";

    public static readonly string[] All = { Senior, FamilyMember, Caregiver, AgencyManager, Admin };
}
