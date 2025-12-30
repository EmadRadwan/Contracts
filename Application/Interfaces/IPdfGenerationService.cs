using Application.Reports;

namespace Application.Interfaces;

public interface IPdfGenerationService
{
    byte[] GeneratePaymentReportPdf(PaymentReportDto data, string companyName = "Golden Land");
}