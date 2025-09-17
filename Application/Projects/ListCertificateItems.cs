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


        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CertificateItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Purpose: Prevent invalid queries
            // Context: Matches validation pattern from Create/Update handlers
            var validator = new QueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result<List<CertificateItemDto>>.Failure(string.Join("; ",
                    validationResult.Errors.Select(e => e.ErrorMessage)));

            // Purpose: Ensure consistent behavior
            // Context: Maintains original behavior with added validation
            var language = request.Language?.ToLower() == "en" ? "en" : request.Language?.ToLower() ?? "en";

            try
            {
                var certificateItems = await _context.WorkEfforts
                    .Where(we =>
                        we.WorkEffortParentId == request.WorkEffortId && we.WorkEffortTypeId == "CERTIFICATE_ITEM")
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
                        (x, uom) => new { x.WorkEffort, x.Product, Uom = uom }
                    )
                    .GroupJoin(
                        _context.Facilities,
                        x => x.WorkEffort.FacilityId,
                        fac => fac.FacilityId,
                        (x, facGroup) => new { x.WorkEffort, x.Product, x.Uom, Facilities = facGroup }
                    )
                    .SelectMany(
                        x => x.Facilities.DefaultIfEmpty(),
                        (x, fac) => new CertificateItemDto
                        {
                            WorkEffortId = x.WorkEffort.WorkEffortId,
                            WorkEffortParentId = x.WorkEffort.WorkEffortParentId,
                            ProductId = x.WorkEffort.ProductId,
                            ProductIdObject = x.Product != null
                                ? new ProductLovDto
                                {
                                    ProductId = x.Product.ProductId,
                                    ProductName = x.Product.ProductName
                                }
                                : null,
                            UomId = x.WorkEffort.QuantityUomId,
                            QuantityUomObject = x.Uom != null
                                ? new UomLovDto
                                {
                                    UomId = x.Uom.UomId,
                                    Description = x.Uom.Description
                                }
                                : null,
                            Description = x.WorkEffort.Description,
                            ProductName = x.Product != null ? x.Product.ProductName : null,
                            UomName = x.Uom != null ? x.Uom.Description : null,
                            Quantity = (decimal)x.WorkEffort.Quantity,
                            UnitPrice = (decimal)x.WorkEffort.Rate,
                            TotalAmount = x.WorkEffort.TotalAmount ??
                                          ((x.WorkEffort.Quantity ?? 0m) * (x.WorkEffort.Rate ?? 0m)),
                            Discount = x.WorkEffort.Discount,
                            Insurance = x.WorkEffort.Insurance,
                            CompletionPercentage = x.WorkEffort.CompletionPercentage,
                            Notes = x.WorkEffort.Notes,
                            ProcurementDate =
                                x.WorkEffort.ProcurementDate ??
                                x.WorkEffort.CreatedDate, // Fallback as per CSV timestamps
                            IsDeleted = false,
                            MaterialPrice = x.WorkEffort.MaterialPrice ?? 0m,
                            LaborPrice = x.WorkEffort.LaborPrice ?? 0m,
                            AdditionalInsurance = x.WorkEffort.AdditionalInsurance ?? 0m,
                            AchievementPercentage = x.WorkEffort.AchievementPercent,
                            TransportationExpenses = x.WorkEffort.TransportationExpenses,
                            Gratuities = x.WorkEffort.Gratuities,
                            Deductions = x.WorkEffort.Deductions,
                        }
                    )
                    .ToListAsync(cancellationToken);

                return Result<List<CertificateItemDto>>.Success(certificateItems);
            }
            catch (Exception ex)
            {
                // Purpose: Provide clear error messages
                // Context: Aligns with Create/Update handler error handling
                return Result<List<CertificateItemDto>>.Failure($"Failed to retrieve certificate items: {ex.Message}");
            }
        }
    }
}