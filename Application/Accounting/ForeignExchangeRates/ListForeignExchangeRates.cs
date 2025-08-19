using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.ForeignExchangeRates;

public class ListForeignExchangeRates
{
    public class Query : IRequest<IQueryable<UomConversionDatedRecord>>
    {
        public ODataQueryOptions<UomConversionDatedRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<UomConversionDatedRecord>>
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

        public async Task<IQueryable<UomConversionDatedRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = from UomConversionDated in _context.UomConversionDateds
                join uomFrom in _context.Uoms on UomConversionDated.UomId equals uomFrom.UomId
                join uomTo in _context.Uoms on UomConversionDated.UomIdTo equals uomTo.UomId
                select new UomConversionDatedRecord
                {
                    UomId = UomConversionDated.UomId,
                    UomIdDescription = uomFrom.Description,
                    UomIdTo = UomConversionDated.UomIdTo,
                    UomIdToDescription = uomTo.Description,
                    FromDate = UomConversionDated.FromDate,
                    ThruDate = UomConversionDated.ThruDate,
                    ConversionFactor = UomConversionDated.ConversionFactor,
                    CustomMethodId = UomConversionDated.CustomMethodId,
                    DecimalScale = UomConversionDated.DecimalScale,
                    RoundingMode = UomConversionDated.RoundingMode,
                    PurposeEnumId = UomConversionDated.PurposeEnumId
                };

            return query;
        }
    }
}