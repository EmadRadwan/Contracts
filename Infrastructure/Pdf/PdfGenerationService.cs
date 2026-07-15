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
            bool isBankTransfer = !isCash && data.IsBankTransfer == true;
            bool isCheque = !isCash && !isBankTransfer;
            // Get currency suffix
            string currencySuffix = GetCurrencySuffix(data.CurrencyUomId);
            string amountInWords = ConvertAmountToArabicWords(data.Amount, currencySuffix);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Portrait());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(14).FontFamily("Lato", "Noto Sans Arabic"));

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
                                    logoCol.Item().MaxWidth(70).Image(logoBytes).FitWidth();
                                }
                            });

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

                                titleCol.Item().AlignCenter().Text(titleText).FontSize(26).Bold()
                                    .FontFamily("Lato", "Noto Sans Arabic");
                                titleCol.Item().AlignCenter().Text(data.PaymentId).FontSize(26).Bold();
                            });

                            // Right: Company Name in Arabic
                            headerRow.RelativeItem(2).AlignRight().AlignMiddle().Column(companyCol =>
                            {
                                companyCol.Item().AlignRight().Text("جولدن لاند").FontSize(19).Bold()
                                    .FontFamily("Lato", "Noto Sans Arabic");
                                companyCol.Item().AlignRight().Text("للتطوير العقارى").FontSize(14)
                                    .FontFamily("Lato", "Noto Sans Arabic");
                                companyCol.Item().AlignRight().Text("ش.م.م").FontSize(11)
                                    .FontFamily("Lato", "Noto Sans Arabic");
                            });
                        });

                        mainCol.Item().PaddingVertical(6).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // ===== PAYMENT METHOD CHECKBOXES =====
                        mainCol.Item().PaddingTop(6).AlignCenter().Row(checkRow =>
                        {
                            // Bank Transfer checkbox
                            checkRow.AutoItem().PaddingHorizontal(7).Row(r =>
                            {
                                r.AutoItem().Border(1).Width(14).Height(14).AlignCenter().AlignMiddle()
                                    .Text(isBankTransfer ? "X" : "").FontSize(11);
                                r.AutoItem().PaddingLeft(4).Text("تحويل بنكى").FontSize(11)
                                    .FontFamily("Lato", "Noto Sans Arabic");
                            });

                            // Cheque checkbox
                            checkRow.AutoItem().PaddingHorizontal(7).Row(r =>
                            {
                                r.AutoItem().Border(1).Width(14).Height(14).AlignCenter().AlignMiddle()
                                    .Text(isCheque ? "X" : "").FontSize(11);
                                r.AutoItem().PaddingLeft(4).Text("شيكات").FontSize(11)
                                    .FontFamily("Lato", "Noto Sans Arabic");
                            });

                            // Cash checkbox
                            checkRow.AutoItem().PaddingHorizontal(7).Row(r =>
                            {
                                r.AutoItem().Border(1).Width(14).Height(14).AlignCenter().AlignMiddle()
                                    .Text(isCash ? "X" : "").FontSize(11);
                                r.AutoItem().PaddingLeft(4).Text("نقدية").FontSize(11)
                                    .FontFamily("Lato", "Noto Sans Arabic");
                            });
                        });

                        // ===== DATE ROW =====
                        mainCol.Item().PaddingTop(12).AlignRight().Row(r =>
                        {
                            r.AutoItem().Text(FormatArabicDate(data.EffectiveDate)).FontSize(14);
                            r.AutoItem().Text(": تحريراً فى").FontSize(14).FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== AMOUNT ROW =====
                        mainCol.Item().PaddingTop(12).AlignCenter().Row(r =>
                        {
                            // Amount boxes (RTL: [amount box] جنيه [piasters box] قرش)
                            r.AutoItem().AlignMiddle().Text("قرش").FontSize(13).FontFamily("Lato", "Noto Sans Arabic");
                            r.AutoItem().PaddingHorizontal(7).Border(1).BorderColor(Colors.Grey.Medium).Padding(4)
                                .Text(ToArabicNumerals(((int)((data.Amount - (int)data.Amount) * 100)).ToString("00")))
                                .FontSize(13);
                            r.AutoItem().PaddingLeft(12).AlignMiddle().Text("جنيه").FontSize(13)
                                .FontFamily("Lato", "Noto Sans Arabic");
                            r.AutoItem().MaxWidth(200).PaddingHorizontal(7).Border(1).BorderColor(Colors.Grey.Medium).Padding(4)
                                .Text(ToArabicNumerals(((int)data.Amount).ToString("N0"))).FontSize(13)
                                .WrapAnywhere();
                        });

                        // ===== RECIPIENT ROW =====
                        mainCol.Item().PaddingTop(12).AlignRight().Row(r =>
                        {
                            bool isReceipt = string.Equals(data.PaymentParentTypeDescription?.Trim(),
                                "RECEIPT",
                                StringComparison.OrdinalIgnoreCase);

                            if (isReceipt)
                            {
                                // Logical order (left → right in code) = visual right → left in PDF
                                // استلمنا ...   من السيد/السادة   [FromPartyName]

                                r.RelativeItem(1).BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingHorizontal(9)
                                    .AlignMiddle().AlignRight()
                                    .Text(data.FromPartyName ?? "")
                                    .FontSize(14).FontFamily("Lato", "Noto Sans Arabic");

                                r.AutoItem().AlignMiddle().PaddingHorizontal(11).Text("من السيد / السادة")
                                    .FontSize(14).FontFamily("Lato", "Noto Sans Arabic");

                                r.AutoItem().AlignMiddle().Text("استلمنا نحن / جولدن لاند للتطوير العقاري")
                                    .FontSize(14).FontFamily("Lato", "Noto Sans Arabic");
                            }
                            else
                            {
                                // Payment style (original):
                                // صرفنا إلى السيد / السادة   [ToPartyName]

                                r.RelativeItem(1).BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingHorizontal(9)
                                    .AlignMiddle().AlignRight()
                                    .Text(data.ToPartyName ?? "")
                                    .FontSize(14).FontFamily("Lato", "Noto Sans Arabic");

                                r.AutoItem().AlignMiddle().PaddingLeft(11).Text("صرفنا إلى السيد / السادة :")
                                    .FontSize(14).FontFamily("Lato", "Noto Sans Arabic");
                            }
                        });

                        // ===== AMOUNT IN WORDS ROW =====
                        mainCol.Item().PaddingTop(12).Row(r =>
                        {
                            r.RelativeItem().AlignMiddle().AlignRight().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .Text(amountInWords).FontSize(14).FontFamily("Lato", "Noto Sans Arabic")
                                .WrapAnywhere();
                            r.AutoItem().AlignMiddle().Text(" : فقط وقدره").FontSize(14)
                                .FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== CHEQUE DETAILS ROW 1 =====
                        mainCol.Item().PaddingTop(12).Row(r =>
                        {
                            // Payment type
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .AlignCenter().AlignMiddle().Text(isCash ? "نقداً" : (isCheque ? "شيك" : ""))
                                .FontSize(13).FontFamily("Lato", "Noto Sans Arabic");
                            r.AutoItem().AlignMiddle().Text(" : نقداً / بموجب").FontSize(13)
                                .FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== CHEQUE DETAILS ROW 2 (Bank) =====
                        mainCol.Item().PaddingTop(8).Row(r =>
                        {
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium);
                            r.AutoItem().AlignMiddle().Text(" مسحوب على بنك").FontSize(13)
                                .FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== CHEQUE DETAILS ROW 3 (Number & Date) =====
                        mainCol.Item().PaddingTop(8).AlignRight().Row(r =>
                        {
                            // Date placeholder
                            var chequeDateText = data.ChequeDate.HasValue
                                ? FormatArabicDate(data.ChequeDate.Value)
                                : "٢٠    /    /    ";
                            r.AutoItem().Text(chequeDateText).FontSize(13);
                            r.AutoItem().Text(" حق  ").FontSize(13).FontFamily("Lato", "Noto Sans Arabic");

                            // Cheque number
                            r.AutoItem().MaxWidth(200).BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingHorizontal(14)
                                .Text(ToArabicNumerals(data.ChequeNumber ?? "")).FontSize(13)
                                .FontFamily("Lato", "Noto Sans Arabic")
                                .WrapAnywhere();
                            r.AutoItem().Text(" رقم").FontSize(13).FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== BANK TRANSFER ROW =====
                        mainCol.Item().PaddingTop(12).Row(r =>
                        {
                            r.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Medium);
                            r.AutoItem().AlignMiddle().MaxWidth(200).BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .Text(isBankTransfer ? (data.PaymentMethodDescription ?? "") : "").FontSize(13)
                                .FontFamily("Lato", "Noto Sans Arabic")
                                .WrapAnywhere();
                            r.AutoItem().AlignMiddle().Text(" : تحويل ( بنكى ، اون لاين )").FontSize(13)
                                .FontFamily("Lato", "Noto Sans Arabic");
                        });

                        // ===== PURPOSE ROW =====
                        mainCol.Item().PaddingTop(12).Column(purposeCol =>
                        {
                            purposeCol.Item().AlignRight()
                                .Text("وذلك عن :")
                                .FontSize(14).FontFamily("Lato", "Noto Sans Arabic");

                            purposeCol.Item().PaddingTop(3)
                                .BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                .PaddingBottom(3)
                                .Text(data.Comments ?? "")
                                .FontSize(14).FontFamily("Lato", "Noto Sans Arabic")
                                .WrapAnywhere();
                        });

                        // ===== SPACER =====
                        mainCol.Item().PaddingVertical(30);

                        // ===== SIGNATURE SECTION =====
                        mainCol.Item().Row(sigRow =>
                        {
                            // Left: Approved
                            sigRow.RelativeItem().AlignLeft().Column(c =>
                            {
                                c.Item().Text("...يعتمد").FontSize(14).FontFamily("Lato", "Noto Sans Arabic");
                                c.Item().PaddingTop(35).BorderBottom(1);
                            });

                            // Center: Accountant
                            sigRow.RelativeItem().PaddingHorizontal(14).AlignCenter().Column(c =>
                            {
                                c.Item().AlignCenter().Text("المحاسب").FontSize(14)
                                    .FontFamily("Lato", "Noto Sans Arabic");
                                c.Item().PaddingTop(35).AlignCenter().BorderBottom(1);
                            });

                            // Right: Recipient
                            sigRow.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().AlignRight().Text("المستلم").FontSize(14)
                                    .FontFamily("Lato", "Noto Sans Arabic");
                                c.Item().PaddingTop(12).Row(r =>
                                {
                                    r.RelativeItem().BorderBottom(1);
                                    r.AutoItem().PaddingLeft(4).Text("الاسم").FontSize(13)
                                        .FontFamily("Lato", "Noto Sans Arabic");
                                });
                                c.Item().PaddingTop(12).Row(r =>
                                {
                                    r.RelativeItem().BorderBottom(1);
                                    r.AutoItem().PaddingLeft(4).Text("التوقيع").FontSize(13)
                                        .FontFamily("Lato", "Noto Sans Arabic");
                                });
                            });
                        });

                        // ===== PAYMENT ID (small reference) =====
                        mainCol.Item().PaddingTop(8).AlignLeft().Text($"مرجع: {data.PaymentId}").FontSize(9)
                            .FontColor(Colors.Grey.Medium);
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

        private string FormatArabicDate(DateOnly? date)
        {
            if (!date.HasValue)
                return "٢٠    /    /    ";   // Empty placeholder

            return ToArabicNumerals(date.Value.ToString("yyyy/MM/dd"));
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
