namespace Application.CRM.Leads.Assignment;

/// <summary>
/// Shared identifiers for lead ownership. LEAD_OWNER already exists as a seeded
/// PartyRelationshipType - no new reference data is required.
/// </summary>
public static class LeadAssignmentConstants
{
    public const string RelationshipTypeId = "LEAD_OWNER";
    public const string OwnerRoleTypeId = "SALES_REP";
    public const string LeadRoleTypeId = "LEAD";

    /// <summary>
    /// Security role permitting a user to assign, reassign and unassign leads.
    /// This is the CRM Admin's defining permission.
    /// </summary>
    public const string AssignSecurityRole = "CRM_Leads_Assign";

    /// <summary>
    /// Security role permitting a user to see every lead. Without it a user
    /// sees only the leads currently assigned to them.
    /// </summary>
    public const string ViewAllSecurityRole = "CRM_Leads_ViewAll";
}
