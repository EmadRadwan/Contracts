using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.Taxes;

public class ListTaxAuthorities
{
    public class Query : IRequest<IQueryable<TaxAuthorityRecord>>
    {
        public ODataQueryOptions<TaxAuthorityRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<TaxAuthorityRecord>>
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

        public async Task<IQueryable<TaxAuthorityRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = (from tax in _context.TaxAuthorities
                join geo in _context.Geos on tax.TaxAuthGeoId equals geo.GeoId
                join pty in _context.Parties on tax.TaxAuthPartyId equals pty.PartyId
                select new TaxAuthorityRecord
                {
                    TaxAuthGeoId = tax.TaxAuthGeoId,
                    TaxAuthGeoDescription = geo.GeoName,
                    TaxAuthPartyId = tax.TaxAuthPartyId,
                    TaxAuthPartyName = pty.Description,
                    RequireTaxIdForExemption = tax.RequireTaxIdForExemption,
                    TaxIdFormatPattern = tax.TaxIdFormatPattern,
                    IncludeTaxInPrice = tax.IncludeTaxInPrice
                }).AsQueryable();


            return query;
        }
    }
}