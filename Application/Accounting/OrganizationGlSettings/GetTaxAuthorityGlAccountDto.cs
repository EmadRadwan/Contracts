namespace Application.Shipments.OrganizationGlSettings;

public class GetTaxAuthorityGlAccountDto
{
    public string TaxAuthGeoId { get; set; }
    public string TaxAuthPartyId { get; set; }
    public string TaxAuthPartyName { get; set; }
    public string GlAccountId { get; set; }
    public string GlAccountName { get; set; }
}