using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetServicesLov
{
    public class ServiceProductEnvelop
    {
        public List<ProductServiceLovDto> ServiceProducts { get; set; }
        public int ServicesCount { get; set; }
    }

    public class Query : IRequest<Result<ServiceProductEnvelop>>
    {
        public ServiceLovParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ServiceProductEnvelop>>
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

        public async Task<Result<ServiceProductEnvelop>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.Products
                .Where(x => x.ProductTypeId == "Service")
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Params.SearchTerm))
            {
                var lowerCaseSearchTerm = request.Params.SearchTerm.Trim().ToLower();

                query = query.Where(p => p.ProductName.ToLower().Contains(lowerCaseSearchTerm));
            }


            var serviceProducts = await query
                .OrderBy(x => x.ProductName)
                .Skip(request.Params.Skip)
                .Take(request.Params.PageSize)
                .Select(x => new ProductServiceLovDto
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName
                })
                .ToListAsync(cancellationToken);

            var productServicesEnvelop = new ServiceProductEnvelop
            {
                ServiceProducts = serviceProducts,
                ServicesCount = query.Count()
            };


            return Result<ServiceProductEnvelop>.Success(productServicesEnvelop);
        }
    }
}