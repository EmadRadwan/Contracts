using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetServiceProductPrice
{
    public class Query : IRequest<Result<decimal>>
    {
        public ServiceProductPriceParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<decimal>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor,
            ILogger<Handler> logger, IProductService productService)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
            _productService = productService;
        }

        public async Task<Result<decimal>> Handle(Query request, CancellationToken cancellationToken)
        {
            var serviceProductPrice =
                await _productService.CalculateServicePrice(request.Params.ProductId, request.Params.VehicleId);

            return Result<decimal>.Success(serviceProductPrice);
        }
    }
}