namespace Application.CRM.Leads.Assignment;

/// <summary>
/// Represents the current ownership of a Lead.
///
/// KEY CONCEPT:
/// Assignment is modelled as a PartyRelationship of type LEAD_OWNER:
///   PartyIdFrom = the sales rep (owner),  RoleTypeIdFrom = SALES_REP
///   PartyIdTo   = the lead,               RoleTypeIdTo   = LEAD
/// The open row (ThruDate == null) is the current owner; closed rows are history.
/// </summary>
public class LeadAssignmentDto
{
    public string LeadPartyId { get; set; } = null!;
    public string? LeadName { get; set; }

    public string? OwnerPartyId { get; set; }
    public string? OwnerName { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// One entry in a Lead's ownership history - the open row plus every closed one.
///
/// Records both who OWNED the lead over which period, and which user performed
/// the assignment (AssignedByUserLogin - null for rows written before that
/// column existed).
/// </summary>
public class LeadAssignmentHistoryDto
{
    public string? OwnerPartyId { get; set; }
    public string? OwnerName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Comments { get; set; }

    /// <summary>UserLogin that performed the assignment, when known.</summary>
    public string? AssignedByUserLogin { get; set; }

    /// <summary>True for the open row - the lead's current owner.</summary>
    public bool IsCurrent { get; set; }
}
