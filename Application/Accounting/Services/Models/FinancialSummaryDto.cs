namespace Application.Accounting.Services.Models;

public class FinancialSummaryDto
{
    public decimal TotalSalesInvoice { get; set; }
    public decimal TotalPurchaseInvoice { get; set; }
    public decimal TotalPaymentsIn { get; set; }
    public decimal TotalPaymentsOut { get; set; }
    public decimal TotalInvoiceNotApplied { get; set; }
    public decimal TotalPaymentNotApplied { get; set; }
    public decimal TotalToBePaid { get; set; }
    public decimal TotalToBeReceived { get; set; }
}