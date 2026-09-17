using Application.Reports;

namespace Application.Interfaces;

/// <summary>
/// Renders the payment voucher (بيان دفعة) through Telerik Reporting, driven by the
/// <see cref="GetPaymentForReport"/> query. The only PDF engine since QuestPDF was removed (2026-09-17).
/// </summary>
public interface IPaymentVoucherReportService
{
    /// <param name="data">Voucher data from <c>GetPaymentForReport.Query</c>.</param>
    /// <param name="format">Telerik render extension: <c>PDF</c> (default), <c>XLSX</c>, <c>DOCX</c>, <c>CSV</c>, <c>IMAGE</c>.</param>
    /// <param name="companyName">Header company name; defaults to Golden Land.</param>
    byte[] Render(PaymentReportDto data, string format = "PDF", string companyName = "Golden Land");
}
