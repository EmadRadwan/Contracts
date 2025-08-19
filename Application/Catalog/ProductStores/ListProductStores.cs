using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.ProductStores;

public class ListProductStores
{
    public class Query : IRequest<IQueryable<ProductStoreRecord>>
    {
        public ODataQueryOptions<ProductStoreRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<ProductStoreRecord>>
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

        public Task<IQueryable<ProductStoreRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var productStores = _context.ProductStores
                .Join(_context.Parties, ps => ps.PayToPartyId, s => s.PartyId, (ps, s) => new { ps, s })
                .OrderByDescending(x => x.ps.CreatedStamp)
                .Select(x => new ProductStoreRecord
                {
                    ProductStoreId = x.ps.ProductStoreId,
                    StoreName = x.ps.StoreName,
                    CompanyName = x.ps.CompanyName,
                    Title = x.ps.Title,
                    PayToPartyId = x.ps.PayToPartyId,
                    PayToPartyName = x.s.Description,
                    InventoryFacilityName = x.ps.InventoryFacility.FacilityName,
                    DefaultCurrencyUomId = x.ps.DefaultCurrencyUomId,
                    ReserveOrderEnumId = x.ps.ReserveOrderEnumId
                });

            return Task.FromResult(productStores);
        }
    }
}