using Application.Projects;
using Telerik.Reporting;
using Telerik.Reporting.Drawing;
using static API.Reporting.ProjectReport.ProjectReportLayout;

namespace API.Reporting.ProjectReport;

/// <summary>
/// First mini-report in the project report's ReportBook — the summary. Static, literal-value
/// content (no data binding), same technique as PaymentVoucherReport: exactly one DetailSection
/// with no DataSource renders once.
/// </summary>
public sealed class ProjectReportSummaryReport : Report
{
    public ProjectReportSummaryReport(ProjectReportSummaryDto summary, string projectName, string projectId, string period)
    {
        ArgumentNullException.ThrowIfNull(summary);

        Name = "ProjectReportSummaryReport";
        ApplyPageSettings(this);

        var header = BuildRunningHeader(projectName, projectId);
        header.Height = Unit.Cm(1.3);
        header.Items.Add(Text("hdrPeriod", period, 0, 0.7, PageWidthCm, 0.5, 9, false, HorizontalAlign.Right,
            System.Drawing.Color.Gray));
        Items.Add(header);

        Items.Add(BuildSummarySection(summary));
    }

    private static DetailSection BuildSummarySection(ProjectReportSummaryDto s)
    {
        var section = new DetailSection { Name = "secSummary", Height = Unit.Cm(1) };
        double y = 0;

        void SectionHeader(string label)
        {
            var tb = Text("sumHdr" + y, label, 0, y, PageWidthCm, 0.6, 12, true, HorizontalAlign.Right,
                System.Drawing.Color.White);
            tb.Style.BackgroundColor = System.Drawing.Color.FromArgb(0x1E, 0x40, 0xAF);
            section.Items.Add(tb);
            y += 0.65;
        }

        void Line(string label, decimal value, bool bold = false, bool isInt = false)
        {
            var l = Text("sumL" + y, label, PageWidthCm * 0.35, y, PageWidthCm * 0.65, 0.5, 10, bold, HorizontalAlign.Right);
            var v = Text("sumV" + y, value.ToString(isInt ? "N0" : "N2"), 0, y, PageWidthCm * 0.35, 0.5, 10, bold, HorizontalAlign.Left);
            if (bold)
            {
                l.Style.BackgroundColor = System.Drawing.Color.FromArgb(0xBF, 0xDB, 0xFE);
                v.Style.BackgroundColor = System.Drawing.Color.FromArgb(0xBF, 0xDB, 0xFE);
            }
            section.Items.Add(l);
            section.Items.Add(v);
            y += 0.55;
        }

        SectionHeader("المصاريف");
        Line("المستخلصات", s.CertificateExpenses);
        Line("الدفعات المباشرة", s.DirectPayments);
        Line("قيود محاسبية", s.AccountingTransactions);
        Line("رواتب المشروع", s.ProjectPayroll);
        Line("المصاريف التشغيلية", s.OperatingExpenses);
        Line("إجمالي مصاريف المشروع", s.TotalProjectExpenses, true);
        y += 0.2;

        SectionHeader("الإيرادات");
        Line("الإيراد المتفق عليه", s.RevenueScheduled);
        Line("المحصل", s.RevenueCollected);
        Line("المتبقي", s.RevenueOutstanding);
        y += 0.2;

        SectionHeader("وديعة الصيانة");
        Line("الإجمالي", s.MaintenanceScheduled);
        Line("المحصل", s.MaintenanceCollected);
        Line("المتبقي", s.MaintenanceOutstanding);
        y += 0.2;

        SectionHeader("مبيعات الوحدات");
        Line("عدد الوحدات المباعة", s.UnitsSold, isInt: true);
        Line("إجمالي القيمة", s.UnitsSoldValue);
        Line("إجمالي المقدمات المحصلة", s.UnitsAdvanceCollected);
        Line("عدد الوحدات المتاحة", s.UnitsAvailable, isInt: true);
        y += 0.2;

        SectionHeader("العمولات");
        Line("عدد الدفعات", s.CommissionPaymentCount, isInt: true);
        Line("المدفوعة", s.CommissionsPaid);
        Line("المستحقة", s.CommissionsPending);
        Line("الإجمالي", s.CommissionsPaid + s.CommissionsPending, true);
        y += 0.2;

        var excludedLabel = s.MgmtExcludedBuildings.Count > 0
            ? $" (عدا {string.Join("، ", s.MgmtExcludedBuildings)})"
            : "";
        SectionHeader("مبلغ الإدارة");
        Line("الأساس" + excludedLabel, s.MgmtFeeBase);
        Line($"النسبة ({s.MgmtFeePercent}%)", s.MgmtFee);
        Line("يُخصم: المصاريف التشغيلية", -s.OperatingExpenses);
        Line("الصافي المتبقي", s.MgmtFeeNet, true);
        y += 0.2;

        SectionHeader("الصافي");
        Line("المحصل − مصاريف المشروع", s.NetAfterExpenses, true);
        Line("بعد خصم العمولات المدفوعة", s.NetAfterPaidCommissions, true);

        section.Height = Unit.Cm(y + 0.3);
        return section;
    }
}
