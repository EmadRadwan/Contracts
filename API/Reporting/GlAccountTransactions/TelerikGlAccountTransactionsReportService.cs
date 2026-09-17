using System.Collections;
using Application.Accounting.Services.Models;
using Application.Interfaces;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

namespace API.Reporting.GlAccountTransactions;

/// <summary>
/// Telerik Reporting implementation of the GL account transactions export. Single report (one
/// DetailSection), rendered exactly like the payment voucher — no ReportBook needed here.
/// </summary>
public sealed class TelerikGlAccountTransactionsReportService : IGlAccountTransactionsReportService
{
    private static readonly HashSet<string> SupportedFormats =
        new(StringComparer.OrdinalIgnoreCase) { "PDF", "XLSX", "DOCX", "IMAGE" };

    public byte[] Render(GlAccountTransactionDetails data, string format = "PDF")
    {
        ArgumentNullException.ThrowIfNull(data);

        var normalizedFormat = string.IsNullOrWhiteSpace(format) ? "PDF" : format.Trim().ToUpperInvariant();
        if (!SupportedFormats.Contains(normalizedFormat))
            throw new ArgumentException($"Unsupported report format '{format}'.", nameof(format));

        using var report = new GlAccountTransactionsReport(data);
        var reportSource = new InstanceReportSource { ReportDocument = report };

        var processor = new ReportProcessor();
        var result = processor.RenderReport(normalizedFormat, reportSource, new Hashtable());

        if (result.HasErrors)
            throw new System.InvalidOperationException(
                "Telerik Reporting failed to render the GL account transactions report: " +
                string.Join("; ", result.Errors.Select(e => e.Message)));

        return result.DocumentBytes;
    }
}
