using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class GetSupplier
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
            var result = await (
                    from prty in _context.Parties
                    where prty.PartyId == request.PartyId

                    // === CORE: Always present ===
                    join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                    join st in _context.StatusItems on prty.StatusId equals st.StatusId into stGroup
                    from st in stGroup.DefaultIfEmpty()

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
                    join pgaAp in _context.PartyGlAccounts
                        on new { Org = "Company", P = prty.PartyId, R = "BILL_FROM_VENDOR", T = "ACCOUNTS_PAYABLE" }
                        equals new { Org = pgaAp.OrganizationPartyId, P = pgaAp.PartyId, R = pgaAp.RoleTypeId, T = pgaAp.GlAccountTypeId }
                        into pgaApGroup
                    from pgaAp in pgaApGroup.DefaultIfEmpty()

                    join glaAp in _context.GlAccounts on pgaAp.GlAccountId equals glaAp.GlAccountId into glaApGroup
                    from glaAp in glaApGroup.DefaultIfEmpty()
                    select new PartyDto
                    {
                        PartyId = prty.PartyId,
                        Description = prty.Description + " ( " + prty.MainRole + " )",

                        // REFACTOR: Use Description as GroupName (no PartyGroup exists)
                        // Purpose: Form expects GroupName for supplier name
                        // Improvement: Matches seeded data and UI
                        GroupName = prty.Description,

                        PartyTypeId = pt.PartyTypeId,
                        PartyTypeDescription = pt.Description,
                        StatusDescription = st.Description,
                        MainRole = prty.MainRole,

                        // === PHONE ===
                        MobileContactNumber = phonePurpose != null ? tn.ContactNumber : null,
                        ContactType = phonePurpose != null ? cmptPhone.Description : null,

                        // === ADDRESS ===
                        Address1 = addrPurpose != null ? pa.Address1 : null,
                        Address2 = addrPurpose != null ? pa.Address2 : null,
                        GeoId = addrPurpose != null ? pa.CountryGeoId : null,
                        GeoName = addrPurpose != null ? geo.GeoName : null,

                        // === EMAIL ===
                        InfoString = emailPurpose != null ? cmEmail.InfoString : null,
                        ApGlAccountId = pgaAp != null ? pgaAp.GlAccountId : null,
                        ApGlAccountName = glaAp != null ? glaAp.AccountName : null,
                        ApGlAccountNameArabic = glaAp != null ? glaAp.AccountNameArabic : null,
                        ApGlAccountDescription = glaAp != null ? glaAp.Description : null,
                        ApGlAccountCreatedStamp = glaAp != null ? (DateTime?)glaAp.CreatedStamp : null,
                    })
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
                return Result<PartyDto>.Failure("Party not found");

            // REFACTOR: Clean GroupName — remove " ( SUPPLIER )" suffix
            // Purpose: UI shows clean name in input
            // Improvement: Consistent with form display
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