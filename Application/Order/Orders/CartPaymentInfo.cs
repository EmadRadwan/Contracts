namespace Application.Order.Orders
{
    public class CartPaymentInfo
    {
        // Unique identifier for the payment method (optional)
        public string? PaymentMethodId { get; set; }
        public string? PaymentMethodTypeId { get; set; }

        // The amount for this payment (optional)
        public decimal? Amount { get; set; }
        

        // Reference number for the payment (if applicable) (optional)
        public string? PaymentReferenceNumber { get; set; }

        // Any additional comments related to the payment (optional)
        public string? Comments { get; set; }

        // Financial account identifier if applicable (optional)
        public string? FinAccountId { get; set; }

        // Billing postal code (optional)
        public string? BillingPostalCode { get; set; }

        // Flag indicating if the payment is present (optional)
        public string? IsPresent { get; set; } 

        // Flag indicating if the payment is swiped (optional)
        public string? IsSwiped { get; set; } 

        // Flag indicating if the payment is an overflow (optional)
        public string? Overflow { get; set; } 

        // Manual reference numbers and authorization codes, if any (optional)
        public string[]? RefNum { get; set; }

        // Security code related to the payment (optional)
        public string? SecurityCode { get; set; }

        // Track 2 data, if applicable (optional)
        public string? Track2 { get; set; }
    }
}
