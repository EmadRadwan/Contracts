namespace Application.Accounting.OrganizationGlSettings;

/// <summary>
/// Represents a single AcctgTrans + AcctgTransEntry record
/// (or a minimal subset) for returning to the caller,
/// mirroring 'acctgTransAndEntry' usage in OFBiz.
/// </summary>
public class AcctgTransAndEntryDto
{
    public string OrganizationPartyId { get; set; }
    public string GlAccountId { get; set; }
    public string DebitCreditFlag { get; set; }
    public decimal Amount { get; set; }

    public string AcctgTransId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string GlFiscalTypeId { get; set; }
    public string IsPosted { get; set; }
}