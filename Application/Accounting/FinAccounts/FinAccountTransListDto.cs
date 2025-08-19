namespace Application.Accounting.FinAccounts;

public class FinAccountTransListDto
{
    public string FinAccountTransId { get; set; } = null!;
    public string? FinAccountTransTypeId { get; set; }
    public string? FinAccountTransTypeDescription { get; set; }
    public string? FinAccountId { get; set; }
    public string? PartyId { get; set; }
    public string? GlReconciliationId { get; set; }
    public DateTime? TransactionDate { get; set; }
    public DateTime? EntryDate { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentId { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? PerformedByPartyId { get; set; }
    public string? ReasonEnumId { get; set; }
    public string? Comments { get; set; }
    public string? StatusId { get; set; }
    public string? StatusDescription { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}