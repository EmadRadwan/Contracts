namespace Application.Accounting.Payments;


public class PaymentDto2
{
    public string PaymentId { get; set; } = null!;
    public string? PaymentTypeId { get; set; }
    public string? PaymentTypeDescription { get; set; }
    public PaymentPartyDto? PartyIdFrom { get; set; }
    public string? FromPartyName { get; set; }
    public PaymentPartyDto? PartyId { get; set; }
    public string? ToPartyName { get; set; }
    public string? RoleTypeId { get; set; }
    public string? StatusId { get; set; }
    public string? StatusDescription { get; set; }

    public string? BillingAccountId { get; set; }
    public string? ContactMechId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? PaymentMessage { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? CurrencyUomName { get; set; }


    public decimal? Total { get; set; }
    public decimal? OutstandingAmount { get; set; }


    public bool AllowSubmit { get; set; }
    public ICollection<PaymentApplicationParam>? PaymentApplications { get; set; }
}