using Application.Uoms;
using Microsoft.EntityFrameworkCore;
using Persistence;
using MediatR;
using Application.Catalog.ProductStores;

namespace Application.Shipments.OrganizationGlSettings;

public class GetBaseCurrencyId
{
    public class Query : IRequest<Result<CurrencyDto>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<CurrencyDto>>
    {
        private readonly IProductStoreService _productStoreService;
        private readonly DataContext _context;

        public Handler(DataContext context, IProductStoreService productStoreService)
        {
            _context = context;
            _productStoreService = productStoreService;
        }

        public async Task<Result<CurrencyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = await _productStoreService.GetAcctgBaseCurrencyId();
            return Result<CurrencyDto>.Success(result);
        }
    }
}