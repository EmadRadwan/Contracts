using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class GetServiceRates
{
    public class Query : IRequest<Result<List<ServiceRateDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<ServiceRateDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ServiceRateDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var serviceRates = await _context.ServiceRates
                .Include(x => x.MakeProductCategory)
                .Include(x => x.ModelProductCategory)
                .Include(x => x.ProductStore)
                .Select(x => new ServiceRateDto
                {
                    ServiceRateId = x.ServiceRateId,
                    MakeId = x.MakeId,
                    MakeDescription = x.MakeProductCategory.Description,
                    ModelId = x.ModelId,
                    ModelDescription = x.ModelProductCategory.Description,
                    ProductStoreId = x.ProductStoreId,
                    ProductStoreName = x.ProductStore.StoreName,
                    FromDate = x.FromDate,
                    ThruDate = x.ThruDate,
                    Rate = x.Rate
                }).ToListAsync(cancellationToken);

            return Result<List<ServiceRateDto>>.Success(serviceRates);
        }
    }
}