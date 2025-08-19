using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetSalesProductsLov
{
    public class ProductLovDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductFeatureId { get; set; }
        public string ColorDescription { get; set; }
        public decimal QuantityOnHandTotal { get; set; }
        public decimal AvailableToPromiseTotal { get; set; }
        public string QuantityUom { get; set; }
        public string UomDescription { get; set; }
    }

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

        public async Task<Result<ProductsEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;
            // REFACTOR: Simplified query to avoid complex expressions and improve EF Core compatibility
            // Purpose: Reduces complexity in the projection and where clause to prevent InvalidOperationException
            // Context: Avoids issues with Distinct and complex conditionals by using simpler SQL-translatable expressions
            var query = (from prd in _context.Products
                         join uom in _context.Uoms
                             on prd.QuantityUomId equals uom.UomId
                         join iif in _context.InventoryItemFeatures
                             on prd.ProductId equals iif.ProductId into iifGroup
                         from iif in iifGroup.DefaultIfEmpty()
                         join pf in _context.ProductFeatures.Where(pf => pf.ProductFeatureTypeId == "COLOR")
                             on iif != null ? iif.ProductFeatureId : null equals pf.ProductFeatureId into pfGroup
                         from pf in pfGroup.DefaultIfEmpty()
                         join inv in _context.InventoryItems
                             on iif != null ? iif.InventoryItemId : null equals inv.InventoryItemId into invGroup
                         from inv in invGroup.DefaultIfEmpty()
                         where prd.ProductTypeId == "FINISHED_GOOD"
                         select new
                         {
                             ProductId = prd.ProductId,
                             ProductName = prd.ProductName,
                             ProductFeatureId = pf != null ? pf.ProductFeatureId : null,
                             ColorDescription = pf != null
                                 ? (language == "ar" ? pf.DescriptionArabic ?? pf.Description : pf.Description)
                                 : null,
                             QuantityOnHandTotal = inv != null && inv.QuantityOnHandTotal.HasValue
                                 ? inv.QuantityOnHandTotal.Value
                                 : 0m,
                             AvailableToPromiseTotal = inv != null && inv.AvailableToPromiseTotal.HasValue
                                 ? inv.AvailableToPromiseTotal.Value
                                 : 0m,
                             QuantityUom = uom.UomId,
                             UomDescription = language == "ar" ? uom.DescriptionArabic : uom.Description
                         });

            // REFACTOR: Apply SearchTerm filter separately to avoid complex conditional in where clause
            // Purpose: Simplifies SQL translation by moving client-side logic out of the query
            if (request.Params != null && !string.IsNullOrEmpty(request.Params.SearchTerm))
            {
                query = query.Where(p => p.ProductName.Contains(request.Params.SearchTerm));
            }

            // REFACTOR: Materialize query results and apply Distinct on the client side
            // Purpose: Avoids EF Core issues with Distinct on complex projections
            var products = await query
                .OrderBy(p => p.ProductName)
                .ThenBy(p => p.ColorDescription ?? string.Empty)
                .Skip(request.Params != null ? request.Params.Skip : 0)
                .Take(request.Params != null ? request.Params.PageSize : 10)
                .ToListAsync(cancellationToken);

            // REFACTOR: Convert anonymous type to ProductLovDto and handle ProductFeatureId formatting
            // Purpose: Moves ToString() conversion to client-side to ensure SQL compatibility
            var productDtos = products
                .Select(p => new ProductLovDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductFeatureId = p.ProductFeatureId != null ? p.ProductFeatureId.ToString() : string.Empty,
                    ColorDescription = p.ColorDescription,
                    QuantityOnHandTotal = p.QuantityOnHandTotal,
                    AvailableToPromiseTotal = p.AvailableToPromiseTotal,
                    QuantityUom = p.QuantityUom,
                    UomDescription = p.UomDescription
                })
                .DistinctBy(p => new { p.ProductId, p.ProductFeatureId }) // Use DistinctBy for client-side deduplication
                .ToList();

            // REFACTOR: Compute count separately to isolate potential query issues
            // Purpose: Ensures CountAsync works by using a simpler query
            var productCount = await query.CountAsync(cancellationToken);

            var productEnvelop = new ProductsEnvelope
            {
                Products = productDtos,
                ProductCount = productCount
            };

            return Result<ProductsEnvelope>.Success(productEnvelop);
        }
    }
}