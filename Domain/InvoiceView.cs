namespace Domain;

public class InvoiceView
{
    public string InvoiceId { get; set; } = null!;

    public string InvoiceTypeId { get; set; } = null!;

    // Arabic by default (fallback to English if Arabic is NULL)
    public string InvoiceTypeDescription { get; set; } = null!;

    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime? CreatedStamp { get; set; }

    public string StatusId { get; set; } = null!;

    // Arabic by default (fallback to English)
    public string StatusDescription { get; set; } = null!;

    public string? Description { get; set; }

    // === From Party (المورد / Supplier / Vendor) ===
    // The actual ID of the party the invoice is from
    public string? FromPartyId { get; set; }

    // The name/description of the from party - Arabic preferred in DB
    public string? FromPartyName { get; set; }

    // === To Party (الشركة / Company - usually Golden Land) ===
    public string? ToPartyId { get; set; }
    public string? ToPartyName { get; set; }

    // Billing account (optional)
    public string? BillingAccountId { get; set; }
    public string? BillingAccountName { get; set; }

    // Calculated total - with special certificate deduction logic and proper rounding
    public decimal Total { get; set; }

    // Placeholder - can be calculated later if needed
    public decimal OutstandingAmount { get; set; }

    // Linked purchase order (if any)
    public string? OrderId { get; set; }

    // Certificate number from linked project certificate (if invoice is based on one)
    public string? CertificateNumber { get; set; }
}