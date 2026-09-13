using System.Collections;
using Application.Projects;

namespace API.Reporting.ProjectReport;

/// <summary>
/// Builds the (title, columns, rows) triple for every non-empty project-report section, in
/// display order. One <see cref="ProjectReportTableReport"/> gets built per entry by
/// TelerikProjectReportService and appended to the report book after the summary.
/// </summary>
public static class ProjectReportSections
{
    public sealed record Section(string Title, IReadOnlyList<ProjectReportColumn> Columns, IList Rows);

    public static List<Section> BuildAll(ProjectReportDto data)
    {
        var expenseRows = ProjectReportPdfRows.BuildExpenses(data.Expenses);
        var directPaymentRows = ProjectReportPdfRows.BuildPayments(data.DirectPayments);
        var transactionRows = ProjectReportPdfRows.BuildPayments(data.AccountingTransactions);
        var payrollRows = ProjectReportPdfRows.BuildPayments(data.Payroll);
        var operatingRows = ProjectReportPdfRows.BuildPayments(data.OperatingExpenses);
        var (agreedRows, maintenanceRows) = ProjectReportPdfRows.BuildRevenues(data.Revenues);
        var salesRows = ProjectReportPdfRows.BuildSales(data.ApartmentSales);
        var commissionRows = ProjectReportPdfRows.BuildCommissions(data.PaidCommissions);

        var sections = new List<Section>
        {
            new($"المستخلصات ({expenseRows.Count})", ExpenseColumns, expenseRows),
            new($"الدفعات المباشرة ({directPaymentRows.Count})", PaymentColumns("رقم الدفعة", "من طرف", "إلى طرف"), directPaymentRows),
            new($"قيود محاسبية ({transactionRows.Count})", PaymentColumns("رقم القيد", "من طرف", "إلى طرف"), transactionRows),
            new($"رواتب المشروع ({payrollRows.Count})", PaymentColumns("رقم القيد", "الموظف", "المشروع"), payrollRows),
            new($"المصاريف التشغيلية ({operatingRows.Count})", PaymentColumns("رقم الدفعة", "من طرف", "إلى طرف"), operatingRows),
            new($"الإيرادات ({agreedRows.Count})", RevenueColumns("الإيراد المتفق عليه"), agreedRows),
            new($"وديعة الصيانة ({maintenanceRows.Count})", RevenueColumns("المجدول"), maintenanceRows),
            new($"مبيعات الوحدات ({salesRows.Count})", SalesColumns, salesRows),
            new($"العمولات المدفوعة ({commissionRows.Count})", CommissionColumns, commissionRows),
        };

        return sections.Where(s => s.Rows.Count > 0).ToList();
    }

    private static readonly ProjectReportColumn[] ExpenseColumns =
    {
        new("التاريخ", "ExpenseDate", 2.5, "{0:dd/MM/yyyy}"),
        new("وصف البند", "ItemDescription", 7),
        new("اسم الطرف", "PartyDisplay", 5),
        new("رقم الشهادة", "CertificateNumber", 3),
        new("النوع", "TypeDisplay", 4),
        new("صافي المعتمد", "NetCertifiedAmount", 3.5, "{0:N2}"),
    };

    private static ProjectReportColumn[] PaymentColumns(string idHeader, string fromHeader, string toHeader) => new[]
    {
        new ProjectReportColumn(idHeader, "PaymentId", 4),
        new ProjectReportColumn("النوع", "TypeDescription", 4),
        new ProjectReportColumn(fromHeader, "FromDisplay", 5),
        new ProjectReportColumn(toHeader, "ToDisplay", 5),
        new ProjectReportColumn("التاريخ", "EffectiveDate", 3, "{0:dd/MM/yyyy}"),
        new ProjectReportColumn("المبلغ", "Amount", 4, "{0:N2}"),
    };

    private static ProjectReportColumn[] RevenueColumns(string scheduledHeader) => new[]
    {
        new ProjectReportColumn("المبنى", "BuildingNumber", 2.5),
        new ProjectReportColumn("الوحدة", "ApartmentId", 3),
        new ProjectReportColumn("العميل", "CustomerName", 5),
        new ProjectReportColumn(scheduledHeader, "ScheduledAmount", 4, "{0:N2}"),
        new ProjectReportColumn("المحصل", "CollectedAmount", 3.5, "{0:N2}"),
        new ProjectReportColumn("المتبقي", "OutstandingAmount", 3.5, "{0:N2}"),
        new ProjectReportColumn("حالة الاستحقاق", "DueStatusDisplay", 4),
    };

    private static readonly ProjectReportColumn[] SalesColumns =
    {
        new("الوحدة", "ApartmentName", 4),
        new("المبنى", "BuildingNumber", 2.5),
        new("العميل", "CustomerName", 5),
        new("تاريخ البيع", "SaleDate", 3, "{0:dd/MM/yyyy}"),
        new("الإجمالي", "TotalPrice", 3.5, "{0:N2}"),
        new("المقدم", "AdvancePayment", 3.5, "{0:N2}"),
    };

    private static readonly ProjectReportColumn[] CommissionColumns =
    {
        new("رقم طلب البيع", "SalesRequestId", 4),
        new("الوحدة", "ApartmentName", 4),
        new("المستفيد", "PayeeDisplay", 5),
        new("المبلغ", "Amount", 3.5, "{0:N2}"),
        new("حالة الدفع", "PaymentStatusDisplay", 3.5),
        new("التاريخ", "EffectiveDate", 3, "{0:dd/MM/yyyy}"),
    };
}
