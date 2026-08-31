namespace Application.CRM.Leads;

/// <summary>
/// DTO representing a Lead (Person) in the CRM.
///
/// KEY CONCEPT:
/// A Lead is a PERSON (who you know).
/// Leads are linked to Sales Opportunities via SalesOpportunityRole.
/// </summary>
public class LeadDto
{
    public string? PartyId { get; set; }

    // Identity
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Display name (computed)
    public string? FullName { get; set; }

    // Communication
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }

    // Address
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryGeoId { get; set; }
    public string? CountryName { get; set; }

    // CRM metadata
    public string? DataSourceId { get; set; }  // How they came to us
    public List<string> Tags { get; set; } = new();
    public string? MarketingStatus { get; set; }  // subscribed, unsubscribed, unknown

    // Status
    public string? StatusId { get; set; }
    public string? StatusDescription { get; set; }

    // Audit
    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastContactedTime { get; set; }

    // Organization (if lead belongs to a company)
    public string? OrganizationPartyId { get; set; }
    public string? OrganizationName { get; set; }
    public string? LeadTemperatureId { get; set; }

    // Assignment - current LEAD_OWNER relationship (null when unassigned)
    public string? OwnerPartyId { get; set; }
    public string? OwnerName { get; set; }
    public DateTime? AssignedDate { get; set; }

    // Broker behind an INDIRECT lead - the open AGENT relationship, if any.
    public string? BrokerPartyId { get; set; }
    public string? BrokerName { get; set; }

    // Duplicate feedback. When true the payload describes the EXISTING lead that
    // already holds the contact details, not the lead that was being saved -
    // same contract as PartyDto2 on the create side, so one UI handles both.
    public bool? IsAlreadyCreated { get; set; }
    public string? DuplicateMatchedField { get; set; }   // "EMAIL" or "MOBILE"
    public string? DuplicateMatchedValue { get; set; }
}

/// <summary>
/// Lightweight DTO for dropdowns/pickers.
/// </summary>
public class LeadLovDto
{
    public string PartyId { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
