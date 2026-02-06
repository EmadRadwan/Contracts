using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Parties.Parties;

public class GetContractor
{
    public class Query : IRequest<Result<PartyDto>>
    {
        public string PartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PartyDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<PartyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query =
                    from prty in _context.Parties
                    where prty.PartyId == request.PartyId

                    // === CORE: Always present ===
                    join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                    join st in _context.StatusItems on prty.StatusId equals st.StatusId into stGroup
                    from st in stGroup.DefaultIfEmpty()
                    join ptgr in _context.PartyGroups on prty.PartyId equals ptgr.PartyId into ptgrGroup
                    from ptgr in ptgrGroup.DefaultIfEmpty()

                    // === PHONE: Left join (PRIMARY_PHONE) ===
                    join phonePurpose in _context.PartyContactMechPurposes
                            .Where(p => p.ContactMechPurposeTypeId == "PRIMARY_PHONE")
                        on prty.PartyId equals phonePurpose.PartyId into phonePurposeGroup
                    from phonePurpose in phonePurposeGroup.DefaultIfEmpty()
                    join pcmPhone in _context.PartyContactMeches
                        on new { phonePurpose.PartyId, phonePurpose.ContactMechId }
                        equals new { pcmPhone.PartyId, pcmPhone.ContactMechId } into pcmPhoneGroup
                    from pcmPhone in pcmPhoneGroup.DefaultIfEmpty()
                    join cmPhone in _context.ContactMeches
                        on pcmPhone.ContactMechId equals cmPhone.ContactMechId into cmPhoneGroup
                    from cmPhone in cmPhoneGroup.DefaultIfEmpty()
                    join tn in _context.TelecomNumbers
                        on cmPhone.ContactMechId equals tn.ContactMechId into tnGroup
                    from tn in tnGroup.DefaultIfEmpty()
                    join cmptPhone in _context.ContactMechPurposeTypes
                        on phonePurpose.ContactMechPurposeTypeId equals cmptPhone.ContactMechPurposeTypeId into
                        cmptPhoneGroup
                    from cmptPhone in cmptPhoneGroup.DefaultIfEmpty()

                    // === ADDRESS: Left join (GENERAL_LOCATION) ===
                    join addrPurpose in _context.PartyContactMechPurposes
                            .Where(p => p.ContactMechPurposeTypeId == "GENERAL_LOCATION")
                        on prty.PartyId equals addrPurpose.PartyId into addrPurposeGroup
                    from addrPurpose in addrPurposeGroup.DefaultIfEmpty()
                    join pcmAddr in _context.PartyContactMeches
                        on new { addrPurpose.PartyId, addrPurpose.ContactMechId }
                        equals new { pcmAddr.PartyId, pcmAddr.ContactMechId } into pcmAddrGroup
                    from pcmAddr in pcmAddrGroup.DefaultIfEmpty()
                    join cmAddr in _context.ContactMeches
                        on pcmAddr.ContactMechId equals cmAddr.ContactMechId into cmAddrGroup
                    from cmAddr in cmAddrGroup.DefaultIfEmpty()
                    join pa in _context.PostalAddresses
                        on cmAddr.ContactMechId equals pa.ContactMechId into paGroup
                    from pa in paGroup.DefaultIfEmpty()
                    join geo in _context.Geos
                        on pa.CountryGeoId equals geo.GeoId into geoGroup
                    from geo in geoGroup.DefaultIfEmpty()

                    // === EMAIL: Left join (PRIMARY_EMAIL) ===
                    join emailPurpose in _context.PartyContactMechPurposes
                            .Where(p => p.ContactMechPurposeTypeId == "PRIMARY_EMAIL")
                        on prty.PartyId equals emailPurpose.PartyId into emailPurposeGroup
                    from emailPurpose in emailPurposeGroup.DefaultIfEmpty()
                    join pcmEmail in _context.PartyContactMeches
                        on new { emailPurpose.PartyId, emailPurpose.ContactMechId }
                        equals new { pcmEmail.PartyId, pcmEmail.ContactMechId } into pcmEmailGroup
                    from pcmEmail in pcmEmailGroup.DefaultIfEmpty()
                    join cmEmail in _context.ContactMeches
                        on pcmEmail.ContactMechId equals cmEmail.ContactMechId into cmEmailGroup
                    from cmEmail in cmEmailGroup.DefaultIfEmpty()
                    join pga in _context.PartyGlAccounts on prty.PartyId equals pga.PartyId into pgaGroup
                    from pga in pgaGroup.DefaultIfEmpty()
                    join gla in _context.GlAccounts on pga.GlAccountId equals gla.GlAccountId into glaGroup
                    from gla in glaGroup.DefaultIfEmpty()
                    join role in _context.RoleTypes on pga.RoleTypeId equals role.RoleTypeId into roleGroup
                    from role in roleGroup.DefaultIfEmpty()
                    select new
                    {
                        Party = prty,
                        PartyType = pt,
                        Status = st,
                        PhonePurpose = phonePurpose,
                        TelecomNumber = tn,
                        PhonePurposeType = cmptPhone,
                        AddrPurpose = addrPurpose,
                        PostalAddress = pa,
                        Geo = geo,
                        EmailPurpose = emailPurpose,
                        EmailInfo = cmEmail.InfoString,
                        Pga = pga,
                        Gla = gla,
                        RoleType = role
                    };
            
            var rawResults = await query
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (!rawResults.Any())
            {
                return Result<PartyDto>.Failure("Party not found");
            }

            // Group results by party (since group join creates cartesian product)
            var firstRecord = rawResults.First();

            var dto = new PartyDto
            {
                PartyId = firstRecord.Party.PartyId,
                Description = $"{firstRecord.Party.Description ?? ""} ( {firstRecord.Party.MainRole ?? ""} )",
                GroupName = firstRecord.Party.Description?.Split(" ( ").FirstOrDefault() ??
                            firstRecord.Party.Description ?? "",
                PartyTypeId = firstRecord.PartyType.PartyTypeId,
                PartyTypeDescription = firstRecord.PartyType.Description,
                StatusDescription = firstRecord.Status?.Description,
                MainRole = firstRecord.Party.MainRole,

                // Phone
                MobileContactNumber = firstRecord.TelecomNumber?.ContactNumber,
                ContactType = firstRecord.PhonePurposeType?.Description,

                // Address
                Address1 = firstRecord.PostalAddress?.Address1,
                Address2 = firstRecord.PostalAddress?.Address2,
                GeoId = firstRecord.PostalAddress?.CountryGeoId,
                GeoName = firstRecord.Geo?.GeoName,

                // Email
                InfoString = firstRecord.EmailInfo,

                // All linked GL accounts
                LinkedGlAccounts = rawResults
                    .Where(r => r.Pga != null && r.Gla != null)
                    .Select(r => new PartyGlAccountSimpleDto
                    {
                        GlAccountId = r.Pga.GlAccountId,
                        GlAccountTypeId = r.Pga.GlAccountTypeId,
                        RoleTypeId = r.Pga.RoleTypeId,
                        RoleDescription = r.RoleType?.Description ?? r.Pga.RoleTypeId,
                        AccountName = r.Gla.AccountName,
                        AccountNameArabic = r.Gla.AccountNameArabic,
                        AccountDescription = r.Gla.Description,
                        CreatedStamp = r.Gla.CreatedStamp
                    })
                    .OrderBy(a => a.RoleTypeId)
                    .ThenBy(a => a.GlAccountId)
                    .ToList()
            };

            return Result<PartyDto>.Success(dto);
        }
    }
}