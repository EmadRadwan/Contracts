using System.ComponentModel.DataAnnotations;
using Application.Order.Orders;

namespace Application.Accounting.Payments;


public class PaymentRecord
{
    [Key] public string PaymentId { get; set; }
    public string PaymentTypeId { get; set; }
    public string PaymentTypeDescription { get; set; }
    public string PaymentMethodId { get; set; }
    public string PaymentMethodTypeId { get; set; }
    public string PaymentMethodTypeDescription { get; set; }
    public string PartyIdFrom { get; set; }
    public string PartyIdFromName { get; set; }
    public string PartyIdTo { get; set; }
    public string PartyIdToName { get; set; }
    public string StatusId { get; set; }
    public string StatusDescription { get; set; }
    public string StatusDescriptionEnglish { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string Comments { get; set; }
    public string PaymentRefNum { get; set; }
    public string PaymentPreferenceId { get; set; }
    public decimal? ActualCurrencyAmount { get; set; }
    public string OverrideGlAccountId { get; set; }

    public decimal Amount { get; set; }
    public decimal AmountToApply { get; set; }
    public string CurrencyUomId { get; set; }
    public string OrganizationPartyId { get; set; }
    public string FinAccountTransId { get; set; }
    public string CreditCardNumber { get; set; }
    public string? CreditCardExpiryDate { get; set; }
    public bool IsDisbursement { get; set; }
    public OrderPartyDto FromPartyId { get; set; }
}