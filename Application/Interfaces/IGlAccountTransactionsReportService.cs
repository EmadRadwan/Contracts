using Application.Accounting.Services.Models;

namespace Application.Interfaces;

/// <summary>
/// Renders the GL account transaction details (the drill-down modal behind both trial balance
/// reports — classic and by-level) through Telerik Reporting. Same server-side Skia pipeline as
/// the payment voucher and the project report, so Arabic shaping/bidi is correct in the PDF.
/// </summary>
public interface IGlAccountTransactionsReportService
{
    /// <param name="data">The result of GetGlAccountTransactionDetails (account header, balances, period, running-balance rows).</param>
    /// <param name="format">Telerik render extension: <c>PDF</c> (default).</param>
    byte[] Render(GlAccountTransactionDetails data, string format = "PDF");
}
