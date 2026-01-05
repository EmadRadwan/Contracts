using Application.Interfaces;
using Application.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Pdf
{
    public class PdfGenerationService : IPdfGenerationService
    {
        private const string ArabicFontFamily = "Amiri-Regular";
        private const string ArabicBoldFontFamily = "Amiri-Bold"; // Separate for bold
        private static bool _fontsRegistered = false;
        private static readonly object _fontLock = new object();

        private readonly ILogger<PdfGenerationService> _logger;


        public PdfGenerationService(ILogger<PdfGenerationService> logger)
        {
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
            //QuestPDF.Settings.EnableDebugging = false;
            // Critical for production stability
            QuestPDF.Settings.UseEnvironmentFonts = false;
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false; // Prevent crash, show ??? if font missing

            RegisterFonts();
        }

        private static void RegisterFonts()
        {
            lock (_fontLock)
            {
                if (_fontsRegistered) return;

                var basePath = AppContext.BaseDirectory; // Most reliable in Docker
                var fontsPath = Path.Combine(basePath, "wwwroot", "fonts");

                Console.WriteLine($"[PDF Debug] Base path: {basePath}");
                Console.WriteLine($"[PDF Debug] Fonts folder path: {fontsPath}");
                Console.WriteLine($"[PDF Debug] Fonts folder exists: {Directory.Exists(fontsPath)}");

                if (Directory.Exists(fontsPath))
                {
                    var files = Directory.GetFiles(fontsPath);
                    Console.WriteLine(
                        $"[PDF Debug] Files in fonts folder: {string.Join(", ", files.Select(Path.GetFileName))}");
                }

                var regularFont = Path.Combine(fontsPath, "NotoSansArabic-Regular.ttf");
                var boldFont = Path.Combine(fontsPath, "NotoSansArabic-Bold.ttf");

                Console.WriteLine($"[PDF Debug] Regular font exists: {File.Exists(regularFont)}");
                Console.WriteLine($"[PDF Debug] Bold font exists: {File.Exists(boldFont)}");

                if (File.Exists(regularFont))
                {
                    try
                    {
                        using var regularStream = File.OpenRead(regularFont);
                        QuestPDF.Drawing.FontManager.RegisterFontWithCustomName(ArabicFontFamily, regularStream);
                        Console.WriteLine(
                            "[PDF Debug] Successfully registered NotoSansArabic-Regular with custom name 'NotoSansArabic'");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PDF Debug] FAILED to register regular font: {ex.Message}");
                        Console.WriteLine($"[PDF Debug] Stack: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine("[PDF Debug] Regular font file NOT FOUND!");
                }

                if (File.Exists(boldFont))
                {
                    try
                    {
                        using var boldStream = File.OpenRead(boldFont);
                        QuestPDF.Drawing.FontManager.RegisterFontWithCustomName(ArabicBoldFontFamily, boldStream);
                        Console.WriteLine(
                            "[PDF Debug] Successfully registered NotoSansArabic-Bold with custom name 'NotoSansArabic-Bold'");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PDF Debug] FAILED to register bold font: {ex.Message}");
                    }
                }

                _fontsRegistered = true;
                Console.WriteLine("[PDF Debug] Font registration completed.");
            }
        }

        public byte[] GeneratePaymentReportPdf(PaymentReportDto data, string companyName = "Golden Land")
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var logoPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "goldenlandlogo.jpg");
                byte[]? logoBytes = null;
                if (File.Exists(logoPath))
                {
                    logoBytes = File.ReadAllBytes(logoPath);
                }

                // Determine payment method type
                var paymentMethod = data.PaymentMethodDescription?.ToUpperInvariant() ?? "";
                bool isCash = paymentMethod.Contains("CASH") || paymentMethod.Contains("نقد");
                bool isCheque = paymentMethod.Contains("CHEQUE") || paymentMethod.Contains("CHECK") ||
                                paymentMethod.Contains("شيك");
                bool isBankTransfer = paymentMethod.Contains("BANK") || paymentMethod.Contains("TRANSFER") ||
                                      paymentMethod.Contains("تحويل") || paymentMethod.Contains("بنك");

                string currencySuffix = GetCurrencySuffix(data.CurrencyUomId);
                string amountInWords = ConvertAmountToArabicWords(data.Amount, currencySuffix);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A5.Landscape());
                        page.Margin(20); // Slightly increased for safety
                        page.DefaultTextStyle(x => x.FontFamily(ArabicFontFamily).FontSize(9));

                        page.Content().Column(mainCol =>
                        {
                            // ===== HEADER SECTION =====
                            mainCol.Item().Row(headerRow =>
                            {
                                // Left: Logo - no fixed height
                                headerRow.RelativeItem(2).AlignLeft().AlignMiddle().Element(block =>
                                {
                                    if (logoBytes != null)
                                    {
                                        // Limit max height to prevent overflow, but allow natural scaling
                                        block.Height(70).Image(logoBytes).FitArea();
                                    }
                                });

                                // Center: Title
                                headerRow.RelativeItem(4).AlignCenter().AlignMiddle().Text("إيصال صرف")
                                    .FontSize(18).Bold();

                                // Right: Company Name
                                headerRow.RelativeItem(3).AlignRight().AlignMiddle().Column(col =>
                                {
                                    col.Item().Text("جولدن لاند").FontSize(14).Bold();
                                    col.Item().Text("للتطوير العقارى").FontSize(10);
                                    col.Item().Text("ش.م.م").FontSize(8);
                                });
                            });
                            mainCol.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            // ===== PAYMENT METHOD CHECKBOXES (SAFE & CENTERED) =====
                            mainCol.Item().PaddingTop(20).Row(row =>
                            {
                                // Center the whole group with equal flexible space on sides
                                row.RelativeItem(1); // Left flexible spacer
                                row.RelativeItem(1).AlignCenter().Row(bankRow =>
                                {
                                    bankRow.AutoItem().Width(14).Height(14).Border(1).AlignCenter().AlignMiddle()
                                        .Text(isBankTransfer ? "X" : "").FontSize(11).Bold();
                                    bankRow.AutoItem().PaddingLeft(8).Text("تحويل بنكى").FontSize(10);
                                });

                                row.RelativeItem(1).AlignCenter().Row(chequeRow =>
                                {
                                    chequeRow.AutoItem().Width(14).Height(14).Border(1).AlignCenter().AlignMiddle()
                                        .Text(isCheque ? "X" : "").FontSize(11).Bold();
                                    chequeRow.AutoItem().PaddingLeft(8).Text("شيكات").FontSize(10);
                                });

                                row.RelativeItem(1).AlignCenter().Row(cashRow =>
                                {
                                    cashRow.AutoItem().Width(14).Height(14).Border(1).AlignCenter().AlignMiddle()
                                        .Text(isCash ? "X" : "").FontSize(11).Bold();
                                    cashRow.AutoItem().PaddingLeft(8).Text("نقدية").FontSize(10);
                                });

                                row.RelativeItem(1); // Right flexible spacer
                            });
                            // ===== DATE & AMOUNT BOXES =====
                            mainCol.Item().PaddingTop(12).Row(row =>
                            {
                                row.RelativeItem(3).AlignLeft().Row(amountRow =>
                                {
                                    amountRow.AutoItem().Text("قرش").FontSize(9);
                                    amountRow.AutoItem().PaddingHorizontal(4).Border(1).Width(40).Height(16)
                                        .AlignCenter()
                                        .Text(ToArabicNumerals(
                                            ((int)((data.Amount - (int)data.Amount) * 100)).ToString("00")));

                                    amountRow.AutoItem().PaddingLeft(10).Text("جنيه").FontSize(9);
                                    amountRow.AutoItem().PaddingHorizontal(4).Border(1).Width(80).Height(16)
                                        .AlignCenter().Text(ToArabicNumerals(((int)data.Amount).ToString("N0")));
                                });

                                row.RelativeItem(2).AlignRight()
                                    .Text($"{FormatArabicDate(data.EffectiveDate)} : تحريراً فى")
                                    .FontSize(10);
                            });

                            // ===== RECIPIENT =====
                            mainCol.Item().PaddingTop(12).Row(row =>
                            {
                                row.RelativeItem().BorderBottom(1).Text(data.ToPartyName ?? "").Bold();
                                row.AutoItem().Text(" : صرفنا إلى السيد / السادة");
                            });

                            // ===== AMOUNT IN WORDS =====
                            mainCol.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().BorderBottom(1).Text(amountInWords);
                                row.AutoItem().Text(" : فقط وقدره");
                            });

                            // ===== CHEQUE / PAYMENT DETAILS =====
                            mainCol.Item().PaddingTop(12).AlignRight().Row(row =>
                            {
                                var chequeDateText = data.ChequeDate.HasValue
                                    ? FormatArabicDate(data.ChequeDate.Value)
                                    : "٢٠    /    /    ";

                                row.AutoItem().Width(80).AlignCenter().Text(chequeDateText);
                                row.AutoItem().Text(" حق");

                                row.AutoItem().PaddingHorizontal(8).Width(60).BorderBottom(1).AlignCenter()
                                    .Text(ToArabicNumerals(data.ChequeNumber ?? ""));
                                row.AutoItem().Text(" رقم");

                                row.AutoItem().PaddingHorizontal(8).Width(100).BorderBottom(1).AlignCenter().Text("");
                                row.AutoItem().Text(" مسحوب على بنك");

                                row.AutoItem().PaddingHorizontal(8).Width(70).BorderBottom(1).AlignCenter()
                                    .Text(isCash ? "نقداً" : isCheque ? "شيك" : "");
                                row.AutoItem().Text(" : نقداً / بموجب");
                            });

                            // ===== BANK TRANSFER DETAILS =====
                            if (isBankTransfer)
                            {
                                mainCol.Item().PaddingTop(8).Row(row =>
                                {
                                    row.RelativeItem().BorderBottom(1).Text(data.PaymentMethodDescription ?? "");
                                    row.AutoItem().Text(" : تحويل ( بنكى ، اون لاين )");
                                });
                            }

                            // ===== PURPOSE =====
                            mainCol.Item().PaddingTop(12).Row(row =>
                            {
                                row.RelativeItem().BorderBottom(1).Text(data.Comments ?? "");
                                row.AutoItem().Text(" : وذلك عن");
                            });

                            mainCol.Item().PaddingVertical(20);

                            // ===== SIGNATURES =====
                            mainCol.Item().Row(sigRow =>
                            {
                                sigRow.RelativeItem().AlignLeft().Column(c =>
                                {
                                    c.Item().Text("...يعتمد");
                                    c.Item().PaddingTop(30).BorderBottom(1).Width(100);
                                });

                                sigRow.RelativeItem().AlignCenter().Column(c =>
                                {
                                    c.Item().Text("المحاسب");
                                    c.Item().PaddingTop(30).AlignCenter().BorderBottom(1).Width(100);
                                });

                                sigRow.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text("المستلم");
                                    c.Item().PaddingTop(15).AlignRight().Row(r =>
                                    {
                                        r.AutoItem().BorderBottom(1).Width(120);
                                        r.AutoItem().PaddingLeft(8).Text("الاسم");
                                    });
                                    c.Item().PaddingTop(10).AlignRight().Row(r =>
                                    {
                                        r.AutoItem().BorderBottom(1).Width(120);
                                        r.AutoItem().PaddingLeft(8).Text("التوقيع");
                                    });
                                });
                            });

                            // ===== REFERENCE =====
                            mainCol.Item().PaddingTop(10).AlignLeft()
                                .Text($"مرجع: {ToArabicNumerals(data.PaymentId)}")
                                .FontSize(7).FontColor(Colors.Grey.Medium);
                        });
                    });
                });

                Console.WriteLine("[PDF Debug] Document composed successfully. Generating PDF bytes...");
                var pdfBytes = document.GeneratePdf();
                Console.WriteLine($"[PDF Debug] PDF generated successfully! Size: {pdfBytes.Length} bytes");

                return pdfBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PDF Debug] EXCEPTION: {ex.Message}");
                Console.WriteLine($"[PDF Debug] Stack: {ex.StackTrace}");
                throw;
            }
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

            string[] ones =
            {
                "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة",
                "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر",
                "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر"
            };
            string[] tens = { "", "", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون" };
            string[] hundreds =
                { "", "مائة", "مائتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة", "تسعمائة" };

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
                else if (thousands >= 3 && thousands <= 10)
                    thousandWord = ConvertNumberToArabicWords(thousands) + " آلاف";
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
                else if (millions >= 3 && millions <= 10)
                    millionWord = ConvertNumberToArabicWords(millions) + " ملايين";
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