#nullable enable
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Parties.Parties;

public class PartiesList
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
            var query = from prty in _context.Parties
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId into tnGroup
                from tn in tnGroup.DefaultIfEmpty()
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals new
                    { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where
                    cmpt.ContactMechPurposeTypeId == "PRIMARY_PHONE" ||
                    cmpt.ContactMechPurposeTypeId == "GENERAL_LOCATION" ||
                    cmpt.ContactMechPurposeTypeId == "PRIMARY_EMAIL"
                select new { prty, pt, pcm, cm, tn, pcmp, cmpt }; // Store the result of the joins

            var groupedResults = from result in query // Use a new variable name 
                group result by result.prty.PartyId
                into partyGroup
                select new PartyRecord
                {
                    PartyId = partyGroup.Key, // The party ID
                    Description = partyGroup.First().prty.Description + " ( " + partyGroup.First().prty.MainRole +
                                  " )", // Access the party from the first record in the group
                    PartyTypeDescription = partyGroup.First().pt.Description,
                    MobileContactNumber = partyGroup
                        .Where(x => x.tn.ContactNumber != null) // Adjusted for telecomNumber nesting
                        .Select(x => x.tn.ContactNumber)
                        .FirstOrDefault(),
                    Address1 = partyGroup.Where(x => x.cm.PostalAddress != null)
                        .Select(x => x.cm.PostalAddress.Address1)
                        .FirstOrDefault(),
                    InfoString = partyGroup.Where(x => x.cm.InfoString != null)
                        .Select(x => x.cm.InfoString)
                        .FirstOrDefault()
                };
            return groupedResults;
        }
    }
}