using Application.Core;
using Application.Interfaces;
using Application.Order.Orders;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Quotes;

public class ListQuotes
{
    public class Query : IRequest<IQueryable<QuoteRecord>>
    {
        public ODataQueryOptions<QuoteRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<QuoteRecord>>
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

        public async Task<IQueryable<QuoteRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from qot in _context.Quotes
                join pty in _context.Parties on qot.PartyId equals pty.PartyId
                join sts in _context.StatusItems on qot.StatusId equals sts.StatusId
                join pcm in _context.PartyContactMeches on pty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                select new QuoteRecord
                {
                    QuoteId = qot.QuoteId,
                    GrandTotal = qot.GrandTotal,
                    CurrencyUomId = qot.CurrencyUomId,
                    
                    StatusDescription = sts.Description,
                    FromPartyId = new OrderPartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description
                    },
                    FromPartyName = pty.Description + " ( " + tn.ContactNumber + " )",
                    CustomerRemarks = qot.CustomerRemarks,
                    InternalRemarks = qot.InternalRemarks,
                    IssueDate = DateTime.SpecifyKind(qot.IssueDate.Truncate(TimeSpan.FromSeconds(1)), DateTimeKind.Utc),
                    ValidThruDate = DateTime.SpecifyKind(qot.ValidThruDate.Truncate(TimeSpan.FromSeconds(1)),
                        DateTimeKind.Utc)
                };

            return query;
        }
    }
}