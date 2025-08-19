using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.Invoices;

public class InvoiceRecord
{
    [Key] public string InvoiceId { get; set; } = null!;

    public string? InvoiceTypeId { get; set; }
    public string? InvoiceTypeDescription { get; set; }
    public InvoicePartyDto? PartyIdFrom { get; set; }
    public string FromPartyName { get; set; }
    public InvoicePartyDto? PartyId { get; set; }
    public string ToPartyName { get; set; }
    public string? RoleTypeId { get; set; }
    public string? StatusId { get; set; }


    public string StatusDescription { get; set; }

    public string? BillingAccountId { get; set; }
    public string? BillingAccountName { get; set; }
    public string? ContactMechId { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? InvoiceMessage { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? CurrencyUomName { get; set; }


    public decimal? Total { get; set; }
    public decimal? OutstandingAmount { get; set; }


    public bool AllowSubmit { get; set; }
}