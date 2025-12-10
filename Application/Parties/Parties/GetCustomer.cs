using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class GetCustomer
{
    public class Query : IRequest<Result<PartyDto>>
    {
        public string PartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PartyDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PartyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = await (
                from prty in _context.Parties
                where prty.PartyId == request.PartyId

                // === CORE: Always present ===
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join st in _context.StatusItems on prty.StatusId equals st.StatusId into stGroup
                from st in stGroup.DefaultIfEmpty()
                join person in _context.Persons on prty.PartyId equals person.PartyId into personGroup
                from person in personGroup.DefaultIfEmpty()               // Person may not exist for non-person parties

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
                    on phonePurpose.ContactMechPurposeTypeId equals cmptPhone.ContactMechPurposeTypeId into cmptPhoneGroup
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

                select new PartyDto
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description + " ( " + prty.MainRole + " )",

                    // REFACTOR: Use Description as GroupName (no PartyGroup for customers either)
                    // Purpose: UI expects GroupName for the name field
                    // Improvement: Consistent with supplier and seeded data
                    GroupName = prty.Description,

                    PartyTypeId = pt.PartyTypeId,
                    PartyTypeDescription = pt.Description,
                    StatusDescription = st != null ? st.Description : null,
                    MainRole = prty.MainRole,

                    // Person fields (only for Person parties)
                    FirstName = person != null ? person.FirstName : null,

                    // === PHONE ===
                    MobileContactNumber = phonePurpose != null ? tn.ContactNumber : null,
                    ContactType = phonePurpose != null ? cmptPhone.Description : null,

                    // === ADDRESS ===
                    Address1 = addrPurpose != null ? pa.Address1 : null,
                    Address2 = addrPurpose != null ? pa.Address2 : null,
                    GeoId = addrPurpose != null ? pa.CountryGeoId : null,
                    GeoName = addrPurpose != null ? geo.GeoName : null,

                    // === EMAIL ===
                    InfoString = emailPurpose != null ? cmEmail.InfoString : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
                return Result<PartyDto>.Failure("Party not found");

            // REFACTOR: Clean GroupName — remove " ( CUSTOMER )" suffix if present
            // Purpose: UI displays a clean name in inputs/selects
            // Improvement: Matches supplier behavior and form expectations
            if (!string.IsNullOrEmpty(result.Description))
            {
                var cleanName = result.Description.Split(" ( ").FirstOrDefault();
                if (!string.IsNullOrEmpty(cleanName))
                    result.GroupName = cleanName;
            }

            return Result<PartyDto>.Success(result);
        }
    }
}