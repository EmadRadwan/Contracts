using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.OrganizationGlSettings;

public class InternalAccountingOrganizationRecord
{
    [Key] public string PartyId { get; set; }

    public string PartyName { get; set; }
}