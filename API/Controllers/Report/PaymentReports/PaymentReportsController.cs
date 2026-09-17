using Application.Reports;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Report.PaymentReports;

public class PaymentReportsController : BaseApiController
{
    private readonly IPaymentVoucherReportService _voucherReportService;

    public PaymentReportsController(IPaymentVoucherReportService voucherReportService)
    {
        _voucherReportService = voucherReportService;
    }

    /// <summary>
    /// Payment voucher rendered through Telerik Reporting (QuestPDF, the previous engine behind
    /// this route, was removed 2026-09-17 — the "payment-report-v2" route it coexisted with is
    /// folded back into this one). <paramref name="format"/>: PDF (default), XLSX, DOCX, CSV, IMAGE.
    /// </summary>
    [HttpGet("payment-report/{paymentId}")]
    public async Task<IActionResult> GetPaymentReportPdf(string paymentId, [FromQuery] string format = "PDF")
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
