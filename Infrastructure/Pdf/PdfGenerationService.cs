using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application.Projects; // Assuming this contains IPdfGenerationService
namespace Infrastructure.Pdf
{
    // REFACTOR: Implement PDF generation with iText7
    // Purpose: Generates PDF server-side for certificates
    // Improvement: Eliminates client-side Buffer and bidi errors; supports Arabic RTL and complex layouts
    public class PdfGenerationService : IPdfGenerationService
    {
        private readonly string _fontsPath;
        private readonly string _imagesPath;

        public PdfGenerationService(IWebHostEnvironment env)
        {
            _fontsPath = Path.Combine(env.WebRootPath, "fonts");
            _imagesPath = Path.Combine(env.WebRootPath, "images");
        }

        public async Task<byte[]> GenerateCertificatePdfAsync(
            string certificateType,
            Certificate certificate,
            List<CertificateItem> items,
            double subtotal,
            Dictionary<string, string> translations)
        {
            // REFACTOR: Use MemoryStream for PDF generation
            // Purpose: Ensures PDF is generated in memory and returned as byte array
            // Improvement: Efficient and suitable for HTTP responses
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());

            // Set up fonts
            var fontPath = Path.Combine(_fontsPath, "DejaVuSans.ttf");
            var boldFontPath = Path.Combine(_fontsPath, "DejaVuSans-Bold.ttf");
            var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            var boldFont = PdfFontFactory.CreateFont(boldFontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

            // Set RTL direction
            document.SetTextAlignment(TextAlignment.RIGHT);
            document.SetFont(font);

            // Header: Logo
            try
            {
                var logoPath = Path.Combine(_imagesPath, "goldenlandlogo.jpg");
                // REFACTOR: Use correct iText Image type
                // Purpose: Ensures logo is properly embedded with correct dimensions
                // Improvement: Fixes previous MediaTypeNames.Image error, enabling SetWidth, SetHeight
                var image = new Image(iText.IO.Image.ImageDataFactory.Create(logoPath))
                    .SetWidth(100)
                    .SetHeight(100)
                    .SetMarginBottom(10);
                document.Add(image);
            }
            catch
            {
                // REFACTOR: Fallback for logo failure
                // Purpose: Adds text if logo fails to load
                // Improvement: Prevents PDF generation failure due to missing image
                document.Add(new Paragraph("Logo Unavailable").SetFontSize(10));
            }

            // Header: Title
            document.Add(
                new Paragraph($"{translations?["projects.certificate.report.title"] ?? "Certificate Report"}: {SafeString(certificate.CertificateNumber)}")
                    .SetFont(boldFont)
                    .SetFontSize(20)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10)
            );

            // Header: Details
            document.Add(
                new Paragraph($"{translations?["projects.certificate.type"] ?? "Type"}: {GetCertificateTypeTranslation(certificateType, translations)}")
                    .SetFontSize(10)
                    .SetMarginBottom(5)
            );
            document.Add(
                new Paragraph($"{translations?["projects.certificate.date"] ?? "Date"}: {DateTime.Now:yyyy-MM-dd}")
                    .SetFontSize(10)
                    .SetMarginBottom(5)
            );
            document.Add(
                new Paragraph($"{translations?["projects.certificate.description"] ?? "Description"}: {SafeString(certificate.Description)}")
                    .SetFontSize(10)
                    .SetMarginBottom(5)
            );
            document.Add(
                new Paragraph(
                    $"{translations?[certificateType == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? "projects.certificate.form.contractor" : "projects.certificate.form.supplier"] ?? "Supplier/Contractor"}: " +
                    $"{SafeString(certificate.PartyIdSupplier ?? certificate.PartyIdContractor)}"
                )
                    .SetFontSize(10)
                    .SetMarginBottom(5)
            );
            document.Add(
                new Paragraph($"{translations?["projects.certificate.total"] ?? "Total"}: {FormatNumber(subtotal)}")
                    .SetFontSize(10)
                    .SetMarginBottom(5)
            );
            if (certificateType != "WORKMANSHIP_CONTRACTING_CERTIFICATE")
            {
                document.Add(
                    new Paragraph($"{translations?["projects.certificate.form.facility"] ?? "Facility"}: {SafeString(certificate.FacilityName)}")
                        .SetFontSize(10)
                        .SetMarginBottom(10)
                );
            }

