using System.Collections;
using System.Reflection;
using Telerik.Reporting;
using Telerik.Reporting.Drawing;
using static API.Reporting.ProjectReport.ProjectReportLayout;

namespace API.Reporting.ProjectReport;

/// <summary>
/// One mini-report per project-report section (المستخلصات, الدفعات المباشرة, …), appended to the
/// ReportBook after <see cref="ProjectReportSummaryReport"/>. Title + a right-to-left header row +
/// one literal TextBox per cell, built by looping over the rows in C# — deliberately NOT using
/// Telerik.Reporting.List / "= Fields.X" data binding: that produced zero rows in practice (every
/// section rendered its header and nothing else) and two rounds were already spent guessing at the
/// exact binding API. This reuses the literal-value technique already proven twice (the payment
/// voucher, ProjectReportSummaryReport) — Telerik only ever lays out content this class has
/// already fully computed, one plain TextBox at a time; a tall DetailSection paginates onto
/// further pages automatically, same as any report engine.
/// </summary>
public sealed class ProjectReportTableReport : Report
{
    public ProjectReportTableReport(
        string title, IReadOnlyList<ProjectReportColumn> columns, IList rows, string projectName, string projectId)
    {
        Name = "ProjectReportTableReport";
        ApplyPageSettings(this);
        Items.Add(BuildRunningHeader(projectName, projectId));
        Items.Add(BuildSection(title, columns, rows));
    }

    private static DetailSection BuildSection(string title, IReadOnlyList<ProjectReportColumn> columns, IList rows)
    {
        var section = new DetailSection { Name = "secTable" };
        section.Items.Add(Text("secTitle", title, 0, 0, PageWidthCm, 0.55, 12, true, HorizontalAlign.Right));

        double totalWeight = columns.Sum(c => c.Weight);
        double scale = totalWeight > 0 ? PageWidthCm / totalWeight : 1;
        var colWidths = columns.Select(c => c.Weight * scale).ToList();

        double y = 0.6;

        // Header row — positioned right-to-left (first column in `columns` sits at the right edge).
        double cursor = 0;
        for (var i = 0; i < columns.Count; i++)
        {
            var w = colWidths[i];
            var x = PageWidthCm - cursor - w;
            var hb = Text($"h{i}", columns[i].Header, x, y, w, RowHeightCm, 9, true, HorizontalAlign.Center,
                System.Drawing.Color.White);
            hb.Style.BackgroundColor = System.Drawing.Color.FromArgb(0x1E, 0x3A, 0x5F);
            section.Items.Add(hb);
            cursor += w;
        }
        y += RowHeightCm;

        var capped = rows.Count > RowCap;
        if (capped)
        {
            section.Items.Add(Text("capNote", $"يعرض أول {RowCap} صف فقط — التصدير الكامل عبر Excel.",
                0, y, PageWidthCm, 0.45, 8, false, HorizontalAlign.Right, System.Drawing.Color.Gray));
            y += 0.45;
        }

        // Resolve each column's property once against the row type (the list is homogeneous —
        // one concrete Pdf*Row type per section, see ProjectReportPdfRows.cs).
        var rowType = rows[0]!.GetType();
        var propsByField = columns.Select(c => rowType.GetProperty(c.Field)).ToList();

        var shown = Math.Min(rows.Count, RowCap);
        for (var r = 0; r < shown; r++)
        {
            var row = rows[r];
            cursor = 0;
            for (var i = 0; i < columns.Count; i++)
            {
                var w = colWidths[i];
                var x = PageWidthCm - cursor - w;
                var raw = propsByField[i]?.GetValue(row);
                var text = FormatCell(raw, columns[i].Format);

                var tb = Text($"r{r}c{i}", text, x, y, w, RowHeightCm, 8, false, HorizontalAlign.Right);
                tb.CanGrow = false;
                tb.Style.BorderStyle.Bottom = BorderType.Solid;
                tb.Style.BorderWidth.Bottom = Unit.Point(0.5);
                tb.Style.BorderColor.Bottom = System.Drawing.Color.LightGray;
                section.Items.Add(tb);
                cursor += w;
            }
            y += RowHeightCm;
        }

        section.Height = Unit.Cm(y + 0.2);
        return section;
    }

    private static string FormatCell(object? value, string? format)
    {
        if (value is null) return "";
        return format != null ? string.Format(format, value) : (value.ToString() ?? "");
    }
}
