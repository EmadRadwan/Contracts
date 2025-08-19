namespace Domain;

public class OrderView
{
    public string OrderId { get; set; }
    public string OrderTypeId { get; set; }
    public string OrderTypeDescription { get; set; }
    public string? OrderTypeDescriptionArabic { get; set; }  // Arabic description for Order Type
    public string? OrderTypeDescriptionTurkish { get; set; } // Turkish description for Order Type
    public string? PaymentMethodId { get; set; }
    public string? PaymentId { get; set; }
    public string? InvoiceId { get; set; }
    public string? PaymentMethodTypeId { get; set; }
    public string? AgreementId { get; set; }
    public string? FromPartyId { get; set; }
    public string? FromPartyNameDescription { get; set; }
    public string? FromPartyName { get; set; }  // The party name
    public string? FromPartyContactNumber { get; set; }  // Separate contact number column
    public DateTime OrderDate { get; set; }
    public string OrderStatus { get; set; }
    public string StatusDescription { get; set; }
    public string? StatusDescriptionArabic { get; set; }  // Arabic description for Status
    public string? StatusDescriptionTurkish { get; set; } // Turkish description for Status
    public string CurrencyUomId { get; set; }
    public string CurrencyUomDescription { get; set; }
    public string? CurrencyUomDescriptionArabic { get; set; }  // Arabic description for Currency UOM
    public string? CurrencyUomDescriptionTurkish { get; set; } // Turkish description for Currency UOM
    public decimal GrandTotal { get; set; }
    public string? BillingAccountId { get; set; }
}
