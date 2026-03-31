#nullable enable
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Parties.Parties;

public class ListParties
{
    public class Query : IRequest<IQueryable<PartyRecord>>
    {
        public ODataQueryOptions<PartyRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<PartyRecord>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<IQueryable<PartyRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var allowedRoles = new[] { "CUSTOMER", "SUPPLIER", "EMPLOYEE", "CONTRACTOR", "PREVIOUS_EMPLOYEE" };

            var query =
                from prty in _context.Parties

                // --------------------------------------
                // NEW FILTER: Only these MainRoles
                // --------------------------------------
                where allowedRoles.Contains(prty.MainRole)

                join pt in _context.PartyTypes
                    on prty.PartyTypeId equals pt.PartyTypeId

                // LEFT JOIN: PartyContactMech
                join pcm in _context.PartyContactMeches
                    on prty.PartyId equals pcm.PartyId into pcmGroup
                from pcm in pcmGroup.DefaultIfEmpty()

                // LEFT JOIN: ContactMech
                join cm in _context.ContactMeches
                    on pcm.ContactMechId equals cm.ContactMechId into cmGroup
                from cm in cmGroup.DefaultIfEmpty()

                // LEFT JOIN: TelecomNumber
                join tn in _context.TelecomNumbers
                    on cm.ContactMechId equals tn.ContactMechId into tnGroup
                from tn in tnGroup.DefaultIfEmpty()

                // LEFT JOIN: PartyContactMechPurpose
                join pcmp in _context.PartyContactMechPurposes
                    on new { PartyId = (string?)pcm.PartyId, ContactMechId = (string?)pcm.ContactMechId }
                    equals new { pcmp.PartyId, pcmp.ContactMechId } into pcmpGroup
                from pcmp in pcmpGroup.DefaultIfEmpty()

                // LEFT JOIN: ContactMechPurposeType
                join cmpt in _context.ContactMechPurposeTypes
                    on pcmp.ContactMechPurposeTypeId equals cmpt.ContactMechPurposeTypeId 
                    into cmptGroup
                from cmpt in cmptGroup.DefaultIfEmpty()

                // Keep parties with NO contact mech (cmpt == null)
                where cmpt == null ||
                      cmpt.ContactMechPurposeTypeId == "PRIMARY_PHONE" ||
                      cmpt.ContactMechPurposeTypeId == "GENERAL_LOCATION" ||
                      cmpt.ContactMechPurposeTypeId == "PRIMARY_EMAIL"

                select new { prty, pt, pcm, cm, tn, pcmp, cmpt };

            // Group and project back to PartyRecord
            var groupedResults =
                from result in query
                group result by result.prty.PartyId into partyGroup
                select new PartyRecord
                {
                    PartyId = partyGroup.Key,
                    Description = partyGroup.First().prty.Description + " ( " +
                                  partyGroup.First().prty.MainRole + " )",
                    PartyTypeDescription = partyGroup.First().pt.Description,

                    MobileContactNumber =
                        partyGroup.Where(x => x.tn != null && x.tn.ContactNumber != null)
                                  .Select(x => x.tn.ContactNumber)
                                  .FirstOrDefault(),

                    Address1 =
                        partyGroup.Where(x => x.cm != null && x.cm.PostalAddress != null)
                                  .Select(x => x.cm.PostalAddress.Address1)
                                  .FirstOrDefault(),

                    InfoString =
                        partyGroup.Where(x => x.cm != null && x.cm.InfoString != null)
                                  .Select(x => x.cm.InfoString)
                                  .FirstOrDefault()
                };

            return groupedResults;
        }
    }
}
