namespace Application.Shipments.Accounting;

public class GlAccountOrganizationAndClassDto
{
    public string GlAccountId { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
    public string? GlAccountTypeId { get; set; }
    public string? GlAccountTypeName { get; set; }

    public string? GlAccountClassId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? ClassName { get; set; }
}