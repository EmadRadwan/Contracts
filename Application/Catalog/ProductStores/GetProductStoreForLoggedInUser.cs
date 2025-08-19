using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductStores;

public class GetProductStoreForLoggedInUser
{
    public class Query : IRequest<Result<ProductStore>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<ProductStore>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IProductStoreService _productStoreService;

        public Handler(DataContext context, IMapper mapper, IProductStoreService productStoreService)
        {
            _context = context;
            _mapper = mapper;
            _productStoreService = productStoreService;
        }

        public async Task<Result<ProductStore>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                // Fetch the ProductStore for the logged-in user
                var productStore = await _productStoreService.GetProductStoreForLoggedInUser();

                if (productStore == null)
                {
                    return Result<ProductStore>.Failure("Product store not found for the logged-in user.");
                }

                return Result<ProductStore>.Success(productStore);
            }
            catch (Exception ex)
            {
                return Result<ProductStore>.Failure($"An error occurred: {ex.Message}");
            }
        }
    }
}
