// For piasters box (35 was too small anyway for some values)
r.AutoItem().PaddingHorizontal(3).Border(1).BorderColor(Colors.Grey.Medium)
    .MinWidth(30).MaxWidth(80)  // optional: give min/max instead of fixed
    .Height(14).AlignCenter().AlignMiddle()
    .Text(ToArabicNumerals(...));

// For amount box
r.AutoItem().PaddingHorizontal(3).Border(1).BorderColor(Colors.Grey.Medium)
    .MinWidth(50).MaxWidth(150)  // adjust as needed
    .Height(14).AlignCenter().AlignMiddle()
    .Text(ToArabicNumerals(((int)data.Amount).ToString("N0")));