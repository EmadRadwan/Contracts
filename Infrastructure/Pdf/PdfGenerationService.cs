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
            QuestPDF.Settings.License = LicenseType.Community;
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
            var paymentMethodId = data.PaymentMethodId?.ToUpperInvariant() ?? "";
            var paymentMethod = data.PaymentMethodDescription?.ToUpperInvariant() ?? "";
            bool isCash = paymentMethodId.Contains("CASH") || paymentMethod.Contains("نقد");
            bool isCheque = !isCash;
            bool isBankTransfer = false;
            // Get currency suffix
            string currencySuffix = GetCurrencySuffix(data.CurrencyUomId);
            string amountInWords = ConvertAmountToArabicWords(data.Amount, currencySuffix);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Portrait());
                    page.Margin(15);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato", "Noto Sans Arabic"));

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
                                    logoCol.Item().MaxWidth(40).Image(logoBytes).FitWidth();
                                }
                            });

                            // Center: Title
                            // Center: Title
                            headerRow.RelativeItem(3).AlignCenter().AlignMiddle().Column(titleCol =>
                            {
                                string titleText;
                                if (data.PaymentParentTypeDescription?.Trim().ToUpperInvariant() == "RECEIPT")
                                {
                                    titleText = "إيصال قبض";
                                }
                                else
                                {
                                    titleText = "إيصال صرف";
                                }

                                titleCol.Item().AlignCenter().Text(titleText).FontSize(16).Bold().FontFamily("Lato", "Noto Sans Arabic");
                                titleCol.Item().AlignCenter().Text(data.PaymentId).FontSize(16).Bold();
                            });

                            // Right: Company Name in Arabic
                            headerRow.RelativeItem(2).AlignRight().AlignMiddle().Column(companyCol =>
                            {
                                companyCol.Item().AlignRight().Text("جولدن لاند").FontSize(12).Bold().FontFamily("Lato", "Noto Sans Arabic");
                                companyCol.Item().AlignRight().Text("للتطوير العقارى").FontSize(9).FontFamily("Lato", "Noto Sans Arabic");
                                companyCol.Item().AlignRight().Text("ش.م.م").FontSize(7).FontFamily("Lato", "Noto Sans Arabic");
                            });
                        });

                        mainCol.Item().PaddingVertical(3).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // ===== PAYMENT METHOD CHECKBOXES =====
                        mainCol.Item().PaddingTop(3).AlignCenter().Row(checkRow =>
                        {
                            // Bank Transfer checkbox
                            checkRow.AutoItem().PaddingHorizontal(4).Row(r =>
                            {
                                r.AutoItem().Border(1).Width(9).Height(9).AlignCenter().AlignMiddle()
                                    .Text(isBankTransfer ? "X" : "").FontSize(7);
                                r.AutoItem().PaddingLeft(2).Text("تحويل بنكى").FontSize(7).FontFamily("Lato", "Noto Sans Arabic");
                            });

                            // Cheque checkbox
                            checkRow.AutoItem().PaddingHorizontal(4).Row(r =>
                            {
                                r.AutoItem().Border(1).Width(9).Height(9).AlignCenter().AlignMiddle()
                                    .Text(isCheque ? "X" : "").FontSize(7);
                                r.AutoItem().PaddingLeft(2).Text("شيكات").FontSize(7).FontFamily("Lato", "Noto Sans Arabic");
                            });

                            // Cash checkbox
                            checkRow.AutoItem().PaddingHorizontal(4).Row(r =>
                            {
                                r.AutoItem().Border(1).Width(9).Height(9).AlignCenter().AlignMiddle()
                                    .Text(isCash ? "X" : "").FontSize(7);
                                r.AutoItem().PaddingLeft(2).Text("نقدية").FontSize(7).FontFamily("Lato", "Noto Sans Arabic");
                            });
                        });

                        // ===== DATE ROW =====
                        mainCol.Item().PaddingTop(6).AlignRight().Row(r =>
                        {
                            r.AutoItem().Text(FormatArabicDate(data.EffectiveDate)).FontSize(9);
                            r.AutoItem().Text(": تحريراً فى").FontSize(9).FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== AMOUNT ROW =====
                        mainCol.Item().PaddingTop(6).AlignCenter().Row(r =>
                        {
                            // Amount boxes (RTL: [amount box] جنيه [piasters box] قرش)
                            r.AutoItem().AlignMiddle().Text("قرش").FontSize(8).FontFamily("Lato", "Noto Sans Arabic");
                            r.AutoItem().PaddingHorizontal(4).Border(1).BorderColor(Colors.Grey.Medium).Padding(2)
                                .Text(ToArabicNumerals(((int)((data.Amount - (int)data.Amount) * 100)).ToString("00"))).FontSize(8);
                            r.AutoItem().PaddingLeft(8).AlignMiddle().Text("جنيه").FontSize(8).FontFamily("Lato", "Noto Sans Arabic");
                            r.AutoItem().PaddingHorizontal(4).Border(1).BorderColor(Colors.Grey.Medium).Padding(2)
                                .Text(ToArabicNumerals(((int)data.Amount).ToString("N0"))).FontSize(8);
                        });

                        // ===== RECIPIENT ROW =====
                        mainCol.Item().PaddingTop(6).Row(r =>
                        {
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium);
                            r.AutoItem().AlignMiddle().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .Text(data.ToPartyName ?? "").FontSize(9).FontFamily("Lato", "Noto Sans Arabic");
                            r.AutoItem().AlignMiddle().Text(" : صرفنا إلى السيد / السادة").FontSize(9).FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== AMOUNT IN WORDS ROW =====
                        mainCol.Item().PaddingTop(6).Row(r =>
                        {
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium);
                            r.AutoItem().AlignMiddle().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .Text(amountInWords).FontSize(9).FontFamily("Lato", "Noto Sans Arabic");
                            r.AutoItem().AlignMiddle().Text(" : فقط وقدره").FontSize(9).FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== CHEQUE DETAILS ROW 1 =====
                        mainCol.Item().PaddingTop(6).Row(r =>
                        {
                            // Payment type
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .AlignCenter().AlignMiddle().Text(isCash ? "نقداً" : (isCheque ? "شيك" : "")).FontSize(8).FontFamily("Lato", "Noto Sans Arabic");
                            r.AutoItem().AlignMiddle().Text(" : نقداً / بموجب").FontSize(8).FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== CHEQUE DETAILS ROW 2 (Bank) =====
                        mainCol.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium);
                            r.AutoItem().AlignMiddle().Text(" مسحوب على بنك").FontSize(8).FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== CHEQUE DETAILS ROW 3 (Number & Date) =====
                        mainCol.Item().PaddingTop(4).AlignRight().Row(r =>
                        {
                            // Date placeholder
                            var chequeDateText = data.ChequeDate.HasValue
                                ? FormatArabicDate(data.ChequeDate.Value)
                                : "٢٠    /    /    ";
                            r.AutoItem().Text(chequeDateText).FontSize(8);
                            r.AutoItem().Text(" حق  ").FontSize(8).FontFamily("Lato", "Noto Sans Arabic");

                            // Cheque number
                            r.AutoItem().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingHorizontal(10)
                                .Text(ToArabicNumerals(data.ChequeNumber ?? "")).FontSize(8).FontFamily("Lato", "Noto Sans Arabic");
                            r.AutoItem().Text(" رقم").FontSize(8).FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== BANK TRANSFER ROW =====
                        mainCol.Item().PaddingTop(6).Row(r =>
                        {
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium);
                            r.AutoItem().AlignMiddle().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .Text(isCheque ? (data.PaymentMethodDescription ?? "") : "").FontSize(8).FontFamily("Lato", "Noto Sans Arabic");
                            r.AutoItem().AlignMiddle().Text(" : تحويل ( بنكى ، اون لاين )").FontSize(8).FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== PURPOSE ROW =====
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

                        // ===== SPACER =====
                        mainCol.Item().PaddingVertical(15);

                        // ===== SIGNATURE SECTION =====
                        mainCol.Item().Row(sigRow =>
                        {
                            // Left: Approved
                            sigRow.RelativeItem().AlignLeft().Column(c =>
                            {
                                c.Item().Text("...يعتمد").FontSize(9).FontFamily("Lato", "Noto Sans Arabic");
                                c.Item().PaddingTop(20).BorderBottom(1);
                            });

                            // Center: Accountant
                            sigRow.RelativeItem().PaddingHorizontal(10).AlignCenter().Column(c =>
                            {
                                c.Item().AlignCenter().Text("المحاسب").FontSize(9).FontFamily("Lato", "Noto Sans Arabic");
                                c.Item().PaddingTop(20).AlignCenter().BorderBottom(1);
                            });

                            // Right: Recipient
                            sigRow.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().AlignRight().Text("المستلم").FontSize(9).FontFamily("Lato", "Noto Sans Arabic");
                                c.Item().PaddingTop(8).Row(r =>
                                {
                                    r.RelativeItem().BorderBottom(1);
                                    r.AutoItem().PaddingLeft(3).Text("الاسم").FontSize(8).FontFamily("Lato", "Noto Sans Arabic");
                                });
                                c.Item().PaddingTop(8).Row(r =>
                                {
                                    r.RelativeItem().BorderBottom(1);
                                    r.AutoItem().PaddingLeft(3).Text("التوقيع").FontSize(8).FontFamily("Lato", "Noto Sans Arabic");
                                });
                            });
                        });

                        // ===== PAYMENT ID (small reference) =====
                        mainCol.Item().PaddingTop(5).AlignLeft().Text($"مرجع: {data.PaymentId}").FontSize(6).FontColor(Colors.Grey.Medium);
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