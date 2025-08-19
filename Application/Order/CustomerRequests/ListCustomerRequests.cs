using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.CustomerRequests;

public class ListCustomerRequests
{
    public class Query : IRequest<Result<PagedList<CustRequestDto>>>
    {
        public CustomerRequestParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PagedList<CustRequestDto>>>
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

        public async Task<Result<PagedList<CustRequestDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = (from rqst in _context.CustRequests
                join pty in _context.Parties on rqst.FromPartyId equals pty.PartyId
                join crt in _context.CustRequestTypes on rqst.CustRequestTypeId equals crt.CustRequestTypeId
                join sts in _context.StatusItems on rqst.StatusId equals sts.StatusId
                //join enm in _context.Enumerations on rqst.SalesChannelEnumId equals enm.EnumId
                join pcm in _context.PartyContactMeches on pty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where (pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                       && request.Params.SearchTerm == null) || tn.ContactNumber.Contains(request.Params.SearchTerm)
                select new CustRequestDto
                {
                    CustRequestId = rqst.CustRequestId,
                    FromPartyName = pty.Description + " ( " + tn.AreaCode + "-" + tn.ContactNumber + " )",
                    CustRequestDate = DateTime.SpecifyKind(rqst.CustRequestDate.Truncate(TimeSpan.FromSeconds(1)),
                        DateTimeKind.Utc)
                }).AsQueryable();

            return Result<PagedList<CustRequestDto>>.Success(
                await PagedList<CustRequestDto>.ToPagedList(query, request.Params.PageNumber,
                    request.Params.PageSize)
            );
        }
    }
}