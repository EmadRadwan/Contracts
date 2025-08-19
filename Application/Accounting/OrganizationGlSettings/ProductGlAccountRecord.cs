using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.OrganizationGlSettings;

public class ProductGlAccountRecord
{
    [Key] public string ProductId { get; set; }

    [Key] public string OrganizationPartyId { get; set; }

    [Key] public string GlAccountTypeId { get; set; }

    public string GlAccountId { get; set; }

    public string AccountName { get; set; }
    public string ProductName { get; set; }
    public string GlAccountTypeDescription { get; set; }
}