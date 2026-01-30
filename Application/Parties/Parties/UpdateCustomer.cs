using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class UpdateCustomer
{
    public class Command : IRequest<Result<PartyDto>>
    {
        public PartyDto PartyDto { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result<PartyDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<PartyDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            var party = await _context.Parties.FindAsync(request.PartyDto.PartyId);

            if (party == null) return null;

            var stamp = DateTime.Now;

            party.LastUpdatedStamp = stamp;
            party.Description = request.PartyDto.FirstName;


            var person = await _context.Persons.FindAsync(request.PartyDto.PartyId);

            if (person == null) return null;

            person.FirstName = request.PartyDto.FirstName;
            person.LastUpdatedStamp = stamp;

            var telcomNumber = from prty in _context.Parties
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                select tn;


            var primaryTelcomNumber = telcomNumber.SingleOrDefault();

            primaryTelcomNumber.ContactNumber = request.PartyDto.MobileContactNumber;


            var currentPostalAddress = from prty in _context.Parties
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pa in _context.PostalAddresses on cm.ContactMechId equals pa.ContactMechId
                join geo in _context.Geos on pa.CountryGeoId equals geo.GeoId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyDto.PartyId &&
                      pcmp.ContactMechPurposeTypeId == "GENERAL_LOCATION"
                select pa;

            var generalLocation = currentPostalAddress.SingleOrDefault();
            if (generalLocation != null)
            {
                generalLocation.Address1 = request.PartyDto.Address1;
                generalLocation.Address2 = request.PartyDto.Address2;
                generalLocation.ToName = request.PartyDto.FirstName;
                generalLocation.CountryGeoId = request.PartyDto.GeoId;
            }
            else
            {
                if (!string.IsNullOrEmpty(request.PartyDto.Address1))
                {
                    var contactMech = new ContactMech
                    {
                        ContactMechId = Guid.NewGuid().ToString(),
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMechTypeId = "POSTAL_ADDRESS"
                    };
                    _context.ContactMeches.Add(contactMech);

                    var partyContactMech = new PartyContactMech
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        Party = party,
                        RoleTypeId = "CUSTOMER"
                    };
                    _context.PartyContactMeches.Add(partyContactMech);

                    var partyContactMechPurposeGeneralLocation = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "GENERAL_LOCATION",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurposeGeneralLocation);

                    var partyContactMechPurposeShippingLocation = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "SHIPPING_LOCATION",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurposeShippingLocation);

                    var postalAddress = new PostalAddress
                    {
                        ContactMech = contactMech,
                        ToName = request.PartyDto.FirstName,
                        Address1 = request.PartyDto.Address1,
                        Address2 = request.PartyDto.Address2,
                        CountryGeoId = request.PartyDto.GeoId
                    };
                    _context.PostalAddresses.Add(postalAddress);
                }
            }

            var currentContactMech = from prty in _context.Parties
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join prs in _context.Persons on prty.PartyId equals prs.PartyId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_EMAIL"
                select cm;

            var primaryEmail = currentContactMech.SingleOrDefault();
            if (primaryEmail != null)
            {
                primaryEmail.InfoString = request.PartyDto.InfoString;
            }
            else
            {
                if (!string.IsNullOrEmpty(request.PartyDto.Address1))
                {
                    var contactMech = new ContactMech
                    {
                        ContactMechId = Guid.NewGuid().ToString(),
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        InfoString = request.PartyDto.InfoString,
                        ContactMechTypeId = "EMAIL_ADDRESS"
                    };
                    _context.ContactMeches.Add(contactMech);


                    var partyContactMech = new PartyContactMech
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        Party = party,
                        RoleTypeId = "CUSTOMER"
                    };
                    _context.PartyContactMeches.Add(partyContactMech);

                    var partyContactMechPurpose = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "PRIMARY_EMAIL",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurpose);
                }
            }

            bool glCreated = false;
            string? newGlAccountId = null;

            var existingPartyGl = await _context.PartyGlAccounts
                .AnyAsync(pga =>
                        pga.OrganizationPartyId == "Company" &&
                        pga.PartyId == request.PartyDto.PartyId &&
                        pga.RoleTypeId == "BILL_TO_CUSTOMER" &&
                        pga.GlAccountTypeId == "ACCOUNTS_RECEIVABLE",
                    cancellationToken);

            if (!existingPartyGl)
            {
                // Generate unique GL Account ID (same logic as in CreateCustomer)
                const string prefix = "12";
                const int digits = 4;
                const int maxAttempts = 900;
                int suffix = 1;
                newGlAccountId = null;

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    string candidate = $"{prefix}{suffix.ToString().PadLeft(digits, '0')}";
                    bool exists = await _context.GlAccounts
                        .AnyAsync(a => a.GlAccountId == candidate, cancellationToken);

                    if (!exists)
                    {
                        newGlAccountId = candidate;
                        break;
                    }

                    suffix++;
                }

                if (newGlAccountId == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<PartyDto>.Failure(
                        $"Could not generate unique GL account ID after {maxAttempts} attempts.");
                }

                // 1. Create GlAccount
                var newGlAccount = new GlAccount
                {
                    GlAccountId = newGlAccountId,
                    GlAccountTypeId = "ACCOUNTS_RECEIVABLE",
                    GlAccountClassId = "CURRENT_ASSET",
                    GlResourceTypeId = "MONEY",
                    GlXbrlClassId = null,
                    ParentGlAccountId = "121100",
                    AccountCode = newGlAccountId,
                    AccountName = $"AR - {request.PartyDto.FirstName} ({request.PartyDto.PartyId})",
                    AccountNameArabic = $"مدينون - {request.PartyDto.FirstName}",
                    Description = $"Accounts Receivable sub-ledger for customer {request.PartyDto.PartyId}",
                    CreatedStamp = stamp,
                    CreatedTxStamp = stamp,
                    LastUpdatedStamp = stamp,
                    LastUpdatedTxStamp = stamp
                };
                _context.GlAccounts.Add(newGlAccount);

                // 2. GlAccountOrganization
                var glOrg = new GlAccountOrganization
                {
                    GlAccountId = newGlAccountId,
                    OrganizationPartyId = "Company",
                    RoleTypeId = null,
                    FromDate = stamp,
                    ThruDate = null,
                    CreatedStamp = stamp,
                    CreatedTxStamp = stamp,
                    LastUpdatedStamp = stamp,
                    LastUpdatedTxStamp = stamp
                };
                _context.GlAccountOrganizations.Add(glOrg);

                // 3. PartyGlAccount
                var partyGl = new PartyGlAccount
                {
                    OrganizationPartyId = "Company",
                    PartyId = request.PartyDto.PartyId,
                    RoleTypeId = "BILL_TO_CUSTOMER",
                    GlAccountTypeId = "ACCOUNTS_RECEIVABLE",
                    GlAccountId = newGlAccountId,
                    CreatedStamp = stamp,
                    CreatedTxStamp = stamp,
                    LastUpdatedStamp = stamp,
                    LastUpdatedTxStamp = stamp
                };
                _context.PartyGlAccounts.Add(partyGl);

                glCreated = true;
            }

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto>.Failure("Failed to update Customer");
            }

            await transaction.CommitAsync(cancellationToken);


            var query1 = from prty in _context.Parties
                join st in _context.StatusItems on prty.StatusId equals st.StatusId
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join prs in _context.Persons on prty.PartyId equals prs.PartyId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                select new PartyDto
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description + " ( " + prty.MainRole + " )",
                    PartyTypeId = pt.PartyTypeId,
                    PartyTypeDescription = pt.Description,
                    FirstName = prs.FirstName,
                    MobileContactNumber = tn.ContactNumber,
                    ContactType = cmpt.Description,
                    InfoString = cm.InfoString,
                    MainRole = prty.MainRole,
                    StatusDescription = st.Description
                };

            var query2 = from prty in _context.Parties
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join prs in _context.Persons on prty.PartyId equals prs.PartyId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pa in _context.PostalAddresses on cm.ContactMechId equals pa.ContactMechId
                join geo in _context.Geos on pa.CountryGeoId equals geo.GeoId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "GENERAL_LOCATION"
                select new PartyDto
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description + " ( " + prty.MainRole + " )",
                    PartyTypeId = pt.PartyTypeId,
                    PartyTypeDescription = pt.Description,
                    FirstName = prs.FirstName,
                    ContactType = cmpt.Description,
                    InfoString = cm.InfoString,
                    Address1 = pa.Address1,
                    Address2 = pa.Address2,
                    GeoId = geo.GeoId,
                    GeoName = geo.GeoName,
                    MainRole = prty.MainRole
                };

            var query3 = from prty in _context.Parties
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join prs in _context.Persons on prty.PartyId equals prs.PartyId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_EMAIL"
                select new PartyDto
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description,
                    PartyTypeId = pt.PartyTypeId,
                    PartyTypeDescription = pt.Description,
                    FirstName = prs.FirstName,
                    ContactType = cmpt.Description,
                    InfoString = cm.InfoString,
                    MainRole = prty.MainRole
                };

            var results1 = query1.ToList();
            var results2 = query2.ToList();
            var results3 = query3.ToList();

            var partyToReturn = new PartyDto();

            if (results1.Count > 0)
            {
                partyToReturn.PartyId = results1[0].PartyId;
                partyToReturn.Description = results1[0].Description;
                partyToReturn.PartyTypeId = results1[0].PartyTypeId;
                partyToReturn.PartyTypeDescription = results1[0].PartyTypeDescription;
                partyToReturn.FirstName = results1[0].FirstName;
                partyToReturn.MobileContactNumber = results1[0].MobileContactNumber;
                partyToReturn.MainRole = results1[0].MainRole;
                partyToReturn.StatusDescription = results1[0].StatusDescription;
            }

            if (results2.Count > 0)
            {
                partyToReturn.Address1 = results2[0].Address1;
                partyToReturn.Address2 = results2[0].Address2;
                partyToReturn.GeoId = results2[0].GeoId;
                partyToReturn.GeoName = results2[0].GeoName;
                partyToReturn.MainRole = results2[0].MainRole;
            }

            if (results3.Count > 0)
            {
                partyToReturn.InfoString = results3[0].InfoString;
                partyToReturn.MainRole = results3[0].MainRole;
            }
            
            if (glCreated && newGlAccountId != null)
            {
                partyToReturn.CreatedGlAccountId = newGlAccountId;
                partyToReturn.CreatedGlAccountName = $"AR - {request.PartyDto.FirstName} ({request.PartyDto.PartyId})";
                partyToReturn.CreatedGlAccountArabicName = $"مدينون - {request.PartyDto.FirstName}";
            }

            return Result<PartyDto>.Success(partyToReturn);
        }
    }
}