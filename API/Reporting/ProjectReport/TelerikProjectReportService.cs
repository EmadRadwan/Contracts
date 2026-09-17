using System.Collections;
using Application.Interfaces;
using Application.Projects;
using Telerik.Reporting;
using Microsoft.Extensions.Configuration;
using Telerik.Reporting.Processing;
using TelerikReport = Telerik.Reporting.Report;

namespace API.Reporting.ProjectReport;

/// <summary>
/// Telerik Reporting implementation of the project report. Renders the summary plus every
/// non-empty section as SEPARATE mini-reports concatenated with a
/// <see cref="Telerik.Reporting.Processing.ReportBook"/> into one document.
///
/// Why a ReportBook and not one Report with several DetailSections: a Report follows the classic
/// banded model with exactly ONE Detail band — an earlier version of this class added a
/// DetailSection per block directly to one Report and only the first one ever rendered (the rest
/// were silently dropped). ReportBook sidesteps that entirely: each block is its own tiny report
/// with exactly one DetailSection — the shape already proven to work for the payment voucher.
/// </summary>
public sealed class TelerikProjectReportService : IProjectReportService
{
    // Passing IConfiguration is what makes the engine read "telerikReporting:privateFonts"
    // from appsettings.json; the parameterless ReportProcessor ignores app configuration.
    private readonly IConfiguration _configuration;

    public TelerikProjectReportService(IConfiguration configuration) => _configuration = configuration;

    private static readonly HashSet<string> SupportedFormats =
        new(StringComparer.OrdinalIgnoreCase) { "PDF", "XLSX", "DOCX", "IMAGE" };

    public byte[] Render(ProjectReportDto data, string projectName, string projectId, string period, string format = "PDF")
    {
        ArgumentNullException.ThrowIfNull(data);

        var normalizedFormat = string.IsNullOrWhiteSpace(format) ? "PDF" : format.Trim().ToUpperInvariant();
        if (!SupportedFormats.Contains(normalizedFormat))
            throw new ArgumentException($"Unsupported report format '{format}'.", nameof(format));

        var reports = new List<TelerikReport>
        {
            new ProjectReportSummaryReport(data.Summary, projectName, projectId, period)
        };

        try
        {
            foreach (var section in ProjectReportSections.BuildAll(data))
                reports.Add(new ProjectReportTableReport(section.Title, section.Columns, section.Rows, projectName, projectId));

            using var book = new ReportBook();
            foreach (var r in reports)
                book.ReportSources.Add(new InstanceReportSource { ReportDocument = r });

            var processor = new ReportProcessor(_configuration);
            var result = processor.RenderReport(normalizedFormat, book, new Hashtable());

            if (result.HasErrors)
                throw new System.InvalidOperationException(
                    "Telerik Reporting failed to render the project report: " +
                    string.Join("; ", result.Errors.Select(e => e.Message)));

            return result.DocumentBytes;
        }
        finally
        {
            foreach (var r in reports) r.Dispose();
        }
    }
}
