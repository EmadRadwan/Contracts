using Application.Reports;
using Application.Interfaces; // This is where IPdfGenerationService lives
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Report.PaymentReports;

public class PaymentReportsController : BaseApiController
{
    private readonly IPdfGenerationService _pdfService;
    private readonly IPaymentVoucherReportService _voucherReportService;

    public PaymentReportsController(
        IPdfGenerationService pdfService,
        IPaymentVoucherReportService voucherReportService)
    {
        _pdfService = pdfService;
        _voucherReportService = voucherReportService;
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

    /// <summary>
    /// Same voucher, rendered through Telerik Reporting instead of QuestPDF. Shares the exact
    /// GetPaymentForReport data path so the two engines can be compared side by side.
    /// <paramref name="format"/>: PDF (default), XLSX, DOCX, CSV, IMAGE.
    /// </summary>
    [HttpGet("payment-report-v2/{paymentId}")]
    public async Task<IActionResult> GetPaymentReportV2(string paymentId, [FromQuery] string format = "PDF")
    {
        var reportData = await Mediator.Send(new GetPaymentForReport.Query(paymentId, "ar"));

        var bytes = _voucherReportService.Render(reportData, format);

        var (contentType, ext) = (format ?? "PDF").Trim().ToUpperInvariant() switch
        {
            "XLSX" => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
            "DOCX" => ("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx"),
            "CSV" => ("text/csv", "csv"),
            "IMAGE" => ("image/png", "png"),
            _ => ("application/pdf", "pdf"),
        };

        var fileName = $"{reportData.PaymentId}_بيان_دفعة.{ext}";

        return File(bytes, contentType, fileName);
    }
}
