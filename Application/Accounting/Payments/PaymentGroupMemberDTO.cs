namespace Application.Accounting.Payments;

public class PaymentGroupMemberDTO
{
    public string PaymentGroupId { get; set; }
    public string PaymentId { get; set; }
    public string PaymentRefNum { get; set; }
    public string PartyIdFrom { get; set; }
    public string PartyIdTo { get; set; }
    public string PaymentTypeId { get; set; }
    public string PaymentMethodTypeId { get; set; }
    public string CardType { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyUomId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
}
