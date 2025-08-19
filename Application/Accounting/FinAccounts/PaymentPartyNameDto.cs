namespace Application.Accounting.FinAccounts;

public class PaymentPartyNameDto
{
    // Payment fields
    public string PaymentId { get; set; }
    public string PartyIdFrom { get; set; }
    public string PartyIdTo { get; set; }
    public string PaymentMethodTypeId { get; set; }
    public string PaymentTypeId { get; set; }
    public string StatusId { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string FinAccountTransId { get; set; }

    // PaymentMethodType
    public string PaymentMethodTypeDesc { get; set; }

    // PaymentType
    public string PaymentTypeDesc { get; set; }
    public string ParentPaymentTypeId { get; set; }

    // StatusItem
    public string StatusDesc { get; set; }

    // Party From name
    public string PartyFromFirstName { get; set; }
    public string PartyFromLastName { get; set; }
    public string PartyFromGroupName { get; set; }

    // Party To name
    public string PartyToFirstName { get; set; }
    public string PartyToLastName { get; set; }
    public string PartyToGroupName { get; set; }
}