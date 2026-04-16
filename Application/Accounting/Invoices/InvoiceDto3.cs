namespace Application.Shipments.Invoices;

public class InvoiceDto3
{
    public string? InvoiceId { get; set; } = null!;
    public string? InvoiceTypeId { get; set; }
    public string? InvoiceTypeDescription { get; set; }
    public string? PartyIdFrom { get; set; }
    public string? FromPartyName { get; set; }
    public string? PartyId { get; set; }
    public string? ToPartyName { get; set; }
    public string? RoleTypeId { get; set; }
    public string? StatusId { get; set; }
    public string? StatusDescription { get; set; }

    public string? BillingAccountId { get; set; }
    public string? ContactMechId { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? PaidDate { get; set; }
    public string? InvoiceMessage { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? CurrencyUomName { get; set; }


    public decimal? Total { get; set; }
    public decimal? OutstandingAmount { get; set; }


    public List<InvoiceItemDto2>? InvoiceItems { get; set; } = new();
}