namespace Application.Accounting.Payments;

public class PaymentGroupMemberPaymentAndFinAccountTrans
{
    public string PaymentGroupId { get; set; }
    public string PaymentId { get; set; }
    public string FinAccountTransId { get; set; }
    public string FinAccountId { get; set; }
    public string PartyId { get; set; }
    public string FinAccountTransStatusId { get; set; }
    public decimal? FinAccountTransAmount { get; set; }
    public string GlReconciliationId { get; set; }
}