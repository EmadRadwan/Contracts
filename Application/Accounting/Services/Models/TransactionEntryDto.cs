namespace Application.Accounting.Services.Models;

public class TransactionEntryDto
{
    public string AcctgTransId { get; set; }
    public string AcctgTransEntrySeqId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string AcctgTransTypeId { get; set; }
    public string GlFiscalTypeId { get; set; }
    public string InvoiceId { get; set; }
    public string PaymentId { get; set; }
    public string WorkEffortId { get; set; }
    public string ShipmentId { get; set; }
    public string PartyId { get; set; }
    public string PartyName { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string IsPosted { get; set; }
    public string Description { get; set; }
    public DateTime? PostedDate { get; set; }
    public string DebitCreditFlag { get; set; }
    public string CurrencyUomId { get; set; }
    public decimal Amount { get; set; }
}