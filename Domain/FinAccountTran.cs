using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]


public class FinAccountTran
{
    public FinAccountTran()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        FinAccountTransAttributes = new HashSet<FinAccountTransAttribute>();
        Payments = new HashSet<Payment>();
        ReturnItemResponses = new HashSet<ReturnItemResponse>();
    }

    public string FinAccountTransId { get; set; } = null!;
    public string? FinAccountTransTypeId { get; set; }
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
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FinAccount? FinAccount { get; set; }
    public FinAccountTransType? FinAccountTransType { get; set; }
    public GlReconciliation? GlReconciliation { get; set; }
    public OrderItem? OrderI { get; set; }
    public Party? Party { get; set; }
    public Payment? Payment { get; set; }
    public Party? PerformedByParty { get; set; }
    public Enumeration? ReasonEnum { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<FinAccountTransAttribute> FinAccountTransAttributes { get; set; }
    public ICollection<Payment> Payments { get; set; }
    public ICollection<ReturnItemResponse> ReturnItemResponses { get; set; }
}