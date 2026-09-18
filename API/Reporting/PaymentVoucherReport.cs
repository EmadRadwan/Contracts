using System.Globalization;
using Application.Reports;
using SkiaSharp;
using Telerik.Reporting;
using Telerik.Reporting.Drawing;

namespace API.Reporting;

/// <summary>
/// Code-defined Telerik report for the payment voucher (بيان صرف / إيصال قبض).
///
/// Authored in code rather than as a <c>.trdp</c> because the Standalone / VS Report Designer
/// is Windows-only and this repo's development happens on macOS (see
/// docs/telerik-reporting-integration-plan.md §7). The layout is a port of the original QuestPDF
/// voucher (removed 2026-09-17 — Telerik is now the only PDF engine).
///
/// The section has no data source, so the DetailSection renders exactly once. All values are
/// literal — they are pushed in through the constructor from the already-resolved
/// <see cref="PaymentReportDto"/>, so there are no <c>Fields.</c> binding expressions.
/// </summary>
public sealed class PaymentVoucherReport : Report
{
    // A4 portrait (21cm) minus 1.5cm margins on each side.
    private const double PageWidthCm = 18.0;

    // Dev/mac-safe family. The Docker image gets Arabic faces added in a later pass
    // (plan §9.2); swap this for "Noto Naskh Arabic" / "Amiri" once those are installed.
    private const string FontFamily = API.Reporting.ProjectReport.ProjectReportLayout.FontFamily; // shared private font (Amiri); bold via ApplyFont

    public PaymentVoucherReport(PaymentReportDto data, string companyName)
    {
        ArgumentNullException.ThrowIfNull(data);

        var isReceipt = string.Equals(data.PaymentParentTypeDescription?.Trim(), "RECEIPT",
            StringComparison.OrdinalIgnoreCase);

        var methodId = data.PaymentMethodId?.ToUpperInvariant() ?? string.Empty;
        var methodDesc = data.PaymentMethodDescription?.ToUpperInvariant() ?? string.Empty;
        var isCash = methodId.Contains("CASH") || methodDesc.Contains("نقد");
        var isBankTransfer = !isCash && data.IsBankTransfer == true;
        var isCheque = !isCash && !isBankTransfer;

        var currencySuffix = ArabicPaymentFormatter.CurrencySuffix(data.CurrencyUomId);
        var amountInWords = ArabicPaymentFormatter.AmountToWords(data.Amount, currencySuffix);
        var pounds = (long)data.Amount;
        var piastres = (int)Math.Round((data.Amount - pounds) * 100m);

        Name = "PaymentVoucherReport";
        Width = Unit.Cm(PageWidthCm);
        Culture = API.Reporting.ProjectReport.ProjectReportLayout.ReportCulture; // RTL paragraph direction, see ReportCulture
        PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
        PageSettings.Margins = new MarginsU(Unit.Cm(1.5), Unit.Cm(1.5), Unit.Cm(1.5), Unit.Cm(1.5));

        var header = new PageHeaderSection { Name = "pageHeader", Height = Unit.Cm(3.4) };
        var detail = new DetailSection { Name = "detail", Height = Unit.Cm(22) };
        Items.Add(header);
        Items.Add(detail);

        BuildHeader(header, data, companyName, isReceipt);
        BuildBody(detail, data, amountInWords, pounds, piastres, isReceipt, isCash, isCheque, isBankTransfer);
    }

    private static void BuildHeader(PageHeaderSection header, PaymentReportDto data, string companyName, bool isReceipt)
    {
        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "goldenlandlogo.jpg");
        if (File.Exists(logoPath))
        {
            // PictureBox.Value accepts string | IImage | System.Drawing.Image | SKBitmap only —
            // not byte[]. Decode to an SKBitmap for the Skia rendering pipeline.
            var logo = new PictureBox
            {
                Name = "logo",
                Value = SKBitmap.Decode(logoPath),
                Sizing = ImageSizeMode.ScaleProportional,
                Location = new PointU(Unit.Cm(0), Unit.Cm(0)),
                Size = new SizeU(Unit.Cm(4), Unit.Cm(2.4)),
            };
            header.Items.Add(logo);
        }

        var title = Text("title", isReceipt ? "إيصال قبض" : "إيصال صرف",
            x: 4.5, y: 0.2, w: 9, h: 1.1, size: 22, bold: true, align: HorizontalAlign.Center);
        var idBox = Text("paymentId", data.PaymentId,
            x: 4.5, y: 1.4, w: 9, h: 1.0, size: 20, bold: true, align: HorizontalAlign.Center);

