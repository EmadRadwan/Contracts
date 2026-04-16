using System;

namespace Application.Accounting.Invoices
{
    public class InvoiceDto4
    {
        public string InvoiceId { get; set; }
        public string InvoiceTypeDescription { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public string StatusId { get; set; }
        public string InvoiceTypeId { get; set; }
        public string Status { get; set; }
        public string StatusDescription { get; set; }
        public string Description { get; set; }
        public string ReferenceNumber { get; set; }
        public DateOnly? DueDate { get; set; }
        public DateOnly? PaidDate { get; set; }
        public InvoicePartyDto4 PartyId { get; set; }
        public string ToPartyName { get; set; }
        public InvoicePartyDto4 PartyIdFrom { get; set; }
        public string FromPartyName { get; set; }
        public string BillingAccountId { get; set; }
        public string BillingAccountName { get; set; }
        public decimal Total { get; set; }
        public decimal OutstandingAmount { get; set; }
    }

    public class InvoicePartyDto4
    {
        public string FromPartyId { get; set; }
        public string FromPartyName { get; set; }
    }
}