using Application.Catalog.ProductStores;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Catalog.Products;

public class GetAvailableProductPromotions
{
    public class Query : IRequest<Result<List<AvailableProductPromoDto>>>
    {
        public string ProductId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<AvailableProductPromoDto>>>
    {
        private readonly ILogger _logger;
        private readonly IProductStoreService _productStoreService;

        public Handler(IProductStoreService productStoreService, ILogger<GetAvailableProductPromotions> logger)
        {
            _productStoreService = productStoreService;
            _logger = logger;
        }


        public async Task<Result<List<AvailableProductPromoDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        { 
            _logger.LogInformation(
                "GetAvailableProductPromotions.QueryHandler.Handle - Retrieving available product promotions for product {ProductId}",
                request.ProductId);

            var promoDataByProduct = await _productStoreService.GetAvailableProductPromotions(request.ProductId);


            return Result<List<AvailableProductPromoDto>>.Success(promoDataByProduct);
        }
    }
}