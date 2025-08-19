using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetJobQuoteProductsLov
{
    public class ProductsEnvelope
    {
        public List<ProductLovDto> Products { get; set; }
        public int ProductCount { get; set; }
    }

    public class Query : IRequest<Result<ProductsEnvelope>>
    {
        public ProductLovParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ProductsEnvelope>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger,
            IProductService productService)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
            _productService = productService;
        }

        public async Task<Result<ProductsEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = (from prd in _context.Products
                join inv in _context.InventoryItems on prd.ProductId equals inv.ProductId into invGroup
                from inv in invGroup.DefaultIfEmpty()
                join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId into facGroup
                from fac in facGroup.DefaultIfEmpty()
                join uoms in _context.Uoms on prd.QuantityUomId equals uoms.UomId
                where request.Params.SearchTerm == null || prd.ProductName.Contains(request.Params.SearchTerm)
                group new { prd, inv, fac, uoms } by new
                {
                    prd.ProductId,
                    prd.ProductName,
                    prd.QuantityUomId,
                    uoms.Description,
                    FacilityName = fac != null ? fac.FacilityName : null, // Handle potential null Facility
                    prd.ProductTypeId
                }
                into grp
                
                select new ProductLovDto
                {
                    ProductId = grp.Key.ProductId,
                    ProductName = grp.Key.ProductName,
                    ProductTypeId = grp.Key.ProductTypeId,
                    FacilityName = grp.Key.FacilityName,
                    QuantityUom = grp.Key.QuantityUomId,
                    UomDescription = grp.Key.Description,
                    QuantityOnHandTotal = grp.Sum(g => g.inv != null ? g.inv.QuantityOnHandTotal : 0),
                    AvailableToPromiseTotal = grp.Sum(g => g.inv != null ? g.inv.AvailableToPromiseTotal : 0),

                    Price = Math.Round(
                        grp.Key.ProductTypeId == "FINISHED_GOOD"
                            ? _context.ProductPrices
                                .Where(prc =>
                                    prc.ProductId == grp.Key.ProductId && prc.ProductPriceTypeId == "DEFAULT_PRICE")
                                .OrderByDescending(prc => prc.FromDate)
                                .Select(prc => prc.Price)
                                .FirstOrDefault() ?? 0M
                            : 0M,
                        2),
                    ListPrice = Math.Round(
                        grp.Key.ProductTypeId == "FINISHED_GOOD"
                            ? _context.ProductPrices
                                .Where(prc =>
                                    prc.ProductId == grp.Key.ProductId && prc.ProductPriceTypeId == "LIST_PRICE")
                                .OrderByDescending(prc => prc.FromDate)
                                .Select(prc => prc.Price)
                                .FirstOrDefault() ?? 0M
                            : 0M,
                        2)
                }).AsQueryable();


            var products = await query
                .Skip(request.Params.Skip)
                .Take(request.Params.PageSize)
                .ToListAsync();


            foreach (var product in products)
                if (product.ProductTypeId == "SERVICE_PRODUCT")
                {
                    // Calculate the service price for each product
                    var servicePrice =
                        await _productService.CalculateServicePrice(product.ProductId, request.Params.VehicleId);
                    product.Price = Math.Round(servicePrice, 2); // Round the service price to 2 decimal places
                }
                else if (product.ProductTypeId == "MARKETING_PKG")
                {
                    // Calculate the price for a marketing package based on its components
                    CalculateMarketingPackagePrice(product, request.Params.VehicleId);
                }

            var productEnvelope = new ProductsEnvelope
            {
                Products = products,
                ProductCount = query.Count()
            };
            return Result<ProductsEnvelope>.Success(productEnvelope);
        }

        private ProductLovDto CalculateMarketingPackagePrice(ProductLovDto product, string vehicleId)
        {
            var packageRecords = (from assoc in _context.ProductAssocs
                    join pkg in _context.Products on assoc.ProductIdTo equals pkg.ProductId
                    where assoc.ProductId == product.ProductId
                    select pkg)
                .ToList();

            var relatedRecords = new List<ProductLovDto>();
            decimal calculatedPrice = 0;
            decimal componentPrice = 0;

            foreach (var packageRecord in packageRecords)
                if (packageRecord.ProductTypeId == "FINISHED_GOOD")
                {
                    componentPrice = _context.ProductPrices
                        .Where(prc =>
                            prc.ProductId == packageRecord.ProductId && prc.ProductPriceTypeId == "DEFAULT_PRICE")
                        .OrderByDescending(prc => prc.FromDate)
                        .Select(prc => prc.Price)
                        .FirstOrDefault() ?? 0;

                    componentPrice = Math.Round(componentPrice, 2); // Round component price to 2 decimal places

                    calculatedPrice += componentPrice;

                    var inventoryGroup = (from inv in _context.InventoryItems
                        join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId into facGroup
                        from fac in facGroup.DefaultIfEmpty()
                        where inv.ProductId == packageRecord.ProductId
                        group new { inv, fac } by new
                        {
                            FacilityName = fac != null ? fac.FacilityName : null
                        }
                        into grp
                        select new
                        {
                            grp.Key.FacilityName,
                            QuantityOnHandTotal = grp.Sum(g => g.inv != null ? g.inv.QuantityOnHandTotal : 0),
                            AvailableToPromiseTotal = grp.Sum(g => g.inv != null ? g.inv.AvailableToPromiseTotal : 0)
                        }).FirstOrDefault();

                    var componentProduct = new ProductLovDto
                    {
                        ProductId = packageRecord.ProductId,
                        ProductName = packageRecord.ProductName,
                        Price = componentPrice, // Individual price for the component
                        FacilityName = inventoryGroup?.FacilityName,
                        QuantityOnHandTotal = inventoryGroup?.QuantityOnHandTotal ?? 0,
                        AvailableToPromiseTotal = inventoryGroup?.AvailableToPromiseTotal ?? 0
                    };

                    relatedRecords.Add(componentProduct);
                }
                else if (packageRecord.ProductTypeId == "SERVICE_PRODUCT")
                {
                    componentPrice += _productService.CalculateServicePrice(packageRecord.ProductId, vehicleId).Result;

                    componentPrice = Math.Round(componentPrice, 2); // Round component price to 2 decimal places

                    calculatedPrice += componentPrice;

                    var componentProduct = new ProductLovDto
                    {
                        ProductId = packageRecord.ProductId,
                        ProductName = packageRecord.ProductName,
                        Price = componentPrice // Individual price for the component
                    };

                    relatedRecords.Add(componentProduct);
                }

            calculatedPrice = Math.Round(calculatedPrice, 2); // Round the final calculated package price

            product.RelatedRecords = relatedRecords;
            product.Price = calculatedPrice;
            return product;
        }
    }
}