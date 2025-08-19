namespace Application.Shipments.Reports;

public class TransactionTotalsViewModel
{
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string GlFiscalTypeId { get; set; }
    public string OrganizationPartyId { get; set; }
    public List<TransactionTotal> PostedTransactionTotals { get; set; }
    public List<TransactionTotal> UnpostedTransactionTotals { get; set; }
    public List<TransactionTotal> AllTransactionTotals { get; set; }
    public List<MonthModel> MonthList { get; set; }
}