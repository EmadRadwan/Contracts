container.Page(page =>
{
    page.Size(PageSizes.A4);

    // Set the scanned form as background
    page.Background().Image("wwwroot/goldenland_voucher_template.jpg");
    // Or use a byte array if you load it dynamically:
    // page.Background().Image(backgroundImageBytes);

    page.Margin(40); // Adjust if needed to match the form's printable area

    // Keep your existing header, content, and footer
    page.Header()
        .Height(90)
        .AlignLeft()
        .AlignMiddle()
        .Image("wwwroot/goldenlandlogo.jpg")
        .FitArea();

    page.Content()
        .PaddingTop(30) // Fine-tune this to align text with form fields
        .Column(col =>
        {
            // ... all your existing content code remains the same
        });

    page.Footer()
        .AlignCenter()
        .Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
        });
});