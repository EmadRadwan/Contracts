using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Invoices
{
    public class GetInvoiceItemTypesByInvoiceId
    {
        public class Query : IRequest<Result<List<InvoiceItemTypeDto>>>
        {
            public string InvoiceId { get; set; }
            public string Language { get; set; }
        }

        // REFACTOR: Reused InvoiceItemTypeDto from original code
        // Purpose: Maintains consistency with existing DTO structure
        // Improvement: Ensures compatibility with frontend expectations
        public class InvoiceItemTypeDto
        {
            public string InvoiceItemTypeId { get; set; }
            public string Description { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<InvoiceItemTypeDto>>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
            }

            // REFACTOR: Reused InvoiceTypeMap from original code
            // Purpose: Maintains the same filtering logic for invoice item types
            // Improvement: Ensures consistency in item type filtering across handlers
            private static readonly Dictionary<string, List<string>> InvoiceTypeMap = new()
            {
                {
                    "SALES_INVOICE",
                    new List<string>
                    {
                        "INV_PROD_ITEM",
                        "INV_DPROD_ITEM",
                        "INV_FPROD_ITEM",
                        "INV_SPROD_ITEM",
                        "INV_SALES_TAX",
                        "INV_SHIPPING_CHARGES",
                        "INV_DISCOUNT_ADJ",
                        "FEE",
                        "ITM_SALES_TAX",
                        "ITM_SHIPPING_CHARGES",
                        "ITM_DISCOUNT_ADJ",
                        "ITM_FEE"
                    }
                },
                {
                    "PURCHASE_INVOICE",
                    new List<string>
                    {
                        "PINV_PROD_ITEM",
                        "PINV_DPROD_ITEM",
                        "PINV_FPROD_ITEM",
                        "PINV_SPROD_ITEM",
                        "PINV_SALES_TAX",
                        "PINV_SHIP_CHARGES",
                        "PINV_DISCOUNT_ADJ",
                        "P_FEE",
                        "PITM_SALES_TAX",
                        "PITM_SHIP_CHARGES",
                        "PITM_DISCOUNT_ADJ",
                        "PITM_FEE"
                    }
                },
                {
                    "PAYROL_INVOICE",
                    new List<string>
                    {
                        "PAYROL_EARN_HOURS",
                        "PAYROL_SALARY",
                        "PAYROL_HRLY_RATE",
                        "PAYROL_BONUS",
                        "PAYROL_COMMISSION",
                        "PAYROL_DD_FROM_GROSS",
                        "PAYROL_DD_401K",
                        "PAYROL_DD_MISC",
                        "PAYROL_TAXES",
                        "PAYROL_TAX_FEDERAL",
                        "PAYROL_TAX_MED_EMPL",
                        "PAYROL_SOC_SEC_EMPL"
                    }
                },
                {
                    "COMMISSION_INVOICE",
                    new List<string>
                    {
                        "COMM_INV_ITEM",
                        "COMM_INV_ADJ"
                    }
                },
                {
                    "CUSTOMER_RETURN",
                    new List<string>
                    {
                        "CRT_PROD_ITEM",
                        "CRT_DPROD_ITEM",
                        "CRT_FPROD_ITEM",
                        "CRT_SPROD_ITEM",
                        "CRT_SALES_TAX_ADJ",
                        "CRT_SHIPPING_ADJ",
                        "CRT_DISCOUNT_ADJ",
                        "CRT_FEE_ADJ"
                    }
                },
                {
                    "SUPPLIER_RETURN",
                    new List<string>
                    {
                        "SRT_PROD_ITEM",
                        "SRT_DPROD_ITEM",
                        "SRT_FPROD_ITEM",
                        "SRT_SPROD_ITEM",
                        "SRT_SALES_TAX_ADJ",
                        "SRT_SHIPPING_ADJ",
                        "SRT_DISCOUNT_ADJ",
                        "SRT_FEE_ADJ"
                    }
                }
            };

            // REFACTOR: Modified GetInvoiceItemTypes to accept invoiceId and query invoiceTypeId
            // Purpose: Queries Invoice entity to get invoiceTypeId before filtering item types
            // Improvement: Allows dynamic invoice type determination based on invoiceId
            private async Task<List<InvoiceItemTypeDto>> GetInvoiceItemTypes(string invoiceId, string language, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Fetching invoice for {InvoiceId} to determine invoice type", invoiceId);

                // REFACTOR: Added query to fetch invoiceTypeId from Invoice entity
                // Purpose: Retrieves invoiceTypeId dynamically based on provided invoiceId
                // Improvement: Ensures accurate filtering without requiring invoiceTypeId as input
                var invoice = await _context.Invoices
                    .Where(x => x.InvoiceId == invoiceId)
                    .Select(x => x.InvoiceTypeId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (string.IsNullOrEmpty(invoice))
                {
                    _logger.LogWarning("No invoice found for {InvoiceId}", invoiceId);
                    return new List<InvoiceItemTypeDto>();
                }

                _logger.LogInformation("Invoice {InvoiceId} has type {InvoiceTypeId}", invoiceId, invoice);

                if (InvoiceTypeMap.TryGetValue(invoice, out var validItemTypes))
                {
                    _logger.LogInformation("Valid item types for {InvoiceTypeId}: {ValidItemTypes}", invoice, string.Join(", ", validItemTypes));

                    var items = await _context.InvoiceItemTypes
                        .Where(x => validItemTypes.Contains(x.InvoiceItemTypeId) || validItemTypes.Contains(x.ParentTypeId))
                        .OrderBy(x => x.ParentTypeId)
                        .ThenBy(x => x.InvoiceItemTypeId)
                        .Select(x => new InvoiceItemTypeDto
                        {
                            InvoiceItemTypeId = x.InvoiceItemTypeId,
                            Description = language == "ar" ? (x.DescriptionArabic ?? x.Description) : x.Description
                        })
                        .ToListAsync(cancellationToken);

                    _logger.LogInformation("Retrieved {Count} invoice item types for {InvoiceTypeId}: {Items}",
                        items.Count, invoice, string.Join(", ", items.Select(x => x.InvoiceItemTypeId)));

                    return items;
                }

                // REFACTOR: Reused simplified fallback from original code
                // Purpose: Returns empty list for unknown invoice types
                // Improvement: Maintains consistency and simplicity
                _logger.LogWarning("No predefined item types for {InvoiceTypeId}", invoice);
                return new List<InvoiceItemTypeDto>();
            }

            // REFACTOR: Updated Handle method to use invoiceId instead of invoiceTypeId
            // Purpose: Integrates invoiceId-based query logic
            // Improvement: Aligns with new requirement while maintaining error handling
            public async Task<Result<List<InvoiceItemTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    _logger.LogInformation("Handling GetInvoiceItemTypesByInvoiceId for invoiceId: {InvoiceId}, language: {Language}",
                        request.InvoiceId, request.Language);

                    var invoiceItemTypes = await GetInvoiceItemTypes(request.InvoiceId, request.Language, cancellationToken);

                    if (!invoiceItemTypes.Any())
                    {
                        _logger.LogWarning("No invoice item types found for invoiceId: {InvoiceId}", request.InvoiceId);
                    }

                    return Result<List<InvoiceItemTypeDto>>.Success(invoiceItemTypes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving invoice item types for invoiceId: {InvoiceId}", request.InvoiceId);
                    return Result<List<InvoiceItemTypeDto>>.Failure(ex.Message);
                }
            }
        }
    }
}