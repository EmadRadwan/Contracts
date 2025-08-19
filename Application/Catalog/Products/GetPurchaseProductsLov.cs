using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetPurchaseProductsLov
{
    public class ProductsEnvelope
    {
        public List<ProductLovDto> Products { get; set; }
        public int ProductCount { get; set; }
    }

    public class Query : IRequest<Result<ProductsEnvelope>>
    {
        public ProductLovParams? Params { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ProductsEnvelope>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        private string GetLocalizedDescription(string language, string defaultDescription, string? arabicDescription)
        {
            return language == "ar" ? arabicDescription ?? defaultDescription : defaultDescription;
        }

        public async Task<Result<ProductsEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                // Purpose: Prevents null reference exceptions
                if (request?.Params == null || string.IsNullOrEmpty(request.Params.SupplierId))
                {
                    _logger.LogWarning("Invalid request: Params or SupplierId is null");
                    return Result<ProductsEnvelope>.Failure("Invalid request parameters.");
                }

                var language = request.Language ?? "en";
                // Purpose: Ensures consistent behavior if Language is null

                // REFACTOR: Modified query to deduplicate InventoryItemFeatures by selecting distinct ProductFeatureId
                // Why: Prevents multiple records for the same color feature (e.g., COLOR_BLACK) for a single product
                var tempQuery = (from prd in _context.Products
                    join prs in _context.SupplierProducts
                        on prd.ProductId equals prs.ProductId
                    // REFACTOR: Added subquery to get distinct InventoryItemFeatures per ProductId and ProductFeatureId
                    // Why: Ensures only one feature per color is selected to avoid duplication
                    join iif in (from iif in _context.InventoryItemFeatures
                            group iif by new { iif.ProductId, iif.ProductFeatureId }
                            into g
                            select new { g.Key.ProductId, g.Key.ProductFeatureId })
                        on prd.ProductId equals iif.ProductId into iifGroup
                    from iif in iifGroup.DefaultIfEmpty()
                    join pf in _context.ProductFeatures.Where(pf => pf.ProductFeatureTypeId == "COLOR")
                        on iif != null ? iif.ProductFeatureId : null equals pf.ProductFeatureId into pfGroup
                    from pf in pfGroup.DefaultIfEmpty()
                    join uom in _context.Uoms on prd.QuantityUomId equals uom.UomId
                    where prd.ProductTypeId != "SERVICE_PRODUCT" &&
                          prs.PartyId == request.Params.SupplierId &&
                          prs.AvailableThruDate ==
                          null && // REFACTOR: Explicitly filter for active SupplierProducts record
                          // Why: Ensures only the latest active supplier record is used
                          (string.IsNullOrEmpty(request.Params.SearchTerm) ||
                           prd.ProductName.Contains(request.Params.SearchTerm))
                    select new
                    {
                        ProductId = prd.ProductId,
                        BaseProductName = prd.ProductName,
                        ProductFeatureId = pf != null ? pf.ProductFeatureId : null,
                        ColorDescription = pf != null ? pf.Description : null,
                        ColorDescriptionArabic = pf != null ? pf.DescriptionArabic : null,
                        QuantityUom = uom.UomId,
                        UomDescription = uom.Description,
                        UomDescriptionArabic = uom.DescriptionArabic,
                        LastPrice = prs.LastPrice
                    }).AsQueryable();

                // Fetch results and apply localization
                var tempResults = await tempQuery
                    .OrderBy(p => p.BaseProductName)
                    .ThenBy(p => p.ColorDescription ?? string.Empty)
                    // Purpose: Avoids calling GetLocalizedDescription in LINQ
                    // Why: Prevents EF Core translation errors
                    .Skip(request.Params.Skip)
                    .Take(request.Params.PageSize)
                    .ToListAsync(cancellationToken);

                // Map to ProductLovDto with localization
                var products = tempResults.Select(r => new ProductLovDto
                {
                    ProductId = r.ProductId,
                    ProductName = r.ColorDescription != null
                        ? $"{r.BaseProductName} ({GetLocalizedDescription(language, r.ColorDescription, r.ColorDescriptionArabic)})"
                        : r.BaseProductName,
                    // Purpose: Moves language logic to C# to avoid translation issues
                    ProductFeatureId = r.ProductFeatureId,
                    ColorDescription = r.ColorDescription != null
                        ? GetLocalizedDescription(language, r.ColorDescription, r.ColorDescriptionArabic)
                        : null,
                    QuantityUom = r.QuantityUom,
                    UomDescription = GetLocalizedDescription(language, r.UomDescription, r.UomDescriptionArabic),
                    LastPrice = r.LastPrice
                }).ToList();

                var productEnvelop = new ProductsEnvelope
                {
                    Products = products,
                    ProductCount = await tempQuery.CountAsync(cancellationToken)
                    // Purpose: Ensures async consistency
                };

                // Purpose: Tracks query execution details
                _logger.LogInformation(
                    "Retrieved {ProductCount} product entries for purchase order, {ColorVariantCount} with color variants for SupplierId {SupplierId}, Language {Language}",
                    productEnvelop.ProductCount,
                    products.Count(p => p.ProductFeatureId != null),
                    request.Params.SupplierId,
                    language);

                return Result<ProductsEnvelope>.Success(productEnvelop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving purchase products for SupplierId {SupplierId}, Language {Language}",
                    request?.Params?.SupplierId, request?.Language);
                return Result<ProductsEnvelope>.Failure("Failed to retrieve purchase products.");
            }
        }
    }
}