namespace Application.Shipments.Reports;

public class AcctgTransEntrySums
{
    public string GlAccountId { get; set; }
    public string GlAccountTypeId { get; set; }
    public string GlAccountClassId { get; set; }
    public string AccountName { get; set; }
    public string AccountCode { get; set; }
    public string GlFiscalTypeId { get; set; }
    public string AcctgTransTypeId { get; set; }
    public string DebitCreditFlag { get; set; }
    public decimal Amount { get; set; }
    public string OrganizationPartyId { get; set; }
    public string IsPosted { get; set; }
    public DateTime TransactionDate { get; set; }
}