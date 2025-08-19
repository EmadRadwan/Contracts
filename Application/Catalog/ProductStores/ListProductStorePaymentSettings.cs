using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductStores
{
    public class ListProductStorePaymentSettings
    {
        public class Query : IRequest<Result<List<ProductStorePaymentSettingDto>>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<List<ProductStorePaymentSettingDto>>>
        {
            private readonly IProductStoreService _productStoreService;

            public Handler(DataContext context, IMapper mapper, IProductStoreService productStoreService)
            {
                _productStoreService = productStoreService;
            }

            public async Task<Result<List<ProductStorePaymentSettingDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Get product store payment settings using the service
                var ProductStorePaymentSettingDtos = await _productStoreService.GetProductStorePaymentSettings();

                if (ProductStorePaymentSettingDtos == null || !ProductStorePaymentSettingDtos.Any())
                {
                    return Result<List<ProductStorePaymentSettingDto>>.Failure("No payment settings found.");
                }

                
                return Result<List<ProductStorePaymentSettingDto>>.Success(ProductStorePaymentSettingDtos);
            }
        }
    }
}