        var company1 = Text("company1", string.IsNullOrWhiteSpace(companyName) ? "جولدن لاند" : companyName,
            x: 12.5, y: 0.1, w: 5.5, h: 0.9, size: 16, bold: true, align: HorizontalAlign.Right);
        var company2 = Text("company2", "للتطوير العقارى",
            x: 12.5, y: 1.0, w: 5.5, h: 0.8, size: 12, bold: false, align: HorizontalAlign.Right);
        var company3 = Text("company3", "ش.م.م",
            x: 12.5, y: 1.7, w: 5.5, h: 0.7, size: 10, bold: false, align: HorizontalAlign.Right);

        // Horizontal rule: an empty TextBox carrying only a bottom border (avoids the
        // Shape/LineShape item type, whose namespace has moved between Reporting versions).
        var rule = new TextBox
        {
            Name = "headerRule",
            Value = string.Empty,
            Location = new PointU(Unit.Cm(0), Unit.Cm(3.0)),
            Size = new SizeU(Unit.Cm(PageWidthCm), Unit.Cm(0.1)),
        };
        rule.Style.BorderStyle.Bottom = BorderType.Solid;
        rule.Style.BorderWidth.Bottom = Unit.Point(1);
        rule.Style.BorderColor.Bottom = System.Drawing.Color.Gray;

