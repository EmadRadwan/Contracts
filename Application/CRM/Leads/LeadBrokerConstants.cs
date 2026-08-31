namespace Application.CRM.Leads;

/// <summary>
/// Shared identifiers for the broker behind an indirect lead.
///
/// AGENT is reused rather than introducing a LEAD_BROKER relationship type,
/// because PartyRelationshipTypes are seeded only when the table is empty
/// (SeedContracts) - a new type would never appear in an existing database.
/// The role types at both ends (BROKER -> LEAD) are what identify the
/// relationship, exactly as LEAD_OWNER does for assignment.
/// </summary>
public static class LeadBrokerConstants
{
    public const string RelationshipTypeId = "AGENT";
    public const string BrokerRoleTypeId = "BROKER";
    public const string LeadRoleTypeId = "LEAD";

    /// <summary>
    /// The one lead source that requires a broker: the lead reached us through
    /// an outside company rather than our own marketing.
    /// </summary>
    public const string IndirectDataSourceId = "INDIRECT";

    public static bool RequiresBroker(string? dataSourceId) =>
        string.Equals(dataSourceId?.Trim(), IndirectDataSourceId, StringComparison.OrdinalIgnoreCase);
}
