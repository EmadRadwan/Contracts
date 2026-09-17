using System.Globalization;
using Application.Accounting.Services.Models;
using Telerik.Reporting;
using Telerik.Reporting.Drawing;
using static API.Reporting.ProjectReport.ProjectReportLayout;

namespace API.Reporting.GlAccountTransactions;

/// <summary>
/// Code-defined Telerik report for the GL account transaction details drill-down (the modal shared
/// by the classic trial balance and the by-level trial balance). A4 landscape: running header with
/// company / account / period, an account-balance summary block, then a right-to-left table with
/// one row per ledger entry (debit, credit, running balance) and a debit/credit totals footer.
///
/// Built with the literal-TextBox-per-cell technique (every value computed in C#, Telerik only
/// lays it out) — the same approach the payment voucher and the project report settled on after
/// Telerik.Reporting.List / "= Fields.X" binding produced zero rows. Page setup and the text
/// helper are borrowed from ProjectReportLayout; they are generic despite the name.
/// </summary>
public sealed class GlAccountTransactionsReport : Report
{
    private const string TitleLabel = "تفاصيل الحركات لحساب";

    // Colours mirror the modal grid / Excel export: debit rows green-tinted, reversal amber,
    // reversed original grey with muted text (app/common/grid/ReversalRow.tsx).
    private static readonly System.Drawing.Color HeaderBg = System.Drawing.Color.FromArgb(0x1E, 0x3A, 0x5F);
    private static readonly System.Drawing.Color DebitBg = System.Drawing.Color.FromArgb(0xD9, 0xF2, 0xCC);
    private static readonly System.Drawing.Color ReversalBg = System.Drawing.Color.FromArgb(0xFF, 0xF1, 0xC2);
    private static readonly System.Drawing.Color ReversedBg = System.Drawing.Color.FromArgb(0xE6, 0xE8, 0xEB);
    private static readonly System.Drawing.Color ReversedFg = System.Drawing.Color.FromArgb(0x6B, 0x77, 0x85);
    private static readonly System.Drawing.Color TotalsBg = System.Drawing.Color.FromArgb(0xBF, 0xDB, 0xFE);

    /// <summary>One printed column: header, relative width, value getter, alignment.</summary>
    private sealed record Column(string Header, double Weight, Func<TransactionEntryDto, string> Value,
        HorizontalAlign Align = HorizontalAlign.Right);

