using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.Agreement;

public class ListAgreements
{
    public class Query : IRequest<IQueryable<AgreementRecord>>
    {
        public ODataQueryOptions<AgreementRecord> Options { get; set; }
        public string Language { get; set; }  // Add Language property
    }

    public class Handler : IRequestHandler<Query, IQueryable<AgreementRecord>>
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

        public async Task<IQueryable<AgreementRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;  // Access the language from the request

            var query = (from agr in _context.Agreements
                         join pty in _context.Parties on agr.PartyIdFrom equals pty.PartyId
                         join pty2 in _context.Parties on agr.PartyIdTo equals pty2.PartyId
                         join rt in _context.RoleTypes on agr.RoleTypeIdFrom equals rt.RoleTypeId into rtGroupFrom
                         from rt in rtGroupFrom.DefaultIfEmpty() // Left join for RoleTypeIdFrom
                         join rt2 in _context.RoleTypes on agr.RoleTypeIdTo equals rt2.RoleTypeId into rtGroupTo
                         from rt2 in rtGroupTo.DefaultIfEmpty() // Left join for RoleTypeIdTo
                         join at in _context.AgreementTypes on agr.AgreementTypeId equals at.AgreementTypeId
                         where agr.ThruDate == null
                         select new AgreementRecord
                         {
                             AgreementId = agr.AgreementId,
                             PartyIdFrom = agr.PartyIdFrom,
                             PartyIdFromName = pty.Description,
                             PartyIdTo = agr.PartyIdTo,
                             PartyIdToName = pty2.Description,
                             RoleTypeIdFrom = agr.RoleTypeIdFrom,
                             RoleTypeIdFromDescription = rt != null ? rt.Description : null, // Handle null for RoleTypeIdFrom
                             RoleTypeIdTo = agr.RoleTypeIdTo,
                             RoleTypeIdToDescription = rt2 != null ? rt2.Description : null,  // Handle null for RoleTypeIdTo
                             AgreementTypeId = agr.AgreementTypeId,
                             AgreementTypeIdDescription = language == "en" ? at.Description : at.DescriptionArabic,
                             AgreementDate = agr.AgreementDate,
                             FromDate = agr.FromDate,
                             ThruDate = agr.ThruDate,
                             Description = agr.Description
                         }).AsQueryable();

            return query;
        }
        
    }
}
