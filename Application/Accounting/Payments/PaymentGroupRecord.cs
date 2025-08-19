using System.ComponentModel.DataAnnotations;
using Application.Order.Orders;

namespace Application.Accounting.Payments;

public class PaymentGroupRecord
{
    [Key] public string PaymentGroupId { get; set; }
    public string PaymentGroupTypeId { get; set; }
    public string PaymentGroupTypeDescription { get; set; }
    public string PaymentGroupName { get; set; }
    public string FinAccountName { get; set; }
    public string OwnerPartyId { get; set; }
    public bool CanGenerateDepositSlip { get; set; }
    public bool CanPrintCheck { get; set; }
    public bool CanCancel { get; set; }
}