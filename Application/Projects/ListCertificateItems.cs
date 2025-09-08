using Application.Catalog.Products;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class ListCertificateItems
{
    public class Query : IRequest<Result<List<CertificateItemDto>>>
    {
        public string WorkEffortId { get; set; }
        public string Language { get; set; }
    }

    // REFACTOR: Add validator for Query
    // Purpose: Ensure WorkEffortId and Language are valid
    // Context: Aligns with validation in Create/Update handlers
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.WorkEffortId).NotEmpty().WithMessage("Work Effort ID is required");
            RuleFor(x => x.Language).NotEmpty().WithMessage("Language is required");
        }
    }

    public class Handler : IRequestHandler<Query, Result<List<CertificateItemDto>>>
    {
        private readonly DataContext _context;

        // REFACTOR: Remove IMapper dependency
        // Purpose: Avoid AutoMapper as per request
        // Context: Use manual mapping to CertificateItemDto
        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CertificateItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Validate inputs
            // Purpose: Prevent invalid queries
            // Context: Matches validation pattern from Create/Update handlers
            var validator = new QueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result<List<CertificateItemDto>>.Failure(string.Join("; ",
                    validationResult.Errors.Select(e => e.ErrorMessage)));

            // REFACTOR: Default to English for language
            // Purpose: Ensure consistent behavior
            // Context: Maintains original behavior with added validation
            var language = request.Language?.ToLower() == "en" ? "en" : request.Language?.ToLower() ?? "en";

            try
            {
                // REFACTOR: Fetch certificate items with joins
                // Purpose: Include product and UOM data, filter by WorkEffortParentId
                // Context: Uses EF Core for efficient querying, supports both certificate item types
                var certificateItems = await _context.WorkEfforts
                    .Where(we => we.WorkEffortParentId == request.WorkEffortId &&
                                 we.WorkEffortTypeId == "CERTIFICATE_ITEM")
                    .GroupJoin(
                        _context.Products,
                        we => we.ProductId,
                        prd => prd.ProductId,
                        (we, prdGroup) => new { WorkEffort = we, Products = prdGroup }
                    )
                    .SelectMany(
                        x => x.Products.DefaultIfEmpty(),
                        (x, prd) => new { x.WorkEffort, Product = prd }
                    )
                    .GroupJoin(
                        _context.Uoms,
                        x => x.WorkEffort.QuantityUomId,
                        uom => uom.UomId,
                        (x, uomGroup) => new { x.WorkEffort, x.Product, Uoms = uomGroup }
                    )
                    .SelectMany(
                        x => x.Uoms.DefaultIfEmpty(),
                        (x, uom) => new CertificateItemDto
                        {
                            WorkEffortId = x.WorkEffort.WorkEffortId,
                            WorkEffortParentId = x.WorkEffort.WorkEffortParentId,
                            ProductId = x.WorkEffort.ProductId, // REFACTOR: Simple string from WorkEffort.ProductId
                            ProductIdObject = x.Product != null
                                ? new ProductLovDto
                                {
                                    ProductId = x.Product.ProductId,
                                    ProductName = x.Product.ProductName
                                }
                                : null, // REFACTOR: Full Product details
                            QuantityUom =
                                x.WorkEffort.QuantityUomId, // REFACTOR: Simple string from WorkEffort.QuantityUomId
                            QuantityUomObject = uom != null
                                ? new UomLovDto
                                {
                                    UomId = uom.UomId,
                                    Description = uom.Description
                                }
                                : null, // REFACTOR: Full Uom details
                            Description = x.WorkEffort.Description,
                            ProductName = x.Product != null ? x.Product.ProductName : null,
                            UomDescription = uom != null ? uom.Description : null,
                            Quantity = x.WorkEffort.Quantity ?? 0,
                            UnitPrice = x.WorkEffort.Rate ?? 0,
                            TotalAmount = (x.WorkEffort.Quantity ?? 0) * (x.WorkEffort.Rate ?? 0),
                            Discount = x.WorkEffort.DiscountAmount ?? 0,
                            Insurance = x.WorkEffort.InsuranceAmount ?? 0,
                            CompletionPercentage = x.WorkEffort.CompletionPercentage ?? 0,
                            Notes = x.WorkEffort.Notes,
                            ProcurementDate = x.WorkEffort.ProcurementDate,
                            FacilityId = x.WorkEffort.FacilityId,
                            IsDeleted = false
                        }
                    )
                    .ToListAsync(cancellationToken);


                return Result<List<CertificateItemDto>>.Success(certificateItems);
            }
            catch (Exception ex)
            {
                // REFACTOR: Add exception handling
                // Purpose: Provide clear error messages
                // Context: Aligns with Create/Update handler error handling
                return Result<List<CertificateItemDto>>.Failure($"Failed to retrieve certificate items: {ex.Message}");
            }
        }
    }
}