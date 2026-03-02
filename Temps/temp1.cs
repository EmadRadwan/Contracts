// ===== RECIPIENT ROW =====
mainCol.Item().PaddingTop(6).Row(r =>
{
    bool isReceipt = string.Equals(data.PaymentParentTypeDescription?.Trim(), 
        "RECEIPT", 
        StringComparison.OrdinalIgnoreCase);

    string labelBeforeName;
    string labelAfterName;
    bool nameHasBottomBorder;

    if (isReceipt)
    {
        labelBeforeName    = "استلمنا نحن / جولدن لاند للتطوير العقاري";
        labelAfterName     = "من السيد / السادة";
        nameHasBottomBorder = false;   // name usually doesn't get underline in receipts
    }
    else
    {
        labelBeforeName    = "صرفنا إلى السيد / السادة";
        labelAfterName     = "";
        nameHasBottomBorder = true;
    }

    // Name field (with or without bottom border)
    var nameCell = r.AutoItem().AlignMiddle();
    if (nameHasBottomBorder)
        nameCell = nameCell.BorderBottom(1).BorderColor(Colors.Grey.Medium);
    
    nameCell.Text(data.ToPartyName ?? "").FontSize(9).FontFamily("Lato", "Noto Sans Arabic");

    // Label after name (usually empty in payment mode)
    if (!string.IsNullOrEmpty(labelAfterName))
    {
        r.AutoItem().AlignMiddle().Text(labelAfterName)
            .FontSize(9).FontFamily("Lato", "Noto Sans Arabic");
    }

    // Main label (left side in RTL)
    r.AutoItem().AlignMiddle().Text($" : {labelBeforeName}")
        .FontSize(9).FontFamily("Lato", "Noto Sans Arabic");
});