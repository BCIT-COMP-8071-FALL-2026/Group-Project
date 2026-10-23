namespace OLTPApp.Authorization;

// One policy per role, registered in Program.cs; use as [Authorize(Policy = Policies.Caregiver)]
public static class Policies
{
    public const string Senior = "RequireSenior";
    public const string FamilyMember = "RequireFamilyMember";
    public const string Caregiver = "RequireCaregiver";
    public const string AgencyManager = "RequireAgencyManager";
    public const string Admin = "RequireAdmin";
}
