using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings
{
    public class GetInvoiceItemTypes
    {
        public class Query : IRequest<Result<List<InvoiceItemTypeDto>>>
        {
            public string InvoiceTypeId { get; set; }
            public string Language { get; set; }
        }

        // REFACTOR: Simplified DTO to match PaymentTypeDto structure
        // Purpose: Includes only essential fields and supports language-based description
        // Improvement: Reduces payload size and aligns with ListOutgoingPaymentTypes
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

            // REFACTOR: Updated InvoiceTypeMap to include simplified item types for all invoice types
            // Purpose: Simplifies SALES_INVOICE, PAYROL_INVOICE, COMMISSION_INVOICE, and adds CUSTOMER_RETURN, SUPPLIER_RETURN with essential types
            // Improvement: Reduces complexity, improves query performance, and ensures usability
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

            // REFACTOR: Updated to handle language parameter and simplified query
            // Purpose: Selects language-appropriate description and filters by InvoiceTypeMap
            // Improvement: Aligns with ListOutgoingPaymentTypes, removes unused PartyId/PartyIdFrom
            private async Task<List<InvoiceItemTypeDto>> GetInvoiceItemTypes(string invoiceTypeId, string language, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Fetching invoice item types for {InvoiceTypeId} with language {Language}", invoiceTypeId, language);

                if (InvoiceTypeMap.TryGetValue(invoiceTypeId, out var validItemTypes))
                {
                    _logger.LogInformation("Valid item types for {InvoiceTypeId}: {ValidItemTypes}", invoiceTypeId, string.Join(", ", validItemTypes));

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
                        items.Count, invoiceTypeId, string.Join(", ", items.Select(x => x.InvoiceItemTypeId)));

                    return items;
                }

                // REFACTOR: Simplified fallback to return empty list for unknown types
                // Purpose: Avoids complex InvoiceItemTypeMaps query, aligns with ListOutgoingPaymentTypes
                // Improvement: Reduces fallback complexity, logs warning for invalid types
                _logger.LogWarning("No predefined item types for {InvoiceTypeId}", invoiceTypeId);
                return new List<InvoiceItemTypeDto>();
            }

            // REFACTOR: Removed unused PartyId/PartyIdFrom parameters
            // Purpose: Simplifies handler to match ListOutgoingPaymentTypes
            // Improvement: Reduces unnecessary data passing, improves maintainability
            public async Task<Result<List<InvoiceItemTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    _logger.LogInformation("Handling GetInvoiceItemTypes for invoiceTypeId: {InvoiceTypeId}, language: {Language}",
                        request.InvoiceTypeId, request.Language);

                    var invoiceItemTypes = await GetInvoiceItemTypes(request.InvoiceTypeId, request.Language, cancellationToken);

                    if (!invoiceItemTypes.Any())
                    {
                        _logger.LogWarning("No invoice item types found for {InvoiceTypeId}", request.InvoiceTypeId);
                    }

                    return Result<List<InvoiceItemTypeDto>>.Success(invoiceItemTypes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving invoice item types for {InvoiceTypeId}", request.InvoiceTypeId);
                    return Result<List<InvoiceItemTypeDto>>.Failure(ex.Message);
                }
            }
        }
    }
}