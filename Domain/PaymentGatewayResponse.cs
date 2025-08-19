namespace Domain;

public class PaymentGatewayResponse
{
    public PaymentGatewayResponse()
    {
        PaymentGatewayRespMsgs = new HashSet<PaymentGatewayRespMsg>();
        Payments = new HashSet<Payment>();
    }

    public string PaymentGatewayResponseId { get; set; } = null!;
    public string? PaymentServiceTypeEnumId { get; set; }
    public string? OrderPaymentPreferenceId { get; set; }
    public string? PaymentMethodTypeId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? TransCodeEnumId { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? ReferenceNum { get; set; }
    public string? AltReference { get; set; }
    public string? SubReference { get; set; }
    public string? GatewayCode { get; set; }
    public string? GatewayFlag { get; set; }
    public string? GatewayAvsResult { get; set; }
    public string? GatewayCvResult { get; set; }
    public string? GatewayScoreResult { get; set; }
    public string? GatewayMessage { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? ResultDeclined { get; set; }
    public string? ResultNsf { get; set; }
    public string? ResultBadExpire { get; set; }
    public string? ResultBadCardNumber { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? CurrencyUom { get; set; }
    public OrderPaymentPreference? OrderPaymentPreference { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public PaymentMethodType? PaymentMethodType { get; set; }
    public Enumeration? PaymentServiceTypeEnum { get; set; }
    public Enumeration? TransCodeEnum { get; set; }
    public ICollection<PaymentGatewayRespMsg> PaymentGatewayRespMsgs { get; set; }
    public ICollection<Payment> Payments { get; set; }
}