        foreach (var item in new ReportItemBase[] { title, idBox, company1, company2, company3, rule })
            header.Items.Add(item);
    }

    private static void BuildBody(DetailSection detail, PaymentReportDto data, string amountInWords,
        long pounds, int piastres, bool isReceipt, bool isCash, bool isCheque, bool isBankTransfer)
    {
        double y = 0.2;

        // ===== Payment-method checkboxes (bank transfer / cheque / cash) =====
        detail.Items.Add(Checkbox("cbBank", "تحويل بنكى", isBankTransfer, x: 12.4, y: y, w: 5.6));
        detail.Items.Add(Checkbox("cbCheque", "شيكات", isCheque, x: 8.4, y: y, w: 3.6));
        detail.Items.Add(Checkbox("cbCash", "نقدية", isCash, x: 5.0, y: y, w: 3.0));
        y += 1.2;

        // ===== Date =====
        detail.Items.Add(Text("dateLabel", "تحريراً فى : " + ArabicPaymentFormatter.FormatArabicDate(data.EffectiveDate),
            x: 8, y: y, w: 10, h: 0.8, size: 12, bold: false, align: HorizontalAlign.Right));
        y += 1.1;

        // ===== Amount box: [ pounds ] جنيه [ piastres ] قرش =====
        // Amount digits are deliberately Western (0-9), not Arabic-Indic — client request; dates,
        // cheque number and the amount-in-words stay Arabic.
        detail.Items.Add(Text("piastresLabel", "قرش", x: 0.0, y: y, w: 1.4, h: 0.9, size: 11, bold: false, align: HorizontalAlign.Center));
        detail.Items.Add(Boxed("piastresBox", piastres.ToString("00", CultureInfo.InvariantCulture),
            x: 1.5, y: y, w: 2.2, h: 0.9, size: 12));
        detail.Items.Add(Text("poundsLabel", "جنيه", x: 3.9, y: y, w: 1.6, h: 0.9, size: 11, bold: false, align: HorizontalAlign.Center));
        detail.Items.Add(Boxed("poundsBox", pounds.ToString("N0", CultureInfo.InvariantCulture),
            x: 5.7, y: y, w: 6.0, h: 0.9, size: 13));
        y += 1.3;

        // ===== Recipient line =====
        var recipientCaption = isReceipt
            ? "استلمنا نحن / جولدن لاند للتطوير العقاري من السيد / السادة :"
            : "صرفنا إلى السيد / السادة :";
        detail.Items.Add(Text("recipientCaption", recipientCaption,
            x: 8.5, y: y, w: 9.5, h: 0.8, size: 12, bold: false, align: HorizontalAlign.Right));
        var recipientName = isReceipt ? data.FromPartyName : data.ToPartyName;
        detail.Items.Add(Underlined("recipientName", recipientName ?? string.Empty,
            x: 0.0, y: y, w: 8.2, h: 0.8, size: 12));
        y += 1.1;

        // ===== Amount in words =====
        detail.Items.Add(Text("wordsLabel", "فقط وقدره :",
            x: 14.0, y: y, w: 4.0, h: 0.8, size: 12, bold: false, align: HorizontalAlign.Right));
        detail.Items.Add(Underlined("wordsValue", amountInWords,
            x: 0.0, y: y, w: 13.6, h: 0.8, size: 12));
        y += 1.2;

        // ===== "نقداً / بموجب" =====
        detail.Items.Add(Text("byLabel", "نقداً / بموجب :",
            x: 13.5, y: y, w: 4.5, h: 0.8, size: 11, bold: false, align: HorizontalAlign.Right));
        detail.Items.Add(Underlined("byValue", isCash ? "نقداً" : (isCheque ? "شيك" : string.Empty),
            x: 0.0, y: y, w: 13.1, h: 0.8, size: 11));
        y += 1.0;

        // ===== Bank =====
        detail.Items.Add(Text("bankLabel", "مسحوب على بنك :",
            x: 13.5, y: y, w: 4.5, h: 0.8, size: 11, bold: false, align: HorizontalAlign.Right));
        detail.Items.Add(Underlined("bankValue", string.Empty, x: 0.0, y: y, w: 13.1, h: 0.8, size: 11));
        y += 1.0;

        // ===== Cheque number & date =====
        var chequeDateText = data.ChequeDate.HasValue
            ? ArabicPaymentFormatter.FormatArabicDate(data.ChequeDate.Value)
            : "٢٠    /    /    ";
        detail.Items.Add(Text("chequeLabel", $"رقم :", x: 15.6, y: y, w: 2.4, h: 0.8, size: 11, bold: false, align: HorizontalAlign.Right));
        detail.Items.Add(Underlined("chequeNumber", ArabicPaymentFormatter.ToArabicNumerals(data.ChequeNumber ?? string.Empty),
            x: 9.5, y: y, w: 6.0, h: 0.8, size: 11));
        detail.Items.Add(Text("chequeDate", "حق " + chequeDateText, x: 0.0, y: y, w: 9.0, h: 0.8, size: 11, bold: false, align: HorizontalAlign.Right));
        y += 1.0;

        // ===== Bank transfer (online) =====
        detail.Items.Add(Text("transferLabel", "تحويل ( بنكى ، اون لاين ) :",
            x: 12.0, y: y, w: 6.0, h: 0.8, size: 11, bold: false, align: HorizontalAlign.Right));
        detail.Items.Add(Underlined("transferValue", isBankTransfer ? (data.PaymentMethodDescription ?? string.Empty) : string.Empty,
            x: 0.0, y: y, w: 11.6, h: 0.8, size: 11));
        y += 1.2;

        // ===== Purpose =====
        detail.Items.Add(Text("purposeLabel", "وذلك عن :",
            x: 14.0, y: y, w: 4.0, h: 0.8, size: 12, bold: false, align: HorizontalAlign.Right));
        y += 0.8;
        detail.Items.Add(Underlined("purposeValue", data.Comments ?? string.Empty,
            x: 0.0, y: y, w: PageWidthCm, h: 1.2, size: 12));
        y += 2.4;

        // ===== Signatures =====
        detail.Items.Add(Text("sigApprovedLabel", "يعتمد ...", x: 0.0, y: y, w: 5.5, h: 0.8, size: 12, bold: false, align: HorizontalAlign.Left));
        detail.Items.Add(Text("sigAccountantLabel", "المحاسب", x: 6.3, y: y, w: 5.4, h: 0.8, size: 12, bold: false, align: HorizontalAlign.Center));
        detail.Items.Add(Text("sigRecipientLabel", "المستلم", x: 12.5, y: y, w: 5.5, h: 0.8, size: 12, bold: false, align: HorizontalAlign.Right));
        y += 1.6;
        detail.Items.Add(Underlined("sigApprovedLine", string.Empty, x: 0.0, y: y, w: 5.5, h: 0.6, size: 10));
        detail.Items.Add(Underlined("sigAccountantLine", string.Empty, x: 6.3, y: y, w: 5.4, h: 0.6, size: 10));
        detail.Items.Add(Text("sigNameLabel", "الاسم :", x: 15.6, y: y, w: 2.4, h: 0.6, size: 10, bold: false, align: HorizontalAlign.Right));
        detail.Items.Add(Underlined("sigNameLine", string.Empty, x: 12.5, y: y, w: 3.0, h: 0.6, size: 10));
        y += 1.0;
        detail.Items.Add(Text("sigSignLabel", "التوقيع :", x: 15.6, y: y, w: 2.4, h: 0.6, size: 10, bold: false, align: HorizontalAlign.Right));
        detail.Items.Add(Underlined("sigSignLine", string.Empty, x: 12.5, y: y, w: 3.0, h: 0.6, size: 10));
        y += 1.2;

        // ===== Reference =====
        detail.Items.Add(Text("reference", $"مرجع: {data.PaymentId}",
            x: 0.0, y: y, w: 8.0, h: 0.6, size: 8, bold: false, align: HorizontalAlign.Left));
    }

    // ---- item factory helpers -------------------------------------------------

    private static TextBox Text(string name, string value, double x, double y, double w, double h,
        double size, bool bold, HorizontalAlign align)
    {
        var tb = new TextBox
        {
            Name = name,
            Value = value ?? string.Empty,
            Location = new PointU(Unit.Cm(x), Unit.Cm(y)),
            Size = new SizeU(Unit.Cm(w), Unit.Cm(h)),
            CanGrow = true,
        };
        API.Reporting.ProjectReport.ProjectReportLayout.ApplyFont(tb.Style, bold);
        tb.Style.Font.Size = Unit.Point(size);
        tb.Style.TextAlign = API.Reporting.ProjectReport.ProjectReportLayout.Physical(align);
        tb.Style.VerticalAlign = VerticalAlign.Middle;
        return tb;
    }

    /// <summary>Right-aligned value sitting on a bottom border (the voucher's fill-in lines).</summary>
    private static TextBox Underlined(string name, string value, double x, double y, double w, double h, double size)
    {
        var tb = Text(name, value, x, y, w, h, size, bold: false, align: HorizontalAlign.Right);
        tb.Style.BorderStyle.Bottom = BorderType.Solid;
        tb.Style.BorderWidth.Bottom = Unit.Point(1);
        tb.Style.BorderColor.Bottom = System.Drawing.Color.Gray;
        return tb;
    }

    /// <summary>Centered value inside a full box (amount cells).</summary>
    private static TextBox Boxed(string name, string value, double x, double y, double w, double h, double size)
    {
        var tb = Text(name, value, x, y, w, h, size, bold: false, align: HorizontalAlign.Center);
        tb.Style.BorderStyle.Default = BorderType.Solid;
        tb.Style.BorderWidth.Default = Unit.Point(1);
        tb.Style.BorderColor.Default = System.Drawing.Color.Gray;
        return tb;
    }

    /// <summary>A [X] box followed by a caption, laid out RTL (caption on the right).</summary>
    private static Panel Checkbox(string name, string caption, bool ticked, double x, double y, double w)
    {
        var panel = new Panel
        {
            Name = name,
            Location = new PointU(Unit.Cm(x), Unit.Cm(y)),
            Size = new SizeU(Unit.Cm(w), Unit.Cm(0.9)),
        };

        var box = new TextBox
        {
            Name = name + "Box",
            Value = ticked ? "X" : string.Empty,
            Location = new PointU(Unit.Cm(0), Unit.Cm(0.05)),
            Size = new SizeU(Unit.Cm(0.6), Unit.Cm(0.6)),
        };
        box.Style.Font.Name = FontFamily;
        box.Style.Font.Size = Unit.Point(11);
        box.Style.TextAlign = HorizontalAlign.Center;
        box.Style.VerticalAlign = VerticalAlign.Middle;
        box.Style.BorderStyle.Default = BorderType.Solid;
        box.Style.BorderWidth.Default = Unit.Point(1);

        var label = new TextBox
        {
            Name = name + "Label",
            Value = caption,
            Location = new PointU(Unit.Cm(0.8), Unit.Cm(0)),
            Size = new SizeU(Unit.Cm(w - 0.8), Unit.Cm(0.9)),
        };
        label.Style.Font.Name = FontFamily;
        label.Style.Font.Size = Unit.Point(11);
        label.Style.TextAlign = API.Reporting.ProjectReport.ProjectReportLayout.Physical(HorizontalAlign.Left);
        label.Style.VerticalAlign = VerticalAlign.Middle;

        panel.Items.Add(box);
        panel.Items.Add(label);
        return panel;
    }
}
