namespace Domain;

public class GiftCardFulfillment
{
    public string FulfillmentId { get; set; } = null!;
    public string? TypeEnumId { get; set; }
    public string? MerchantId { get; set; }
    public string? PartyId { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? SurveyResponseId { get; set; }
    public string? CardNumber { get; set; }
    public string? PinNumber { get; set; }
    public decimal? Amount { get; set; }
    public string? ResponseCode { get; set; }
    public string? ReferenceNum { get; set; }
    public string? AuthCode { get; set; }
    public DateTime? FulfillmentDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader? Order { get; set; }
    public OrderItem? OrderI { get; set; }
    public Party? Party { get; set; }
    public SurveyResponse? SurveyResponse { get; set; }
    public Enumeration? TypeEnum { get; set; }
}