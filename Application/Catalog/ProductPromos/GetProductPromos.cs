using Application.Catalog.ProductStores;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Catalog.ProductPromos;

public class GetProductPromos
{
    public class Query : IRequest<Result<List<ProductPromoDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<ProductPromoDto>>>
    {
        private readonly ILogger _logger;
        private readonly IProductStoreService _productStoreService;

        public Handler(IProductStoreService productStoreService, ILogger<GetProductPromos> logger)
        {
            _productStoreService = productStoreService;
            _logger = logger;
        }

        public async Task<Result<List<ProductPromoDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetProductPromos.QueryHandler.Handle - Retrieving product promotions");
            var result = _productStoreService.GetProductPromos();

            return Result<List<ProductPromoDto>>.Success(await result);
        }
    }
}