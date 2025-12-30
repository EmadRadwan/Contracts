// Infrastructure/Services/PdfGenerationService.cs (partial update)
public byte[] GeneratePaymentReportPdf(PaymentReportDto data, string companyName = "Golden Land")
{
    QuestPDF.Settings.License = LicenseType.Community;

    var document = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Amiri"));
            page.Content().DirectionRightToLeft();

            page.Header().Height(90).AlignLeft().AlignMiddle()
                .Image("wwwroot/goldenlandlogo.jpg").FitArea();

            page.Content().PaddingTop(30).Column(col =>
            {
                col.Spacing(15);

                col.Item().Text("بيان دفعة").FontSize(20).Bold().AlignCenter();

                col.Item().Row(row =>
                {
                    row.RelativeItem(4).Text(data.PaymentId).FontSize(16).Bold();
                    row.RelativeItem(5)
                        .Background(Colors.Yellow.Lighten2)
                        .PaddingVertical(8)
                        .AlignCenter()
                        .Text(data.StatusDescription)
                        .FontSize(14).Bold();
                });

                col.Item().PaddingVertical(10);

                // Parties
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(3); c.ConstantColumn(20); c.RelativeColumn(2); c.RelativeColumn(3); });

                    t.Cell().ColumnSpan(2).Text("من").Bold();
                    t.Cell().ColumnSpan(3).Text(data.FromPartyName).AlignRight();

                    t.Cell().Text("");
                    t.Cell().ColumnSpan(2).Text("إلى").Bold();
                    t.Cell().ColumnSpan(3).Text(data.ToPartyName).AlignRight();
                });

                // Type & Method
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(3); c.ConstantColumn(20); c.RelativeColumn(2); c.RelativeColumn(3); });

                    t.Cell().ColumnSpan(2).Text("نوع الدفعة").Bold();
                    t.Cell().ColumnSpan(3).Text(data.PaymentTypeDescription).AlignRight();

                    t.Cell().Text("");
                    t.Cell().ColumnSpan(2).Text("طريقة الدفع").Bold();
                    t.Cell().ColumnSpan(3).Text(data.PaymentMethodDescription).AlignRight();
                });

                // Cheque
                if (!string.IsNullOrEmpty(data.ChequeNumber))
                {
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(3); c.ConstantColumn(20); c.RelativeColumn(2); c.RelativeColumn(3); });

                        t.Cell().ColumnSpan(2).Text("رقم الشيك").Bold();
                        t.Cell().ColumnSpan(3).Text(data.ChequeNumber).AlignRight();

                        t.Cell().Text("");
                        t.Cell().ColumnSpan(2).Text("تاريخ الشيك").Bold();
                        t.Cell().ColumnSpan(3).Text(data.ChequeDate?.ToString("dd/MM/yyyy") ?? "").AlignRight();
                    });
                }

                // Amount & Currency
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(3); c.ConstantColumn(20); c.RelativeColumn(2); c.RelativeColumn(3); });

                    t.Cell().ColumnSpan(2).Text("المبلغ").Bold();
                    t.Cell().ColumnSpan(3).Text(data.Amount.ToString("N2", new System.Globalization.CultureInfo("ar-EG")))
                        .FontSize(14).Bold().AlignRight();

                    t.Cell().Text("");
                    t.Cell().ColumnSpan(2).Text("العملة").Bold();
                    t.Cell().ColumnSpan(3).Text(data.CurrencyUomId).AlignRight();
                });

                // Cost Center & Project
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(3); c.ConstantColumn(20); c.RelativeColumn(2); c.RelativeColumn(3); });

                    t.Cell().ColumnSpan(2).Text("مركز التكلفة").Bold();
                    t.Cell().ColumnSpan(3).Text(data.CostCenterDescription).AlignRight();

                    t.Cell().Text("");
                    t.Cell().ColumnSpan(2).Text("المشروع").Bold();
                    t.Cell().ColumnSpan(3).Text(data.ProjectName ?? "غير محدد").AlignRight();
                });

                // Effective Date
                col.Item().Row(row =>
                {
                    row.RelativeItem(2).Text("تاريخ السريان").Bold();
                    row.RelativeItem(3).Text(data.EffectiveDate.ToString("dd/MM/yyyy")).AlignRight();
                });

                // Comments
                if (!string.IsNullOrEmpty(data.Comments))
                {
                    col.Item().PaddingTop(20);
                    col.Item().Text("البيان").Bold();
                    col.Item()
                        .Background(Colors.Grey.Lighten3)
                        .Padding(10)
                        .Text(data.Comments)
                        .FontSize(12)
                        .LineHeight(1.5);
                }
            });

            page.Footer().AlignCenter().Text(x => { x.Span("Page "); x.CurrentPageNumber(); });
        });
    });

    return document.GeneratePdf();
}