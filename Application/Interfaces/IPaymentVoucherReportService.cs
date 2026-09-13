using Application.Reports;

namespace Application.Interfaces;

/// <summary>
/// Renders the payment voucher (بيان دفعة) through Telerik Reporting.
/// Parallel to <see cref="IPdfGenerationService"/> (QuestPDF) — both are driven by the same
/// <see cref="GetPaymentForReport"/> query so the two engines can be compared side by side.
/// </summary>
public interface IPaymentVoucherReportService
{
    /// <param name="data">Voucher data from <c>GetPaymentForReport.Query</c>.</param>
    /// <param name="format">Telerik render extension: <c>PDF</c> (default), <c>XLSX</c>, <c>DOCX</c>, <c>CSV</c>, <c>IMAGE</c>.</param>
    /// <param name="companyName">Header company name; defaults to Golden Land.</param>
    byte[] Render(PaymentReportDto data, string format = "PDF", string companyName = "Golden Land");
}
