namespace Application.Reports;

/// <summary>
/// Arabic formatting helpers for the payment voucher (بيان دفعة): amount-to-words,
/// Western→Arabic-Indic digits, currency suffix and date formatting.
///
/// </summary>
public static class ArabicPaymentFormatter
{
    private static readonly char[] ArabicDigits =
        { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };

    public static string CurrencySuffix(string? currencyCode) =>
        currencyCode?.ToUpperInvariant() switch
        {
            "EGP" => "جنيه مصرى",
            "USD" => "دولار أمريكى",
            "EUR" => "يورو",
            "SAR" => "ريال سعودى",
            "AED" => "درهم إماراتى",
            _ => "جنيه"
        };

    public static string ToArabicNumerals(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input ?? string.Empty;

        var result = new char[input.Length];
        for (var i = 0; i < input.Length; i++)
            result[i] = char.IsDigit(input[i]) ? ArabicDigits[input[i] - '0'] : input[i];

        return new string(result);
    }

    /// <summary>Formats a date as ٢٠٢٦/٠٩/١٠, or an empty stencil when null.</summary>
    public static string FormatArabicDate(DateOnly? date) =>
        date.HasValue
            ? ToArabicNumerals(date.Value.ToString("yyyy/MM/dd"))
            : "٢٠    /    /    ";

    /// <summary>"مائتان وخمسون ألف جنيه مصرى لا غير" style full-amount phrase.</summary>
    public static string AmountToWords(decimal amount, string currencySuffix)
    {
        var intPart = (long)amount;
        var decPart = (int)Math.Round((amount - intPart) * 100m);

        var result = NumberToWords(intPart) + " " + currencySuffix;
        if (decPart > 0)
            result += " و " + NumberToWords(decPart) + " قرش";

        result += " لا غير";
        return result;
    }

    public static string NumberToWords(long number)
    {
        if (number == 0) return "صفر";
        if (number < 0) return "سالب " + NumberToWords(-number);

        string[] ones =
        {
            "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة",
            "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر",
            "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر"
        };
        string[] tens = { "", "", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون" };
        string[] hundreds =
            { "", "مائة", "مائتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة", "تسعمائة" };

        if (number < 20) return ones[number];

        if (number < 100)
        {
            var remainder = number % 10;
            var ten = number / 10;
            return remainder == 0 ? tens[ten] : ones[remainder] + " و " + tens[ten];
        }

        if (number < 1000)
        {
            var remainder = number % 100;
            var hundred = number / 100;
            return remainder == 0 ? hundreds[hundred] : hundreds[hundred] + " و " + NumberToWords(remainder);
        }

        if (number < 1_000_000)
        {
            var thousands = number / 1000;
            var remainder = number % 1000;
            string thousandWord = thousands switch
            {
                1 => "ألف",
                2 => "ألفان",
                >= 3 and <= 10 => NumberToWords(thousands) + " آلاف",
                _ => NumberToWords(thousands) + " ألف"
            };
            return remainder == 0 ? thousandWord : thousandWord + " و " + NumberToWords(remainder);
        }

        if (number < 1_000_000_000)
        {
            var millions = number / 1_000_000;
            var remainder = number % 1_000_000;
            string millionWord = millions switch
            {
                1 => "مليون",
                2 => "مليونان",
                >= 3 and <= 10 => NumberToWords(millions) + " ملايين",
                _ => NumberToWords(millions) + " مليون"
            };
            return remainder == 0 ? millionWord : millionWord + " و " + NumberToWords(remainder);
        }

        var billions = number / 1_000_000_000;
        var billionRemainder = number % 1_000_000_000;
        string billionWord = billions switch
        {
            1 => "مليار",
            2 => "ملياران",
            >= 3 and <= 10 => NumberToWords(billions) + " مليارات",
            _ => NumberToWords(billions) + " مليار"
        };
        return billionRemainder == 0 ? billionWord : billionWord + " و " + NumberToWords(billionRemainder);
    }
}
