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
                return Result<List<CertificateItemDto>>.Failure(
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var language = request.Language?.ToLower() == "en" ? "en" : request.Language?.ToLower() ?? "en";

            try
            {
                var certificateItems = await _context.WorkEfforts
                    .Where(we => we.WorkEffortParentId == request.WorkEffortId && we.WorkEffortTypeId == "CERTIFICATE_ITEM")
                    .GroupJoin(
                        _context.WorkEfforts,
                        we => we.WorkEffortParentId,
                        parent => parent.WorkEffortId,
                        (we, parentGroup) => new { WorkEffort = we, Parent = parentGroup }
                    )
                    .SelectMany(
                        x => x.Parent.DefaultIfEmpty(),
                        (x, parent) => new { x.WorkEffort, Parent = parent }
                    )
                    .GroupJoin(
                        _context.Products,
                        x => x.WorkEffort.ProductId,
                        prd => prd.ProductId,
                        (x, prdGroup) => new { x.WorkEffort, x.Parent, Products = prdGroup }
                    )
                    .SelectMany(
                        x => x.Products.DefaultIfEmpty(),
                        (x, prd) => new { x.WorkEffort, x.Parent, Product = prd }
                    )
                    .GroupJoin(
                        _context.Uoms,
                        x => x.WorkEffort.QuantityUomId,
                        uom => uom.UomId,
                        (x, uomGroup) => new { x.WorkEffort, x.Parent, x.Product, Uoms = uomGroup }
                    )
                    .SelectMany(
                        x => x.Uoms.DefaultIfEmpty(),
                        (x, uom) => new { x.WorkEffort, x.Parent, x.Product, Uom = uom }
                    )
                    .GroupJoin(
                        _context.Facilities,
                        x => x.WorkEffort.FacilityId,
                        fac => fac.FacilityId,
                        (x, facGroup) => new { x.WorkEffort, x.Parent, x.Product, x.Uom, Facilities = facGroup }
                    )
                    .SelectMany(
                        x => x.Facilities.DefaultIfEmpty(),
                        (x, fac) => new { x.WorkEffort, x.Parent, x.Product, x.Uom }
                    )
                    // REFACTOR: All calculations in single expression
                    // Purpose: EF Core can translate to SQL; no statement body
                    .Select(x => new CertificateItemDto
                    {
                        WorkEffortId = x.WorkEffort.WorkEffortId,
                        WorkEffortParentId = x.WorkEffort.WorkEffortParentId,
                        ProductId = x.WorkEffort.ProductId,
                        ProductIdObject = x.Product != null
                            ? new ProductLovDto { ProductId = x.Product.ProductId, ProductName = x.Product.ProductName }
                            : null,
                        UomId = x.WorkEffort.QuantityUomId,
                        QuantityUomObject = x.Uom != null
                            ? new UomLovDto { UomId = x.Uom.UomId, Description = x.Uom.Description }
                            : null,
                        Description = x.WorkEffort.Description,
                        ProductName = x.Product.ProductName,
                        UomName = x.Uom != null
                            ? (language == "ar" ? x.Uom.DescriptionArabic : x.Uom.Description)
                            : null,
                        UomAbbreviation = x.Uom != null ? x.Uom.Abbreviation : null,
                        Quantity = x.WorkEffort.Quantity ?? 0m,
                        UnitPrice = (decimal)(x.WorkEffort.Rate ?? 0m),
                        MaterialPrice = x.WorkEffort.MaterialPrice ?? 0m,
                        LaborPrice = x.WorkEffort.LaborPrice ?? 0m,

                        // REFACTOR: TotalAmount = quantity × (material + labor) for Workmanship
                        // Fallback: legacy supply calculation
                        TotalAmount = (x.Parent.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE")
                            ? (x.WorkEffort.Quantity ?? 0m) * ((x.WorkEffort.MaterialPrice ?? 0m) + (x.WorkEffort.LaborPrice ?? 0m))
                            : (x.WorkEffort.TotalAmount ?? 
                               ((x.WorkEffort.Quantity ?? 0m) * (x.WorkEffort.Rate ?? 0m)) +
                               (x.WorkEffort.TransportationExpenses ?? 0m) +
                               (x.WorkEffort.Gratuities ?? 0m) -
                               (x.WorkEffort.Discount ?? 0m)),

                        // REFACTOR: Deserved = total × achievement% – deductions
                        // Purpose: Calculated on-the-fly; matches UI
                        Deserved = (x.Parent.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE")
                            ? Math.Max(0m,
                                ((x.WorkEffort.Quantity ?? 0m) *
                                 ((x.WorkEffort.MaterialPrice ?? 0m) + (x.WorkEffort.LaborPrice ?? 0m)) *
                                 ((decimal)(x.WorkEffort.AchievementPercent ?? 0) / 100m)) -
                                (x.WorkEffort.Deductions ?? 0m))
                            : 0m,

                        // REFACTOR: Net = deserved – insurance – additionalInsurance
                        // Purpose: Final payable amount; matches UI "Net" column
                        Net = (x.Parent.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE")
                            ? Math.Max(0m,
                                (Math.Max(0m,
                                    ((x.WorkEffort.Quantity ?? 0m) *
                                     ((x.WorkEffort.MaterialPrice ?? 0m) + (x.WorkEffort.LaborPrice ?? 0m)) *
                                     ((decimal)(x.WorkEffort.AchievementPercent ?? 0) / 100m)) -
                                    (x.WorkEffort.Deductions ?? 0m))) -
                                (x.WorkEffort.Insurance ?? 0m) -
                                (x.WorkEffort.AdditionalInsurance ?? 0m))
                            : (x.WorkEffort.TotalAmount ?? 
                               ((x.WorkEffort.Quantity ?? 0m) * (x.WorkEffort.Rate ?? 0m)) +
                               (x.WorkEffort.TransportationExpenses ?? 0m) +
                               (x.WorkEffort.Gratuities ?? 0m) -
                               (x.WorkEffort.Discount ?? 0m)),

                        Discount = x.WorkEffort.Discount ?? 0m,
                        Insurance = x.WorkEffort.Insurance ?? 0m,
                        AdditionalInsurance = x.WorkEffort.AdditionalInsurance ?? 0m,
                        CompletionPercentage = x.WorkEffort.CompletionPercentage,
                        Notes = x.WorkEffort.Notes,
                        ProcurementDate = x.WorkEffort.ProcurementDate ?? x.WorkEffort.CreatedDate,
                        IsDeleted = false,
                        AchievementPercentage = x.WorkEffort.AchievementPercent,
                        TransportationExpenses = x.WorkEffort.TransportationExpenses ?? 0m,
                        Gratuities = x.WorkEffort.Gratuities ?? 0m,
                        Deductions = x.WorkEffort.Deductions ?? 0m,
                        DeductionDescription = x.WorkEffort.DeductionDescription
                    })
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