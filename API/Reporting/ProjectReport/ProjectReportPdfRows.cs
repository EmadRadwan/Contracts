using Application.Accounting.Payments;
using Application.Order.SalesRequests;
using Application.Projects;

namespace API.Reporting.ProjectReport;

// Flat, pre-formatted row shapes for the PDF tables. Values are fully resolved in C# (fallbacks,
// conditional blanks) so the Telerik report only ever does a plain "= Fields.PropertyName" bind —
// no expression-language risk, same philosophy as the payment voucher (PaymentVoucherReport.cs).

public class PdfExpenseRow
{
    public DateTime? ExpenseDate { get; set; }
    public string ItemDescription { get; set; } = "";
    public string PartyDisplay { get; set; } = "";
    public string CertificateNumber { get; set; } = "";
    public string TypeDisplay { get; set; } = "";
    public decimal NetCertifiedAmount { get; set; }
}

// Shared by direct payments, operating expenses, accounting transactions and payroll —
// all four sections are the same 6-column shape in the Excel/grid sheets.
public class PdfPaymentRow
{
    public string PaymentId { get; set; } = "";
    public string TypeDescription { get; set; } = "";
    public string FromDisplay { get; set; } = "";
    public string ToDisplay { get; set; } = "";
    public DateTime? EffectiveDate { get; set; }
    public decimal Amount { get; set; }
}

public class PdfRevenueRow
{
    public string BuildingNumber { get; set; } = "";
    public string ApartmentId { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public decimal ScheduledAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string DueStatusDisplay { get; set; } = "";
}

public class PdfSalesRow
{
    public string ApartmentName { get; set; } = "";
    public string BuildingNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public DateTime? SaleDate { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal AdvancePayment { get; set; }
}

public class PdfCommissionRow
{
    public string SalesRequestId { get; set; } = "";
    public string ApartmentName { get; set; } = "";
    public string PayeeDisplay { get; set; } = "";
    public decimal Amount { get; set; }
    public string PaymentStatusDisplay { get; set; } = "";
    public DateTime? EffectiveDate { get; set; }
}

public static class ProjectReportPdfRows
{
    private static DateTime? ToDt(DateOnly? d) => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : null;
    private static string Or(string? a, string? b) => !string.IsNullOrWhiteSpace(a) ? a! : (b ?? "");

    public static bool IsMaintenance(ProjectRevenueRecord r) =>
        r.PaymentTypeId == "RECEIPT_MAINTENANCE_AMOUNT" || r.RevenueCategory == "Maintenance Deposit";

    public static List<PdfExpenseRow> BuildExpenses(IEnumerable<ProjectExpenseRecord> expenses) =>
        expenses.Select(e => new PdfExpenseRow
        {
            ExpenseDate = e.ExpenseDate,
            ItemDescription = e.ItemDescription ?? "",
            PartyDisplay = Or(e.PartyName, e.PartyId),
            CertificateNumber = e.CertificateNumber ?? "",
            TypeDisplay = Or(e.CertificateTypeArabic, e.CertificateType),
            NetCertifiedAmount = e.NetCertifiedAmount,
        }).ToList();

    public static List<PdfPaymentRow> BuildPayments(IEnumerable<PaymentRecord> payments) =>
        payments.Select(p => new PdfPaymentRow
        {
            PaymentId = p.PaymentId ?? "",
            TypeDescription = p.PaymentTypeDescription ?? "",
            FromDisplay = p.PartyIdFromName ?? "",
            ToDisplay = p.PartyIdToName ?? "",
            EffectiveDate = ToDt(p.EffectiveDate),
            Amount = p.Amount,
        }).ToList();

    public static (List<PdfRevenueRow> Agreed, List<PdfRevenueRow> Maintenance) BuildRevenues(
        IEnumerable<ProjectRevenueRecord> revenues)
    {
        var list = revenues.ToList();
        PdfRevenueRow Map(ProjectRevenueRecord r) => new()
        {
            BuildingNumber = r.BuildingNumber ?? "",
            ApartmentId = r.ApartmentId ?? "",
            CustomerName = r.CustomerName ?? "",
            ScheduledAmount = r.ScheduledAmount,
            CollectedAmount = r.CollectedAmount,
            OutstandingAmount = r.OutstandingAmount,
            DueStatusDisplay = r.DueStatusArabic ?? "",
        };
        var agreed = list.Where(r => !IsMaintenance(r)).Select(Map).ToList();
        var maintenance = list.Where(IsMaintenance).Select(Map).ToList();
        return (agreed, maintenance);
    }

    public static List<PdfSalesRow> BuildSales(IEnumerable<SalesRequestOrApartmentRecord> sales) =>
        sales.Select(s => new PdfSalesRow
        {
            ApartmentName = s.ApartmentName ?? "",
            BuildingNumber = s.BuildingNumber ?? "",
            CustomerName = s.FromPartyName ?? "",
            SaleDate = s.IsSold ? ToDt(s.SaleDate) : null,
            TotalPrice = s.IsSold ? (s.TotalPrice ?? 0m) : 0m,
            AdvancePayment = s.IsSold ? (s.AdvancePayment ?? 0m) : 0m,
        }).ToList();

    public static List<PdfCommissionRow> BuildCommissions(IEnumerable<ProjectCommissionPaymentRecord> commissions) =>
        commissions.Select(c => new PdfCommissionRow
        {
            SalesRequestId = c.SalesRequestId ?? "",
            ApartmentName = c.ApartmentName ?? "",
            PayeeDisplay = Or(c.PayeeName, c.PayeePartyId),
            Amount = c.Amount,
            PaymentStatusDisplay = c.PaymentStatusArabic ?? "",
            EffectiveDate = ToDt(c.EffectiveDate),
        }).ToList();
}
