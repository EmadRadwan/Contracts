using Application.Interfaces;
using Application.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Pdf
{
    public class PdfGenerationService : IPdfGenerationService
    {
        private const string ArabicFontFamily = "NotoSansArabic";
        private static bool _fontsRegistered = false;
        private static readonly object _fontLock = new object();

        public PdfGenerationService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            RegisterFonts();
        }

        private static void RegisterFonts()
        {
            lock (_fontLock)
            {
                if (_fontsRegistered) return;

                var fontsPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "fonts"); // Better than GetCurrentDirectory() in Docker

                var regularFont = Path.Combine(fontsPath, "NotoSansArabic-Regular.ttf");
                var boldFont = Path.Combine(fontsPath, "NotoSansArabic-Bold.ttf");

                if (File.Exists(regularFont))
                {
                    using var regularStream = File.OpenRead(regularFont);
                    QuestPDF.Drawing.FontManager.RegisterFontWithCustomName("NotoSansArabic", regularStream);
                }

                if (File.Exists(boldFont))
                {
                    using var boldStream = File.OpenRead(boldFont);
                    QuestPDF.Drawing.FontManager.RegisterFontWithCustomName("NotoSansArabic Bold", boldStream);
                }

                _fontsRegistered = true;
            }
        }
        public byte[] GeneratePaymentReportPdf(PaymentReportDto data, string companyName = "Golden Land")
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "goldenlandlogo.jpg");
            byte[]? logoBytes = null;
            if (File.Exists(logoPath))
            {
                logoBytes = File.ReadAllBytes(logoPath);
            }

            // Determine payment method type
            var paymentMethod = data.PaymentMethodDescription?.ToUpperInvariant() ?? "";
            bool isCash = paymentMethod.Contains("CASH") || paymentMethod.Contains("نقد");
            bool isCheque = paymentMethod.Contains("CHEQUE") || paymentMethod.Contains("CHECK") || paymentMethod.Contains("شيك");
            bool isBankTransfer = paymentMethod.Contains("BANK") || paymentMethod.Contains("TRANSFER") || paymentMethod.Contains("تحويل") || paymentMethod.Contains("بنك");

            // Get currency suffix
            string currencySuffix = GetCurrencySuffix(data.CurrencyUomId);
            string amountInWords = ConvertAmountToArabicWords(data.Amount, currencySuffix);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(15);
                    page.DefaultTextStyle(x => x.FontFamily("NotoSansArabic").FontSize(9));
                    page.Content().Column(mainCol =>
                    {
                        // ===== HEADER SECTION =====
                        mainCol.Item().Row(headerRow =>
                        {
                            // Left: Logo
                            headerRow.RelativeItem(2).AlignLeft().AlignMiddle().Column(logoCol =>
                            {
                                if (logoBytes != null)
                                {
                                    logoCol.Item().Width(60).Image(logoBytes).FitWidth();
                                }
                            });

                            // Center: Title
                            headerRow.RelativeItem(3).AlignCenter().AlignMiddle().Column(titleCol =>
                            {
                                titleCol.Item().AlignCenter().Text("إيصال صرف").FontSize(16).Bold().FontFamily(ArabicFontFamily);
                            });

                            // Right: Company Name in Arabic
                            headerRow.RelativeItem(2).AlignRight().AlignMiddle().Column(companyCol =>
                            {
                                companyCol.Item().AlignRight().Text("جولدن لاند").FontSize(12).Bold().FontFamily(ArabicFontFamily);
                                companyCol.Item().AlignRight().Text("للتطوير العقارى").FontSize(9).FontFamily(ArabicFontFamily);
                                companyCol.Item().AlignRight().Text("ش.م.م").FontSize(7).FontFamily(ArabicFontFamily);
                            });
                        });

                        mainCol.Item().PaddingVertical(3).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // ===== PAYMENT METHOD CHECKBOXES =====
                        mainCol.Item().PaddingTop(3).Row(methodRow =>
                        {
                            methodRow.RelativeItem().AlignCenter().Row(checkRow =>
                            {
                                // Bank Transfer checkbox
                                checkRow.AutoItem().PaddingHorizontal(8).Row(r =>
                                {
                                    r.AutoItem().Border(1).Width(10).Height(10).AlignCenter().AlignMiddle()
                                        .Text(isBankTransfer ? "X" : "").FontSize(8);
                                    r.AutoItem().PaddingLeft(2).Text("تحويل بنكى").FontSize(8).FontFamily(ArabicFontFamily);
                                });

                                // Cheque checkbox
                                checkRow.AutoItem().PaddingHorizontal(8).Row(r =>
                                {
                                    r.AutoItem().Border(1).Width(10).Height(10).AlignCenter().AlignMiddle()
                                        .Text(isCheque ? "X" : "").FontSize(8);
                                    r.AutoItem().PaddingLeft(2).Text("شيكات").FontSize(8).FontFamily(ArabicFontFamily);
                                });

                                // Cash checkbox
                                checkRow.AutoItem().PaddingHorizontal(8).Row(r =>
                                {
                                    r.AutoItem().Border(1).Width(10).Height(10).AlignCenter().AlignMiddle()
                                        .Text(isCash ? "X" : "").FontSize(8);
                                    r.AutoItem().PaddingLeft(2).Text("نقدية").FontSize(8).FontFamily(ArabicFontFamily);
                                });
                            });
                        });

                        // ===== DATE ROW =====
                        mainCol.Item().PaddingTop(6).Row(dateRow =>
                        {
                            // Amount boxes (RTL: [amount box] جنيه [piasters box] قرش)
                            dateRow.RelativeItem(3).AlignLeft().AlignMiddle().Row(r =>
                            {
                                // قرش label then piasters box (far left)
                                r.AutoItem().PaddingHorizontal(3).AlignMiddle().Text("قرش").FontSize(8).FontFamily(ArabicFontFamily);
                                r.AutoItem().PaddingHorizontal(3).Border(1).BorderColor(Colors.Grey.Medium)
                                    .Width(35).Height(14).AlignCenter().AlignMiddle()
                                    .Text(ToArabicNumerals(((int)((data.Amount - (int)data.Amount) * 100)).ToString("00"))).FontSize(8);
                                // جنيه label then amount box (right side)
                                r.AutoItem().PaddingHorizontal(3).AlignMiddle().Text("جنيه").FontSize(8).FontFamily(ArabicFontFamily);
                                r.AutoItem().PaddingHorizontal(3).Border(1).BorderColor(Colors.Grey.Medium)
                                    .Width(60).Height(14).AlignCenter().AlignMiddle()
                                    .Text(ToArabicNumerals(((int)data.Amount).ToString("N0"))).FontSize(8);
                            });

                            // Date on right
                            dateRow.RelativeItem(2).AlignRight().Row(r =>
                            {
                                r.AutoItem().Text(FormatArabicDate(data.EffectiveDate)).FontSize(9);
                                r.AutoItem().Text(": تحريراً فى").FontSize(9).FontFamily(ArabicFontFamily);
                            });
                        });

                        // ===== RECIPIENT ROW =====
                        mainCol.Item().PaddingTop(6).Row(r =>
                        {
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium);
                            r.AutoItem().AlignMiddle().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .Text(data.ToPartyName ?? "").FontSize(9).FontFamily(ArabicFontFamily);
                            r.AutoItem().AlignMiddle().Text(" : صرفنا إلى السيد / السادة").FontSize(9).FontFamily(ArabicFontFamily);
                        });

                        // ===== AMOUNT IN WORDS ROW =====
                        mainCol.Item().PaddingTop(6).Row(r =>
                        {
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium);
                            r.AutoItem().AlignMiddle().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .Text(amountInWords).FontSize(9).FontFamily(ArabicFontFamily);
                            r.AutoItem().AlignMiddle().Text(" : فقط وقدره").FontSize(9).FontFamily(ArabicFontFamily);
                        });

                        // ===== CHEQUE DETAILS ROW =====
                        mainCol.Item().PaddingTop(6).AlignRight().Row(r =>
                        {
                            // Date placeholder (show ٢٠  /  /  if no date)
                            var chequeDateText = data.ChequeDate.HasValue
                                ? FormatArabicDate(data.ChequeDate.Value)
                                : "٢٠    /    /    ";
                            r.AutoItem().PaddingHorizontal(2).Width(70)
                                .AlignCenter().AlignMiddle().Text(chequeDateText).FontSize(8);
                            r.AutoItem().AlignMiddle().Text(" حق").FontSize(8).FontFamily(ArabicFontFamily);

                            // Cheque number
                            r.AutoItem().PaddingHorizontal(2).Width(50).BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .AlignCenter().AlignMiddle().Text(ToArabicNumerals(data.ChequeNumber ?? "")).FontSize(8).FontFamily(ArabicFontFamily);
                            r.AutoItem().AlignMiddle().Text(" رقم").FontSize(8).FontFamily(ArabicFontFamily);

                            // Bank name
                            r.AutoItem().PaddingHorizontal(2).Width(80).BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .AlignCenter().AlignMiddle().Text("").FontSize(8);
                            r.AutoItem().AlignMiddle().Text(" مسحوب على بنك").FontSize(8).FontFamily(ArabicFontFamily);

                            // Payment type
                            r.AutoItem().PaddingHorizontal(2).Width(60).BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .AlignCenter().AlignMiddle().Text(isCash ? "نقداً" : (isCheque ? "شيك" : "")).FontSize(8).FontFamily(ArabicFontFamily);
                            r.AutoItem().AlignMiddle().Text(" : نقداً / بموجب").FontSize(8).FontFamily(ArabicFontFamily);
                        });

                        // ===== BANK TRANSFER ROW =====
                        mainCol.Item().PaddingTop(6).Row(r =>
                        {
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium);
                            r.AutoItem().AlignMiddle().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .Text(isBankTransfer ? (data.PaymentMethodDescription ?? "") : "").FontSize(8).FontFamily(ArabicFontFamily);
                            r.AutoItem().AlignMiddle().Text(" : تحويل ( بنكى ، اون لاين )").FontSize(8).FontFamily(ArabicFontFamily);
                        });

                        // ===== PURPOSE ROW =====
                        mainCol.Item().PaddingTop(6).Row(r =>
                        {
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium);
                            r.AutoItem().AlignMiddle().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .Text(data.Comments ?? "").FontSize(9).FontFamily(ArabicFontFamily);
                            r.AutoItem().AlignMiddle().Text(" : وذلك عن").FontSize(9).FontFamily(ArabicFontFamily);
                        });

                        // ===== SPACER =====
                        mainCol.Item().PaddingVertical(15);

                        // ===== SIGNATURE SECTION =====
                        mainCol.Item().Row(sigRow =>
                        {
                            // Left: Approved
                            sigRow.RelativeItem().AlignLeft().Column(c =>
                            {
                                c.Item().Text("...يعتمد").FontSize(9).FontFamily(ArabicFontFamily);
                                c.Item().PaddingTop(20).BorderBottom(1).Width(70);
                            });

                            // Center: Accountant
                            sigRow.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().AlignCenter().Text("المحاسب").FontSize(9).FontFamily(ArabicFontFamily);
                                c.Item().PaddingTop(20).AlignCenter().BorderBottom(1).Width(70);
                            });

                            // Right: Recipient
                            sigRow.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().AlignRight().Text("المستلم").FontSize(9).FontFamily(ArabicFontFamily);
                                c.Item().PaddingTop(8).AlignRight().Row(r =>
                                {
                                    r.AutoItem().BorderBottom(1).Width(80);
                                    r.AutoItem().PaddingLeft(3).Text("الاسم").FontSize(8).FontFamily(ArabicFontFamily);
                                });
                                c.Item().PaddingTop(8).AlignRight().Row(r =>
                                {
                                    r.AutoItem().BorderBottom(1).Width(80);
                                    r.AutoItem().PaddingLeft(3).Text("التوقيع").FontSize(8).FontFamily(ArabicFontFamily);
                                });
                            });
                        });

                        // ===== PAYMENT ID (small reference) =====
                        mainCol.Item().PaddingTop(5).AlignLeft().Text($"مرجع: {ToArabicNumerals(data.PaymentId)}").FontSize(6).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private string GetCurrencySuffix(string? currencyCode)
        {
            return currencyCode?.ToUpperInvariant() switch
            {
                "EGP" => "جنيه مصرى",
                "USD" => "دولار أمريكى",
                "EUR" => "يورو",
                "SAR" => "ريال سعودى",
                "AED" => "درهم إماراتى",
                _ => "جنيه"
            };
        }

        private string ConvertAmountToArabicWords(decimal amount, string currencySuffix)
        {
            var intPart = (long)amount;
            var decPart = (int)((amount - intPart) * 100);

            var result = ConvertNumberToArabicWords(intPart) + " " + currencySuffix;

            if (decPart > 0)
            {
                result += " و " + ConvertNumberToArabicWords(decPart) + " قرش";
            }

            result += " لا غير";
            return result;
        }

        private string ToArabicNumerals(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var arabicDigits = new[] { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };
            var result = new char[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]))
                    result[i] = arabicDigits[input[i] - '0'];
                else
                    result[i] = input[i];
            }

            return new string(result);
        }

        private string FormatArabicDate(DateTime date)
        {
            return ToArabicNumerals($"{date:yyyy/MM/dd}");
        }

        private string FormatArabicNumber(decimal number)
        {
            return ToArabicNumerals(((int)number).ToString("N0"));
        }

        private string ConvertNumberToArabicWords(long number)
        {
            if (number == 0) return "صفر";

            string[] ones = { "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة",
                             "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر",
                             "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر" };
            string[] tens = { "", "", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون" };
            string[] hundreds = { "", "مائة", "مائتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة", "تسعمائة" };

            if (number < 0) return "سالب " + ConvertNumberToArabicWords(-number);
            if (number < 20) return ones[number];
            if (number < 100)
            {
                var remainder = number % 10;
                var ten = number / 10;
                if (remainder == 0) return tens[ten];
                return ones[remainder] + " و " + tens[ten];
            }
            if (number < 1000)
            {
                var remainder = number % 100;
                var hundred = number / 100;
                if (remainder == 0) return hundreds[hundred];
                return hundreds[hundred] + " و " + ConvertNumberToArabicWords(remainder);
            }
            if (number < 1000000)
            {
                var thousands = number / 1000;
                var remainder = number % 1000;
                string thousandWord;
                if (thousands == 1) thousandWord = "ألف";
                else if (thousands == 2) thousandWord = "ألفان";
                else if (thousands >= 3 && thousands <= 10) thousandWord = ConvertNumberToArabicWords(thousands) + " آلاف";
                else thousandWord = ConvertNumberToArabicWords(thousands) + " ألف";

                if (remainder == 0) return thousandWord;
                return thousandWord + " و " + ConvertNumberToArabicWords(remainder);
            }
            if (number < 1000000000)
            {
                var millions = number / 1000000;
                var remainder = number % 1000000;
                string millionWord;
                if (millions == 1) millionWord = "مليون";
                else if (millions == 2) millionWord = "مليونان";
                else if (millions >= 3 && millions <= 10) millionWord = ConvertNumberToArabicWords(millions) + " ملايين";
                else millionWord = ConvertNumberToArabicWords(millions) + " مليون";

                if (remainder == 0) return millionWord;
                return millionWord + " و " + ConvertNumberToArabicWords(remainder);
            }

            // For billions
            var billions = number / 1000000000;
            var billionRemainder = number % 1000000000;
            string billionWord;
            if (billions == 1) billionWord = "مليار";
            else if (billions == 2) billionWord = "ملياران";
            else if (billions >= 3 && billions <= 10) billionWord = ConvertNumberToArabicWords(billions) + " مليارات";
            else billionWord = ConvertNumberToArabicWords(billions) + " مليار";

            if (billionRemainder == 0) return billionWord;
            return billionWord + " و " + ConvertNumberToArabicWords(billionRemainder);
        }
    }
}