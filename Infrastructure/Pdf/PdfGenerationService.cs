using Application.Interfaces;
using Application.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Pdf 
{
    public class PdfGenerationService : IPdfGenerationService
    {
        public PdfGenerationService()
        {
            QuestPDF.Settings.License = LicenseType.Community; // Can be set once here
        }

        public byte[] GeneratePaymentReportPdf(PaymentReportDto data, string companyName = "Golden Land")
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var backgroundPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "goldenland_voucher_template.jpg");
    
            if (!File.Exists(backgroundPath))
            {
                throw new FileNotFoundException($"Background template not found at: {backgroundPath}");
            }
    
            var backgroundBytes = File.ReadAllBytes(backgroundPath);
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Background().Image(backgroundBytes).FitArea();
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Amiri"));

                    // Optional: fallback if Amiri not loaded
                    page.DefaultTextStyle(x => x.Fallback(f => f.FontFamily("DejaVu Sans")));

                    // Logo
                    page.Header()
                        .Height(90)
                        .AlignLeft()
                        .AlignMiddle()
                        .Image("wwwroot/goldenlandlogo.jpg")
                        .FitArea();

                    page.Content().PaddingTop(30).Column(col =>
                    {
                        col.Spacing(15);

                        // Title
                        col.Item().Text(Rtl("بيان دفعة"))
                            .FontSize(20).Bold().AlignCenter();

                        // Payment ID + Status
                        col.Item().Row(row =>
                        {
                            row.RelativeItem(4).Text(Rtl(data.PaymentId)).FontSize(16).Bold();
                            row.RelativeItem(5)
                                .Background(Colors.Yellow.Lighten2)
                                .Padding(10)
                                .AlignCenter()
                                .Text(Rtl(data.StatusDescription))
                                .FontSize(14).Bold();
                        });

                        col.Item().PaddingVertical(10);

                        // Parties
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.ConstantColumn(20);
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                            });

                            t.Cell().ColumnSpan(2).Text(Rtl("من")).Bold();
                            t.Cell().ColumnSpan(3).Text(Rtl(data.FromPartyName)).AlignRight();

                            t.Cell().Text("");
                            t.Cell().ColumnSpan(2).Text(Rtl("إلى")).Bold();
                            t.Cell().ColumnSpan(3).Text(Rtl(data.ToPartyName)).AlignRight();
                        });

                        // Type & Method
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.ConstantColumn(20);
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                            });

                            t.Cell().ColumnSpan(2).Text(Rtl("نوع الدفعة")).Bold();
                            t.Cell().ColumnSpan(3).Text(Rtl(data.PaymentTypeDescription)).AlignRight();

                            t.Cell().Text("");
                            t.Cell().ColumnSpan(2).Text(Rtl("طريقة الدفع")).Bold();
                            t.Cell().ColumnSpan(3).Text(Rtl(data.PaymentMethodDescription)).AlignRight();
                        });

                        // Cheque
                        if (!string.IsNullOrEmpty(data.ChequeNumber))
                        {
                            col.Item().Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(3);
                                    c.ConstantColumn(20);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(3);
                                });

                                t.Cell().ColumnSpan(2).Text(Rtl("رقم الشيك")).Bold();
                                t.Cell().ColumnSpan(3).Text(Rtl(data.ChequeNumber)).AlignRight();

                                t.Cell().Text("");
                                t.Cell().ColumnSpan(2).Text(Rtl("تاريخ الشيك")).Bold();
                                t.Cell().ColumnSpan(3).Text(data.ChequeDate?.ToString("dd/MM/yyyy") ?? "").AlignRight();
                            });
                        }

                        // Amount & Currency
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.ConstantColumn(20);
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                            });

                            t.Cell().ColumnSpan(2).Text(Rtl("المبلغ")).Bold();
                            t.Cell().ColumnSpan(3)
                                .Text(data.Amount.ToString("N2", new System.Globalization.CultureInfo("ar-EG")))
                                .FontSize(14).Bold().AlignRight();

                            t.Cell().Text("");
                            t.Cell().ColumnSpan(2).Text(Rtl("العملة")).Bold();
                            t.Cell().ColumnSpan(3).Text(Rtl(data.CurrencyUomId)).AlignRight();
                        });

                        // Cost Center & Project
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.ConstantColumn(20);
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                            });

                            t.Cell().ColumnSpan(2).Text(Rtl("مركز التكلفة")).Bold();
                            t.Cell().ColumnSpan(3).Text(Rtl(data.CostCenterDescription ?? "غير محدد")).AlignRight();

                            t.Cell().Text("");
                            t.Cell().ColumnSpan(2).Text(Rtl("المشروع")).Bold();
                            t.Cell().ColumnSpan(3).Text(Rtl(data.ProjectName ?? "غير محدد")).AlignRight();
                        });

                        // Effective Date
                        col.Item().Row(row =>
                        {
                            row.RelativeItem(2).Text(Rtl("تاريخ السريان")).Bold();
                            row.RelativeItem(3).Text(data.EffectiveDate.ToString("dd/MM/yyyy")).AlignRight();
                        });

                        // Comments
                        if (!string.IsNullOrEmpty(data.Comments))
                        {
                            col.Item().PaddingTop(20);
                            col.Item().Text(Rtl("البيان")).Bold();
                            col.Item()
                                .Background(Colors.Grey.Lighten3)
                                .Padding(10)
                                .Text(Rtl(data.Comments))
                                .FontSize(12)
                                .LineHeight((float?)1.5);
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return document.GeneratePdf();
        }

        // Helper methods inside the class
        private string GetLabel(bool isArabic, string ar, string en) => isArabic ? ar : en;

        private string FormatNumber(decimal? amount) =>
            amount?.ToString("N2", new System.Globalization.CultureInfo("ar-EG")) ?? "0.00";

        private string FormatDate(string? dateStr, bool isArabic)
        {
            if (string.IsNullOrEmpty(dateStr)) return "غير محدد";
            if (DateTime.TryParse(dateStr, out var date))
                return date.ToString(isArabic ? "dd/MM/yyyy" : "yyyy-MM-dd");
            return dateStr;
        }

        private string Rtl(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // \u202B = Right-to-Left Embedding (strong RTL direction)
            // \u202C = Pop Directional Formatting (ends the embedding)
            return $"\u202B{text}\u202C";
        }
    }
}