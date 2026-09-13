using Application.Projects;

namespace Application.Interfaces;

/// <summary>
/// Renders the project report (summary + all sections) through Telerik Reporting.
/// Replaces the KendoReact Grid PDFExport attempt, which does not shape/reorder Arabic text —
/// this reuses the same Telerik/Skia pipeline already proven correct for the payment voucher.
/// </summary>
public interface IProjectReportService
{
    /// <param name="data">The report DTO from GetProjectReport / GetCompanyReport (Summary must be populated).</param>
    /// <param name="projectName">Header display name (client-supplied, same as the Excel export).</param>
    /// <param name="projectId">Header reference id.</param>
    /// <param name="period">Pre-formatted period label for the header, e.g. "المصاريف: الكل · الإيرادات: 2026-01-01_إلى_2026-09-11".</param>
    /// <param name="format">Telerik render extension: <c>PDF</c> (default).</param>
    byte[] Render(ProjectReportDto data, string projectName, string projectId, string period, string format = "PDF");
}
