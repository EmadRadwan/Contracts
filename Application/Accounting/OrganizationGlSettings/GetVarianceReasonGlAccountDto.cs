namespace Application.Shipments.OrganizationGlSettings;

public class GetVarianceReasonGlAccountDto
{
    public string VarianceReasonId { get; set; } = null!;
    public string VarianceReasonDescription { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
    public string? GlAccountId { get; set; }
    public string? GlAccountName { get; set; }
}