    // Right-to-left order: the first column sits at the right edge of the page. Trimmed vs the
    // modal grid (no product / work-effort / is-posted) so the row fits one landscape line — Excel
    // remains the full-width export.
    private static readonly IReadOnlyList<Column> Columns = new[]
    {
        new Column("رقم القيد", 1.7, t => t.AcctgTransId ?? "", HorizontalAlign.Center),
        new Column("تاريخ الحركة", 1.9, t => t.TransactionDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), HorizontalAlign.Center),
        new Column("نوع القيد", 2.6, t => t.AcctgTransTypeDescription ?? t.AcctgTransTypeId ?? ""),
        new Column("مدين", 2.2, t => t.DebitCreditFlag == "D" ? Money(t.Amount) : "", HorizontalAlign.Left),
        new Column("دائن", 2.2, t => t.DebitCreditFlag == "C" ? Money(t.Amount) : "", HorizontalAlign.Left),
        new Column("الرصيد", 2.4, t => Money(t.RunningBalance), HorizontalAlign.Left),
        new Column("رقم الفاتورة", 1.7, t => t.InvoiceId ?? "", HorizontalAlign.Center),
        new Column("رقم الدفعة", 1.7, t => t.PaymentId ?? "", HorizontalAlign.Center),
        new Column("مركز التكلفة", 2.4, t => t.CostCenterDescription ?? ""),
        new Column("اسم الطرف", 3.2, t => t.PartyName ?? t.PartyId ?? ""),
        new Column("البيان", 4.0, t => t.Description ?? ""),
    };

    public GlAccountTransactionsReport(GlAccountTransactionDetails data)
    {
        ArgumentNullException.ThrowIfNull(data);

        Name = "GlAccountTransactionsReport";
        ApplyPageSettings(this);
        Items.Add(BuildHeader(data));
        Items.Add(BuildDetail(data));
        Items.Add(BuildFooter());
    }

    private static PageHeaderSection BuildHeader(GlAccountTransactionDetails d)
    {
        var header = new PageHeaderSection { Name = "pageHeader", Height = Unit.Cm(1.6) };
        header.Items.Add(Text("hdrTitle", $"{TitleLabel} {d.AccountName} ({d.AccountCode})",
            0, 0, PageWidthCm, 0.7, 13, true, HorizontalAlign.Right));

        var period = d.PeriodName ?? "";
        if (d.PeriodFromDate.HasValue && d.PeriodThruDate.HasValue)
            period += $" ({d.PeriodFromDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} - {d.PeriodThruDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)})";
        header.Items.Add(Text("hdrSub", $"{d.OrganizationName} — الفترة: {period}",
            0, 0.75, PageWidthCm, 0.5, 9, false, HorizontalAlign.Right, System.Drawing.Color.Gray));
        return header;
    }

    private static DetailSection BuildDetail(GlAccountTransactionDetails d)
    {
        var section = new DetailSection { Name = "secTransactions" };
        var rows = d.Transactions ?? new List<TransactionEntryDto>();
        double y = 0;

        // ---- account balances (label on the right, value to its left — RTL) -------------------
        void SummaryLine(string label, decimal value, bool bold = false)
        {
            section.Items.Add(Text("sumL" + y, label, PageWidthCm - 6.0, y, 6.0, 0.5, 10, bold, HorizontalAlign.Right));
            section.Items.Add(Text("sumV" + y, Money(value), PageWidthCm - 10.5, y, 4.5, 0.5, 10, bold, HorizontalAlign.Left));
            y += 0.5;
        }

        SummaryLine("الرصيد الافتتاحي:", d.OpeningBalance, true);
        SummaryLine("المدين المرحل:", d.PostedDebits);
        SummaryLine("الدائن المرحل:", d.PostedCredits);
        SummaryLine("الرصيد الختامي:", d.EndingBalance, true);
        y += 0.3;

        // ---- column geometry --------------------------------------------------------------------
        double totalWeight = Columns.Sum(c => c.Weight);
        double scale = totalWeight > 0 ? PageWidthCm / totalWeight : 1;
        var colWidths = Columns.Select(c => c.Weight * scale).ToList();

        // ---- header row ------------------------------------------------------------------------
        double cursor = 0;
        for (var i = 0; i < Columns.Count; i++)
        {
            var w = colWidths[i];
            var hb = Text($"h{i}", Columns[i].Header, PageWidthCm - cursor - w, y, w, RowHeightCm, 9, true,
                HorizontalAlign.Center, System.Drawing.Color.White);
            hb.Style.BackgroundColor = HeaderBg;
            section.Items.Add(hb);
            cursor += w;
        }
        y += RowHeightCm;

        var capped = rows.Count > RowCap;
        if (capped)
        {
            section.Items.Add(Text("capNote", $"يعرض أول {RowCap} حركة فقط — التصدير الكامل عبر Excel.",
                0, y, PageWidthCm, 0.45, 8, false, HorizontalAlign.Right, System.Drawing.Color.Gray));
            y += 0.45;
        }

        // ---- data rows -------------------------------------------------------------------------
        var shown = Math.Min(rows.Count, RowCap);
        for (var r = 0; r < shown; r++)
        {
            var t = rows[r];
            var isReversal = !string.IsNullOrEmpty(t.ReversalOfAcctgTransId);
            var isReversed = !string.IsNullOrEmpty(t.ReversedByAcctgTransId);
            System.Drawing.Color? bg = isReversal ? ReversalBg
                : isReversed ? ReversedBg
                : t.DebitCreditFlag == "D" ? DebitBg
                : (System.Drawing.Color?)null;

            cursor = 0;
            for (var i = 0; i < Columns.Count; i++)
            {
                var w = colWidths[i];
                var tb = Text($"r{r}c{i}", Columns[i].Value(t), PageWidthCm - cursor - w, y, w, RowHeightCm, 8,
                    false, Columns[i].Align, isReversed ? ReversedFg : (System.Drawing.Color?)null);
                tb.CanGrow = false;
                tb.Style.BorderStyle.Bottom = BorderType.Solid;
                tb.Style.BorderWidth.Bottom = Unit.Point(0.5);
                tb.Style.BorderColor.Bottom = System.Drawing.Color.LightGray;
                if (bg.HasValue) tb.Style.BackgroundColor = bg.Value;
                section.Items.Add(tb);
                cursor += w;
            }
            y += RowHeightCm;
        }

        // ---- totals row (all rows, not only the RowCap-shown ones — matches the modal footer) ----
        var totalDebit = rows.Where(t => t.DebitCreditFlag == "D").Sum(t => t.Amount);
        var totalCredit = rows.Where(t => t.DebitCreditFlag != "D").Sum(t => t.Amount);
        y += 0.1;
        var totals = Text("totals",
            $"إجمالي المدين: {Money(totalDebit)}     |     إجمالي الدائن: {Money(totalCredit)}     |     عدد الحركات: {rows.Count}",
            0, y, PageWidthCm, 0.6, 10, true, HorizontalAlign.Right);
        totals.Style.BackgroundColor = TotalsBg;
        section.Items.Add(totals);
        y += 0.6;

        section.Height = Unit.Cm(y + 0.2);
        return section;
    }

    private static PageFooterSection BuildFooter()
    {
        var footer = new PageFooterSection { Name = "pageFooter", Height = Unit.Cm(0.6) };
        var page = Text("pageNo", "", 0, 0, PageWidthCm, 0.5, 8, false, HorizontalAlign.Center, System.Drawing.Color.Gray);
        // Built-in page-number expression — the one Telerik expression in this report; if it ever
        // renders literally, swap for the Telerik.Reporting PageNumber item.
        page.Value = "= 'صفحة ' + PageNumber + ' / ' + PageCount";
        footer.Items.Add(page);
        return footer;
    }

    private static string Money(decimal value) => value.ToString("N2", CultureInfo.InvariantCulture);
}
