using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Orders.Returns;

public class ListReturns
{
    public class Query : IRequest<IQueryable<ReturnRecord>>
    {
        public ODataQueryOptions<ReturnRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<ReturnRecord>>
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

        public async Task<IQueryable<ReturnRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from ret in _context.ReturnHeaders
                join pty in _context.Parties on ret.FromPartyId equals pty.PartyId
                join rett in _context.ReturnHeaderTypes on ret.ReturnHeaderTypeId equals rett.ReturnHeaderTypeId
                join sts in _context.StatusItems on ret.StatusId equals sts.StatusId
                join pty2 in _context.Parties on ret.ToPartyId equals pty2.PartyId
                select new ReturnRecord
                {
                    ReturnId = ret.ReturnId,
                    FromPartyId = new OrderPartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description ?? string.Empty
                    },
                    FromPartyName = pty.Description,
                    ToPartyId = new OrderPartyDto
                    {
                        FromPartyId = pty2.PartyId,
                        FromPartyName = pty2.Description ?? string.Empty
                    },
                    ToPartyName = pty2.Description,
                    EntryDate = ret.EntryDate,
                    StatusId = sts.StatusId,
                    StatusDescription = sts.Description,
                    ReturnHeaderTypeId = rett.ReturnHeaderTypeId,
                    ReturnHeaderTypeDescription = rett.Description,
                    DestinationFacilityId = ret.DestinationFacilityId,
                    CurrencyUomId = ret.CurrencyUomId
                };

            return query;
        }
    }
}