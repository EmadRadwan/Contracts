// ... existing code ...
// ===== PURPOSE (COMMENTS) SECTION =====
// Use a Column (not a single Row) so long Arabic text can wrap naturally on A5.
mainCol.Item().PaddingTop(6).Column(purposeCol =>
{
    purposeCol.Item().AlignRight()
        .Text("وذلك عن :")
        .FontSize(9).FontFamily("Lato", "Noto Sans Arabic");

    purposeCol.Item().PaddingTop(2)
        .BorderBottom(1).BorderColor(Colors.Grey.Medium)
        .PaddingBottom(2)
        .Text(data.Comments ?? "")
        .FontSize(9).FontFamily("Lato", "Noto Sans Arabic")
        .WrapAnywhere();
});
// ... existing code ...