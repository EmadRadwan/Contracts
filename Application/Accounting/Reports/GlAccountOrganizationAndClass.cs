namespace Application.Shipments.Reports;

public class GlAccountOrganizationAndClass
{
    public string GlAccountId { get; set; }
    public string OrganizationPartyId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
}