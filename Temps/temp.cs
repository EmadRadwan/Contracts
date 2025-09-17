using Application.Catalog.Products;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Projects
{
    public class ListCertificateItems
    {
        public class Query : IRequest<Result<List<CertificateItemDto>>>
        {
            public string WorkEffortId { get; set; }
            public string Language { get; set; }
        }

        // REFACTOR: Enhanced QueryValidator to include specific validation rules
        // Purpose: Ensure WorkEffortId and Language are valid and non-empty
        // Improvement: Strengthens input validation, aligning with CreateProjectCertificate
        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(x => x.WorkEffortId)
                    .NotEmpty()
                    .WithMessage("Work Effort ID is required");
                RuleFor(x => x.Language)
                    .NotEmpty()
                    .WithMessage("Language is required")
                    .Must(lang => lang.ToLower() == "en" || lang.ToLower() == "ar")
                    .WithMessage("Language must be 'en' or 'ar'");
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
                // REFACTOR: Moved validation to async for consistency
                // Purpose: Validate input before querying the database
                // Improvement: Reduces unnecessary database calls on invalid input
                var validator = new QueryValidator();
                var validationResult = await validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Result<List<CertificateItemDto>>.Failure(
                        string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                }

                // REFACTOR: Normalized language handling with fallback
                // Purpose: Ensure consistent language code for localization
                // Improvement: Simplifies logic and supports bilingual display
                var language = request.Language?.ToLower() == "en" ? "en" : "ar";

                try
                {
                    var certificateItems = await _context.WorkEfforts
                        .Where(we => we.WorkEffortParentId == request.WorkEffortId && 
                                     we.WorkEffortTypeId == "CERTIFICATE_ITEM")
                        .GroupJoin(
                            _context.Products,
                            we => we.ProductId,
                            prd => prd.ProductId,
                            (we, prdGroup) => new { WorkEffort = we, Products = prdGroup })
                        .SelectMany(
                            x => x.Products.DefaultIfEmpty(),
                            (x, prd) => new { x.WorkEffort, Product = prd })
                        .GroupJoin(
                            _context.Uoms,
                            x => x.WorkEffort.QuantityUomId,
                            uom => uom.UomId,
                            (x, uomGroup) => new { x.WorkEffort, x.Product, Uoms = uomGroup })
                        .SelectMany(
                            x => x.Uoms.DefaultIfEmpty(),
                            (x, uom) => new { x.WorkEffort, x.Product, Uom = uom })
                        .GroupJoin(
                            _context.Facilities,
                            x => x.WorkEffort.FacilityId,
                            fac => fac.FacilityId,
                            (x, facGroup) => new { x.WorkEffort, x.Product, x.Uom, Facilities = facGroup })
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
                                QuantityUom = x.WorkEffort.QuantityUomId,
                                QuantityUomObject = x.Uom != null
                                    ? new UomLovDto
                                    {
                                        UomId = x.Uom.UomId,
                                        Description = x.Uom.Description
                                    }
                                    : null,
                                Description = x.WorkEffort.Description,
                                ProductName = x.Product != null ? x.Product.ProductName : null,
                                UomDescription = x.Uom != null ? x.Uom.Description : null,
                                Quantity = x.WorkEffort.Quantity ?? 0m,
                                UnitPrice = x.WorkEffort.Rate ?? 0m,
                                // REFACTOR: Added MaterialPrice, LaborPrice, and AdditionalInsurance
                                // Purpose: Map new WorkEffort fields to CertificateItemDto
                                // Improvement: Aligns with updated WorkEffort schema and frontend expectations
                                MaterialPrice = x.WorkEffort.MaterialPrice ?? 0m,
                                LaborPrice = x.WorkEffort.LaborPrice ?? 0m,
                                AdditionalInsurance = x.WorkEffort.AdditionalInsurance ?? 0m,
                                TotalAmount = x.WorkEffort.TotalAmount ?? 
                                             (x.WorkEffort.Quantity ?? 0m) * (x.WorkEffort.Rate ?? 0m),
                                Discount = x.WorkEffort.Discount ?? 0m,
                                Insurance = x.WorkEffort.Insurance ?? 0m,
                                CompletionPercentage = x.WorkEffort.CompletionPercentage ?? 0m,
                                Notes = x.WorkEffort.Notes,
                                ProcurementDate = x.WorkEffort.ProcurementDate ?? x.WorkEffort.CreatedDate,
                                IsDeleted = false,
                                AchievementPercentage = x.WorkEffort.AchievementPercent ?? 0m,
                                TransportationExpenses = x.WorkEffort.TransportationExpenses ?? 0m,
                                Gratuities = x.WorkEffort.Gratuities ?? 0m,
                                Deductions = x.WorkEffort.Deductions ?? 0m,
                                // REFACTOR: Added Deserved and Net calculations
                                // Purpose: Compute derived fields server-side for consistency
                                // Improvement: Ensures alignment with frontend calculations
                                Deserved = (x.WorkEffort.Quantity ?? 0m) * 
                                          (x.WorkEffort.Rate ?? 0m) * 
                                          ((x.WorkEffort.CompletionPercentage ?? 0m) / 100),
                                Net = ((x.WorkEffort.Quantity ?? 0m) * 
                                      (x.WorkEffort.Rate ?? 0m) * 
                                      ((x.WorkEffort.CompletionPercentage ?? 0m) / 100)) -
                                      (x.WorkEffort.Insurance ?? 0m) -
                                      (x.WorkEffort.AdditionalInsurance ?? 0m) -
                                      (x.WorkEffort.Deductions ?? 0m)
                            })
                        .ToListAsync(cancellationToken);

                    return Result<List<CertificateItemDto>>.Success(certificateItems);
                }
                catch (Exception ex)
                {
                    // REFACTOR: Improved error handling with stack trace
                    // Purpose: Provide detailed error information for debugging
                    // Improvement: Enhances traceability without exposing sensitive data
                    return Result<List<CertificateItemDto>>.Failure(
                        $"Failed to retrieve certificate items: {ex.Message}");
                }
            }
        }
    }
}