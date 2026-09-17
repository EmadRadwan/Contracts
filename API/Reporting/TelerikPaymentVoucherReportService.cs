using System.Collections;
using Application.Interfaces;
using Application.Reports;
using Telerik.Reporting;
using Microsoft.Extensions.Configuration;
using Telerik.Reporting.Processing;

namespace API.Reporting;

/// <summary>
/// Telerik Reporting implementation of the payment voucher. Builds the report in code
/// (<see cref="PaymentVoucherReport"/>) and renders it with <see cref="ReportProcessor"/> —
/// no Reporting REST service or viewer involved. This is the Tier 1 ("just give me the
/// document") path from docs/telerik-reporting-integration-plan.md §8.
/// </summary>
public sealed class TelerikPaymentVoucherReportService : IPaymentVoucherReportService
{
    // Passing IConfiguration is what makes the engine read "telerikReporting:privateFonts"
    // from appsettings.json; the parameterless ReportProcessor ignores app configuration.
    private readonly IConfiguration _configuration;

    public TelerikPaymentVoucherReportService(IConfiguration configuration) => _configuration = configuration;

    private static readonly HashSet<string> SupportedFormats =
        new(StringComparer.OrdinalIgnoreCase) { "PDF", "XLSX", "DOCX", "CSV", "IMAGE", "PPTX" };

    public byte[] Render(PaymentReportDto data, string format = "PDF", string companyName = "Golden Land")
    {
        ArgumentNullException.ThrowIfNull(data);

        var normalizedFormat = string.IsNullOrWhiteSpace(format) ? "PDF" : format.Trim().ToUpperInvariant();
        if (!SupportedFormats.Contains(normalizedFormat))
            throw new ArgumentException($"Unsupported report format '{format}'.", nameof(format));

        using var report = new PaymentVoucherReport(data, companyName);
        var reportSource = new InstanceReportSource { ReportDocument = report };

        var processor = new ReportProcessor(_configuration);
        var result = processor.RenderReport(normalizedFormat, reportSource, new Hashtable());

        if (result.HasErrors)
            throw new System.InvalidOperationException(
                "Telerik Reporting failed to render the payment voucher: " +
                string.Join("; ", result.Errors.Select(e => e.Message)));

        return result.DocumentBytes;
    }
}
