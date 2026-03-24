using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class ListAllParties
{
    public class PartyReportDto
    {
        public string PartyId { get; set; }
        public string Description { get; set; }
        public string MainRole { get; set; }
        public string MobileContactNumber { get; set; }
        public List<PartyGlAccountDto> GlAccounts { get; set; } = new();
    }

    public class PartyGlAccountDto
    {
        public string GlAccountId { get; set; }
        public string AccountNameArabic { get; set; }
    }

    public class Query : IRequest<Result<List<PartyReportDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<PartyReportDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PartyReportDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var allowedRoles = new[] { "CUSTOMER", "SUPPLIER", "EMPLOYEE", "CONTRACTOR" };

            var partiesQuery =
                from prty in _context.Parties
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
                    on new { PartyId = pcm.PartyId, ContactMechId = pcm.ContactMechId }
                    equals new { pcmp.PartyId, pcmp.ContactMechId } into pcmpGroup
                from pcmp in pcmpGroup.DefaultIfEmpty()

                // LEFT JOIN: ContactMechPurposeType
                join cmpt in _context.ContactMechPurposeTypes
                    on pcmp.ContactMechPurposeTypeId equals cmpt.ContactMechPurposeTypeId 
                    into cmptGroup
                from cmpt in cmptGroup.DefaultIfEmpty()

                where cmpt == null ||
                      cmpt.ContactMechPurposeTypeId == "PRIMARY_PHONE" ||
                      cmpt.ContactMechPurposeTypeId == "GENERAL_LOCATION" ||
                      cmpt.ContactMechPurposeTypeId == "PRIMARY_EMAIL"

                select new { prty, pt, tn, cmpt };

            var rawData = await partiesQuery.ToListAsync(cancellationToken);

            // Group in memory to handle multiple contact mechanisms if any, though the original query does FirstOrDefault
            var parties = rawData
                .GroupBy(x => x.prty.PartyId)
                .Select(g => new PartyReportDto
                {
                    PartyId = g.Key,
                    Description = g.First().prty.Description,
                    MainRole = g.First().prty.MainRole,
                    MobileContactNumber = g.Where(x => x.tn != null).Select(x => x.tn.ContactNumber).FirstOrDefault()
                })
                .OrderBy(p => p.Description)
                .ToList();

            var partyIds = parties.Select(p => p.PartyId).ToList();

            var glAccounts = await _context.PartyGlAccounts
                .Where(pga => partyIds.Contains(pga.PartyId))
                .Include(pga => pga.GlAccount)
                .Select(pga => new
                {
                    pga.PartyId,
                    pga.GlAccountId,
                    pga.GlAccount.AccountNameArabic
                })
                .ToListAsync(cancellationToken);

            foreach (var party in parties)
            {
                party.GlAccounts = glAccounts
                    .Where(ga => ga.PartyId == party.PartyId)
                    .Select(ga => new PartyGlAccountDto
                    {
                        GlAccountId = ga.GlAccountId,
                        AccountNameArabic = ga.AccountNameArabic
                    })
                    .ToList();
            }

            return Result<List<PartyReportDto>>.Success(parties);
        }
    }
}
