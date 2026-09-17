using Telerik.Reporting;
using Telerik.Reporting.Drawing;

namespace API.Reporting.ProjectReport;

/// <summary>
/// Shared page setup + text-item helper for every mini-report in the project report's
/// <see cref="Telerik.Reporting.Processing.ReportBook"/> (see TelerikProjectReportService). Kept as
/// static helpers rather than a common base class — simpler than fighting Report's constructor
/// chain for something this small.
/// </summary>
internal static class ProjectReportLayout
{
    public const double PageWidthCm = 26.0; // A4 landscape (29.7cm) minus ~1.85cm margins each side
    // Amiri ships with the app as a Telerik private font (API/Fonts + appsettings.json
    // "telerikReporting:privateFonts"), so this renders the same on macOS dev and in the Docker
    // image. It was "Arial" before: absent in the container, so fontconfig substituted a face
    // with no Arabic glyphs and every Arabic run printed as boxes.
    public const string FontFamily = "Amiri";
    // Bold is a SEPARATE private-font family on purpose. Registering Amiri-Bold.ttf under the
    // same "Amiri" family with fontStyle "Bold" made Telerik shape every regular-weight run with
    // the Bold file's OpenType tables while embedding the Regular file's outlines — glyph IDs
    // differ between the two files, so all joined Arabic came out as garbage (digits and Latin,
    // whose IDs coincide, looked fine). Verified with HarfBuzz against the PDF's glyph stream.
    // One file per family name sidesteps it: pick the face by name, never via Font.Bold.
    public const string BoldFontFamily = "Amiri Bold";

    /// <summary>Applies the report font to a style: family chosen by weight, Bold flag left off.</summary>
    public static void ApplyFont(Telerik.Reporting.Drawing.Style style, bool bold)
    {
        style.Font.Name = bold ? BoldFontFamily : FontFamily;
        style.Font.Bold = false;
    }
    public const double RowHeightCm = 0.55;
    public const int RowCap = 500; // safety valve; Excel remains the full, uncapped export

    public static void ApplyPageSettings(Report report)
    {
        report.Width = Unit.Cm(PageWidthCm);
        report.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
        report.PageSettings.Landscape = true;
        report.PageSettings.Margins = new MarginsU(Unit.Cm(1.2), Unit.Cm(1.2), Unit.Cm(1.0), Unit.Cm(1.0));
    }

    /// <summary>The running "تقرير المشروع — name (id)" title, repeated on every page of a mini-report.</summary>
    public static PageHeaderSection BuildRunningHeader(string projectName, string projectId)
    {
        var header = new PageHeaderSection { Name = "pageHeader", Height = Unit.Cm(0.8) };
        header.Items.Add(Text("hdrTitle", $"تقرير المشروع — {projectName} ({projectId})",
            0, 0, PageWidthCm, 0.7, 12, true, HorizontalAlign.Right));
        return header;
    }

    public static TextBox Text(string name, string value, double x, double y, double w, double h,
        double size, bool bold, HorizontalAlign align, System.Drawing.Color? color = null)
    {
        var tb = new TextBox
        {
            Name = name,
            Value = value ?? "",
            Location = new PointU(Unit.Cm(x), Unit.Cm(y)),
            Size = new SizeU(Unit.Cm(w), Unit.Cm(h)),
            CanGrow = true,
        };
        ApplyFont(tb.Style, bold);
        tb.Style.Font.Size = Unit.Point(size);
        tb.Style.TextAlign = align;
        tb.Style.VerticalAlign = VerticalAlign.Middle;
        if (color.HasValue) tb.Style.Color = color.Value;
        return tb;
    }
}
