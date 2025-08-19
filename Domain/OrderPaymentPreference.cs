using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class OrderPaymentPreference
{
    public OrderPaymentPreference()
    {
        PaymentGatewayResponses = new HashSet<PaymentGatewayResponse>();
        Payments = new HashSet<Payment>();
        ReturnItemResponses = new HashSet<ReturnItemResponse>();
    }

    public string OrderPaymentPreferenceId { get; set; } = null!;
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ShipGroupSeqId { get; set; }
    public string? ProductPricePurposeId { get; set; }
    public string? PaymentMethodTypeId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? FinAccountId { get; set; }
    public string? SecurityCode { get; set; }
    public string? Track2 { get; set; }
    public string? PresentFlag { get; set; }
    public string? SwipedFlag { get; set; }
    public string? OverflowFlag { get; set; }
    public decimal MaxAmount { get; set; }
    public int? ProcessAttempt { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? ManualAuthCode { get; set; }
    public string? ManualRefNum { get; set; }
    public string? StatusId { get; set; }
    public string? NeedsNsfRetry { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public FinAccount? FinAccount { get; set; }
    public OrderHeader? Order { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public PaymentMethodType? PaymentMethodType { get; set; }
    public ProductPricePurpose? ProductPricePurpose { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<PaymentGatewayResponse> PaymentGatewayResponses { get; set; }
    public ICollection<Payment> Payments { get; set; }
    public ICollection<ReturnItemResponse> ReturnItemResponses { get; set; }
}