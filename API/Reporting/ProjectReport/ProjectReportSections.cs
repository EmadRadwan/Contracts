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
    /// <param name="Total">
    /// Optional bold total row under the table: (label, amount, field of the column the amount
    /// sits under). Only sections the client reads as a ledger block carry one — everything else
    /// is totalled on the summary page.
    /// </param>
    public sealed record Section(
        string Title, IReadOnlyList<ProjectReportColumn> Columns, IList Rows, SectionTotal? Total = null);

    public sealed record SectionTotal(string Label, decimal Amount, string Field);

    public static List<Section> BuildAll(ProjectReportDto data)
    {
        var expenseRows = ProjectReportPdfRows.BuildExpenses(data.Expenses);
        // Direct payments split into a paid block and an unpaid (open commitments) block, each
        // with its own total — same partition as the Excel sheet and the on-screen tab.
        var directPaidRows = ProjectReportPdfRows.BuildPayments(
            data.DirectPayments.Where(p => !ProjectReportSummaryDto.IsUnpaid(p)));
        var directUnpaidRows = ProjectReportPdfRows.BuildPayments(
            data.DirectPayments.Where(ProjectReportSummaryDto.IsUnpaid));
        var transactionRows = ProjectReportPdfRows.BuildPayments(data.AccountingTransactions);
        var payrollRows = ProjectReportPdfRows.BuildPayments(data.Payroll);
        var operatingPaidRows = ProjectReportPdfRows.BuildPayments(
            data.OperatingExpenses.Where(p => !ProjectReportSummaryDto.IsUnpaid(p)));
        var operatingUnpaidRows = ProjectReportPdfRows.BuildPayments(
            data.OperatingExpenses.Where(ProjectReportSummaryDto.IsUnpaid));
        var revenueRows = ProjectReportPdfRows.BuildRevenues(data.Revenues);
        var maintenanceRows = ProjectReportPdfRows.BuildMaintenance(data.MaintenanceDeposits);
        var salesRows = ProjectReportPdfRows.BuildSales(data.ApartmentSales);
        var commissionRows = ProjectReportPdfRows.BuildCommissions(data.PaidCommissions);
        var commissionEntryRows = ProjectReportPdfRows.BuildCommissionEntries(data.PaidCommissionAcctgEntries);

        var sections = new List<Section>
        {
            new($"المستخلصات ({expenseRows.Count})", ExpenseColumns, expenseRows),
            new($"الدفعات المباشرة — مدفوعة ({directPaidRows.Count})", DirectPaymentColumns, directPaidRows,
                new SectionTotal("إجمالي الدفعات المباشرة المدفوعة", data.Summary.DirectPaymentsPaid, "Amount")),
            new($"الدفعات المباشرة — غير مدفوعة ({directUnpaidRows.Count}) — للعرض فقط", DirectPaymentColumns, directUnpaidRows,
                new SectionTotal("إجمالي الدفعات المباشرة غير المدفوعة (لا يدخل في الحساب)", data.Summary.DirectPaymentsUnpaid, "Amount")),
            new($"قيود محاسبية ({transactionRows.Count})", PaymentColumns("رقم القيد", "من طرف", "إلى طرف"), transactionRows),
            new($"رواتب المشروع ({payrollRows.Count})", PaymentColumns("رقم القيد", "الموظف", "المشروع"), payrollRows),
            new($"المصاريف التشغيلية — مدفوعة ({operatingPaidRows.Count})", DirectPaymentColumns, operatingPaidRows,
                new SectionTotal("إجمالي المصاريف التشغيلية المدفوعة", data.Summary.OperatingExpenses, "Amount")),
            new($"المصاريف التشغيلية — غير مدفوعة ({operatingUnpaidRows.Count}) — للعرض فقط", DirectPaymentColumns, operatingUnpaidRows,
                new SectionTotal("إجمالي المصاريف التشغيلية غير المدفوعة (لا يدخل في الحساب)", data.Summary.OperatingExpensesUnpaid, "Amount")),
            new($"الإيرادات ({revenueRows.Count})", RevenueColumns, revenueRows),
            new($"وديعة الصيانة ({maintenanceRows.Count})", MaintenanceColumns, maintenanceRows),
            new($"مبيعات الوحدات ({salesRows.Count})", SalesColumns, salesRows),
            new($"العمولات المدفوعة ({commissionRows.Count})", CommissionColumns, commissionRows),
            new($"قيود العمولات المدفوعة ({commissionEntryRows.Count})", CommissionEntryColumns, commissionEntryRows),
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

    // Direct payments carry a status column (due status for the unpaid block) — the other
    // payment-style sections keep the shared 6-column layout.
    private static readonly ProjectReportColumn[] DirectPaymentColumns =
    {
        new("رقم الدفعة", "PaymentId", 3.5),
        new("النوع", "TypeDescription", 4),
        new("من طرف", "FromDisplay", 4.5),
        new("إلى طرف", "ToDisplay", 4.5),
        new("الحالة", "StatusDisplay", 3.5),
        new("التاريخ", "EffectiveDate", 2.5, "{0:dd/MM/yyyy}"),
        new("المبلغ", "Amount", 3.5, "{0:N2}"),
    };

    private static readonly ProjectReportColumn[] RevenueColumns =
    {
        new("الفئة", "CategoryDisplay", 3.5),
        new("المبنى", "BuildingNumber", 2),
        new("الوحدة", "ApartmentId", 2.5),
        new("العميل", "CustomerName", 5),
        new("المجدول", "ScheduledAmount", 3.5, "{0:N2}"),
        new("المحصل", "CollectedAmount", 3.5, "{0:N2}"),
        new("المتبقي", "OutstandingAmount", 3.5, "{0:N2}"),
        new("حالة الاستحقاق", "DueStatusDisplay", 4),
    };

    // One row per sold unit: agreed deposit (sales request) vs. collected receipts.
    private static readonly ProjectReportColumn[] MaintenanceColumns =
    {
        new("رقم الطلب", "SalesRequestId", 2.5),
        new("المبنى", "BuildingNumber", 2),
        new("الوحدة", "ApartmentName", 4),
        new("العميل", "CustomerName", 5),
        new("تاريخ البيع", "SaleDate", 2.5, "{0:dd/MM/yyyy}"),
        new("وديعة الصيانة", "MaintenanceDeposit", 3.5, "{0:N2}"),
        new("المحصل", "CollectedAmount", 3, "{0:N2}"),
        new("المتبقي", "OutstandingAmount", 3, "{0:N2}"),
        new("الحالة", "StatusDisplay", 4),
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
        new("حالة الدفع", "PaymentStatusDisplay", 3),
        new("التاريخ", "EffectiveDate", 2.5, "{0:dd/MM/yyyy}"),
        new("رقم الدفعة", "PaymentId", 3),
        new("مركز التكلفة", "CostCenterDisplay", 4),
        new("الحساب البديل", "OverrideGlAccountDisplay", 5.5),
    };

    private static readonly ProjectReportColumn[] CommissionEntryColumns =
    {
        new("رقم الدفعة", "PaymentId", 3),
        new("رقم القيد", "AcctgTransId", 3),
        new("نوع القيد", "TypeDescription", 3.5),
        new("تاريخ القيد", "TransactionDate", 2.5, "{0:dd/MM/yyyy}"),
        new("مرحّل", "PostedDisplay", 1.5),
        new("الحساب", "GlAccountDisplay", 6.5),
        new("مدين", "Debit", 3, "{0:N2}"),
        new("دائن", "Credit", 3, "{0:N2}"),
        new("مركز التكلفة", "CostCenterDisplay", 3.5),
    };
}