            // Table
            if (items.Any())
            {
                var isSupplyWithDiscount = certificateType == "SUPPLY_PROCUREMENT_CERTIFICATE";
                var columnCount = certificateType == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? 15 : isSupplyWithDiscount ? 11 : 10;
                var table = new Table(columnCount)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                // Table headers
                var headers = certificateType == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                    ? new[]
                    {
                        "projects.certificate.items.list.item",
                        "projects.certificate.items.list.code",
                        "projects.certificate.items.list.description",
                        "projects.certificate.items.list.quantity",
                        "projects.certificate.items.list.unitOfMeasure",
                        "projects.certificate.items.list.materialPrice",
                        "projects.certificate.items.list.laborPrice",
                        "projects.certificate.items.list.totalAmount",
                        "projects.certificate.items.list.deductions",
                        "projects.certificate.items.list.deductionDescription",
                        "projects.certificate.items.list.deserved",
                        "projects.certificate.items.list.insurance",
                        "projects.certificate.items.list.additionalInsurance",
                        "projects.certificate.items.list.net",
                        "projects.certificate.items.list.achievementPercentage"
                    }
                    : new[]
                    {
                        "projects.certificate.items.list.item",
                        "projects.certificate.items.list.code",
                        "projects.certificate.items.list.description",
                        "projects.certificate.items.list.quantity",
                        "projects.certificate.items.list.unitOfMeasure",
                        "projects.certificate.items.list.unitPrice",
                        "projects.certificate.items.list.totalAmount",
                        isSupplyWithDiscount ? "projects.certificate.items.list.discount" : null,
                        "projects.certificate.items.list.procurementDate",
                        "projects.certificate.items.list.transportationExpenses",
                        "projects.certificate.items.list.gratuities"
                    }.Where(h => h != null).ToArray();

                foreach (var header in headers)
                {
                    table.AddHeaderCell(
                        new Cell()
                            .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                            .SetFont(boldFont)
                            .SetFontSize(10)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .Add(new Paragraph(translations?[header] ?? header))
                    );
                }

                // Table rows
                foreach (var item in items)
                {
                    var row = certificateType == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                        ? new[]
                        {
                            // REFACTOR: Simplify string interpolation for product name
                            // Purpose: Removes nested interpolation to fix ": expected" and whitespace errors
                            // Improvement: Cleaner, more reliable string construction
                            item.IsLastInGroup && item.ProductSubtotal.HasValue
                                ? $"{SafeString(item.ProductName)} ({FormatNumber(item.ProductSubtotal.Value)})"
                                : SafeString(item.ProductName),
                            SafeString(item.Code),
                            SafeString(item.Description),
                            FormatNumber(item.Quantity, 0),
                            SafeString(item.UomName),
                            FormatNumber(item.MaterialPrice),
                            FormatNumber(item.LaborPrice),
                            FormatNumber(item.DisplayTotal),
                            FormatNumber(item.Deductions),
                            SafeString(item.DeductionDescription),
                            FormatNumber(item.Deserved),
                            FormatNumber(item.Insurance),
                            FormatNumber(item.AdditionalInsurance),
                            FormatNumber(item.Net),
                            SafeString(item.AchievementPercentage)
                        }
                        : new[]
                        {
                            // REFACTOR: Simplify string interpolation for product name
                            // Purpose: Removes nested interpolation to fix ": expected" and whitespace errors
                            // Improvement: Cleaner, more reliable string construction
                            item.IsLastInGroup && item.ProductSubtotal.HasValue
                                ? $"{SafeString(item.ProductName)} ({FormatNumber(item.ProductSubtotal.Value)})"
                                : SafeString(item.ProductName),
                            SafeString(item.Code),
                            SafeString(item.Description),
                            FormatNumber(item.Quantity, 0),
                            SafeString(item.UomName),
                            FormatNumber(item.UnitPrice),
                            FormatNumber(item.DisplayTotal),
                            isSupplyWithDiscount ? FormatNumber(item.Discount) : null,
                            SafeString(item.FormattedProcurementDate),
                            FormatNumber(item.TransportationExpenses),
                            FormatNumber(item.Gratuities)
                        }.Where(c => c != null).ToArray();

                    foreach (var cell in row)
                    {
                        table.AddCell(
                            new Cell()
                                .SetFontSize(9)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .Add(new Paragraph(cell))
                        );
                    }
                }

                document.Add(table);
            }
            else
            {
                document.Add(
                    new Paragraph(translations?["projects.certificate.items.list.noData"] ?? "No items available")
                        .SetFontSize(9)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.RED)
                );
            }

            // Notes
            if (items.Any(item => !string.IsNullOrWhiteSpace(item.MainItemDescription)))
            {
                document.Add(
                    new Paragraph(translations?["projects.certificate.items.mainDescription"] ?? "Main Item Description")
                        .SetFont(boldFont)
                        .SetFontSize(10)
                        .SetMarginTop(15)
                );
                foreach (var item in items.Where(i => !string.IsNullOrWhiteSpace(i.MainItemDescription)))
                {
                    document.Add(
                        new Paragraph(SafeString(item.MainItemDescription))
                            .SetFontSize(9)
                    );
                }
            }

            if (items.Any(item => !string.IsNullOrWhiteSpace(item.DiscountNote)))
            {
                document.Add(
                    new Paragraph(translations?["projects.certificate.items.discountNote"] ?? "Discount Description Note")
                        .SetFont(boldFont)
                        .SetFontSize(10)
                        .SetMarginTop(15)
                );
                foreach (var item in items.Where(i => !string.IsNullOrWhiteSpace(i.DiscountNote)))
                {
                    document.Add(
                        new Paragraph(SafeString(item.DiscountNote))
                            .SetFontSize(9)
                    );
                }
            }

            document.Close();
            return stream.ToArray();
        }

        // REFACTOR: Safe string and number formatting
        // Purpose: Prevents null or invalid data from breaking PDF
        // Improvement: Ensures robust data handling, avoiding errors from malformed inputs
        private string SafeString(string value) => string.IsNullOrWhiteSpace(value) ? "N/A" : value.Trim();
        private string FormatNumber(double? value, int decimals = 2) =>
            value.HasValue && !double.IsNaN(value.Value)
                ? value.Value.ToString($"N{decimals}", System.Globalization.CultureInfo.InvariantCulture)
                : "N/A";

        private string GetCertificateTypeTranslation(string type, Dictionary<string, string> translations) =>
            type switch
            {
                "SUPPLY_PROCUREMENT_CERTIFICATE" => translations?["SUPPLY_PROCUREMENT_CERTIFICATE"] ?? "مستخلص توريدات",
                "COMPANY_SUPPLY_SALE_CERTIFICATE" => translations?["COMPANY_SUPPLY_SALE_CERTIFICATE"] ?? "مستخلص مقاوله",
                "WORKMANSHIP_CONTRACTING_CERTIFICATE" => translations?["WORKMANSHIP_CONTRACTING_CERTIFICATE"] ?? "مستخلص توريدات من مخازن الشركة",
                _ => type
            };
    }
}