using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.OrganizationGlSettings;

public class GlAccountTypeDefaultRecord
{
    public string GlAccountId { get; set; }

    [Key] public string OrganizationPartyId { get; set; }

    [Key] public string GlAccountTypeId { get; set; }

    public string AccountName { get; set; }
    public string GlAccountTypeDescription { get; set; }
}