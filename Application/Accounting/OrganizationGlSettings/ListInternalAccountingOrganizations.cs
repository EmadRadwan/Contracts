using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class ListInternalAccountingOrganizations
{
    public class Query : IRequest<IQueryable<InternalAccountingOrganizationRecord>>
    {
        public ODataQueryOptions<InternalAccountingOrganizationRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<InternalAccountingOrganizationRecord>>
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

        public async Task<IQueryable<InternalAccountingOrganizationRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = from p in _context.Parties
                join pap in _context.PartyAcctgPreferences on p.PartyId equals pap.PartyId
                join pr in _context.PartyRoles on p.PartyId equals pr.PartyId
                where pap.EnableAccounting == "Y" && pr.RoleTypeId == "INTERNAL_ORGANIZATIO"
                select new InternalAccountingOrganizationRecord
                {
                    PartyId = p.PartyId,
                    PartyName = p.Description
                };

            return query;
        }
    }
}