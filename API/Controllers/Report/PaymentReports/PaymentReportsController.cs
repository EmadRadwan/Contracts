using Application.Reports;
using Application.Interfaces; // This is where IPdfGenerationService lives
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Report.PaymentReports;

public class PaymentReportsController : BaseApiController
{
    private readonly IPdfGenerationService _pdfService;

    public PaymentReportsController(IPdfGenerationService pdfService)
    {
        _pdfService = pdfService;
    }

    [HttpGet("payment-report/{paymentId}")]
    public async Task<IActionResult> GetPaymentReportPdf(string paymentId)
    {
        // Fetch data via Application layer (MediatR)
        var reportData = await Mediator.Send(new GetPaymentForReport.Query(paymentId, "ar"));

        // Generate PDF using Infrastructure service
        var pdfBytes = _pdfService.GeneratePaymentReportPdf(reportData);

        var fileName = $"{reportData.PaymentId}_بيان_دفعة.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}