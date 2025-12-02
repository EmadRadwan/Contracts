using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class CreateLead
{
    public record Command : IRequest<Result<PartyDto2>>
    {
        public  PartyDto2 PartyDto { get; init; }
        public string? DataSourceId { get; init; } 
    }

    public class Handler : IRequestHandler<Command, Result<PartyDto2>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly IUtilityService _utilityService;

        public Handler(DataContext context, IUserAccessor userAccessor, IUtilityService utilityService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _utilityService = utilityService;
        }

        public async Task<Result<PartyDto2>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            var stamp = DateTime.UtcNow;
            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.UserName == _userAccessor.GetUsername(), ct);

            // === Load all lookup data once ===
            var partyTypePerson = await _context.PartyTypes.SingleAsync(x => x.PartyTypeId == "PERSON", ct);
            var statusEnabled   = await _context.StatusItems.SingleAsync(x => x.StatusId == "PARTY_ENABLED", ct);
            var roleTypeLead    = await _context.RoleTypes.SingleAsync(x => x.RoleTypeId == "LEAD", ct);

            var contactMechTypes = await _context.ContactMechTypes
                .Where(cmt => new[] { "TELECOM_NUMBER", "EMAIL_ADDRESS", "POSTAL_ADDRESS" }
                    .Contains(cmt.ContactMechTypeId))
                .ToDictionaryAsync(x => x.ContactMechTypeId, ct);

            var purposeTypes = await _context.ContactMechPurposeTypes
                .Where(cmp => new[] { "PRIMARY_PHONE", "PRIMARY_EMAIL", "GENERAL_LOCATION", "SHIPPING_LOCATION" }
                    .Contains(cmp.ContactMechPurposeTypeId))
                .ToDictionaryAsync(x => x.ContactMechPurposeTypeId, ct);

            // === Generate OFBiz-style IDs ===
            var partyId       = await _utilityService.GetNextSequence("Party");
            var contactMechId = await _utilityService.GetNextSequence("ContactMech"); // one reusable if only phone/email/address

            var party = new Party
            {
                PartyId          = partyId.ToString(),
                PartyType        = partyTypePerson,
                Status           = statusEnabled,
                Description      = $"{request.PartyDto.FirstName} {request.PartyDto.FirstName}".Trim(),
                CreatedStamp     = stamp,
                LastUpdatedStamp = stamp
            };
            _context.Parties.Add(party);

            // Mandatory for PERSON parties in real OFBiz
            _context.PartyGroups.Add(new PartyGroup { Party = party });
            _context.Persons.Add(new Person
            {
                Party         = party,
                FirstName     = request.PartyDto.FirstName,
                LastName      = request.PartyDto.FirstName ?? "",
                PersonalTitle = request.PartyDto.PersonalTitle,
                CreatedStamp  = stamp,
                LastUpdatedStamp = stamp
            });

            // Role: LEAD
            _context.PartyRoles.Add(new PartyRole
            {
                Party        = party,
                RoleType     = roleTypeLead,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });

            // Status history
            _context.PartyStatuses.Add(new PartyStatus
            {
                Party      = party,
                Status     = statusEnabled,
                StatusDate = stamp,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });

            // === Contact Mechanisms (correct OFBiz way – no PartyRole column!) ===
            if (!string.IsNullOrWhiteSpace(request.PartyDto.MobileContactNumber))
            {
                var cmId = await _utilityService.GetNextSequence("ContactMech");
                var cm = new ContactMech { ContactMechId = cmId.ToString(), ContactMechType = contactMechTypes["TELECOM_NUMBER"], CreatedStamp = stamp, LastUpdatedStamp = stamp };
                _context.ContactMeches.Add(cm);

                _context.TelecomNumbers.Add(new TelecomNumber
                {
                    ContactMech   = cm,
                    ContactNumber = request.PartyDto.MobileContactNumber,
                    CreatedStamp  = stamp,
                    LastUpdatedStamp = stamp
                });

                _context.PartyContactMeches.Add(new PartyContactMech
                {
                    Party        = party,
                    ContactMech  = cm,
                    FromDate     = stamp,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });

                _context.PartyContactMechPurposes.Add(new PartyContactMechPurpose
                {
                    Party                 = party,
                    ContactMech           = cm,
                    ContactMechPurposeType = purposeTypes["PRIMARY_PHONE"],
                    FromDate              = stamp,
                    CreatedStamp          = stamp,
                    LastUpdatedStamp      = stamp
                });
            }

            if (!string.IsNullOrWhiteSpace(request.PartyDto.InfoString))
            {
                var cmId = await _utilityService.GetNextSequence("ContactMech");
                var cm = new ContactMech
                {
                    ContactMechId = cmId.ToString(),
                    InfoString    = request.PartyDto.InfoString,
                    ContactMechType = contactMechTypes["EMAIL_ADDRESS"],
                    CreatedStamp  = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.ContactMeches.Add(cm);

                _context.PartyContactMeches.Add(new PartyContactMech { Party = party, ContactMech = cm, FromDate = stamp, CreatedStamp = stamp, LastUpdatedStamp = stamp });
                _context.PartyContactMechPurposes.Add(new PartyContactMechPurpose
                {
                    Party                 = party,
                    ContactMech           = cm,
                    ContactMechPurposeType = purposeTypes["PRIMARY_EMAIL"],
                    FromDate              = stamp,
                    CreatedStamp          = stamp,
                    LastUpdatedStamp      = stamp
                });
            }

            if (!string.IsNullOrWhiteSpace(request.PartyDto.Address1))
            {
                var cmId = await _utilityService.GetNextSequence("ContactMech");
                var cm = new ContactMech { ContactMechId = cmId.ToString(), ContactMechType = contactMechTypes["POSTAL_ADDRESS"], CreatedStamp = stamp, LastUpdatedStamp = stamp };
                _context.ContactMeches.Add(cm);

                _context.PostalAddresses.Add(new PostalAddress
                {
                    ContactMech = cm,
                    ToName      = $"{request.PartyDto.FirstName} {request.PartyDto.FirstName}",
                    Address1    = request.PartyDto.Address1,
                    Address2    = request.PartyDto.Address2,
                    CountryGeoId = request.PartyDto.GeoId,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });

                _context.PartyContactMeches.Add(new PartyContactMech { Party = party, ContactMech = cm, FromDate = stamp, CreatedStamp = stamp, LastUpdatedStamp = stamp });
                _context.PartyContactMechPurposes.AddRange(new[]
                {
                    new PartyContactMechPurpose { Party = party, ContactMech = cm, ContactMechPurposeType = purposeTypes["GENERAL_LOCATION"], FromDate = stamp, CreatedStamp = stamp, LastUpdatedStamp = stamp },
                    new PartyContactMechPurpose { Party = party, ContactMech = cm, ContactMechPurposeType = purposeTypes["SHIPPING_LOCATION"], FromDate = stamp, CreatedStamp = stamp, LastUpdatedStamp = stamp }
                });
            }

            // === PARTY_DATA_SOURCE – exactly like in your log ===
            _context.PartyDataSources.Add(new PartyDataSource
            {
                Party        = party,
                DataSourceId = request.DataSourceId ?? "COLD_CALL",
                FromDate     = stamp,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });

            /*// Optional but recommended: link current user as sales rep
            if (!string.IsNullOrEmpty(user?.PartyId))
            {
                _context.PartyRelationships.Add(new PartyRelationship
                {
                    PartyIdFrom           = user.PartyId,
                    RoleTypeIdFrom        = "SALES_REP",
                    PartyIdTo             = party.PartyId,
                    RoleTypeIdTo          = "LEAD",
                    PartyRelationshipTypeId = "SALES_ASSIGNMENT",
                    FromDate              = stamp,
                    CreatedStamp          = stamp,
                    LastUpdatedStamp       = stamp
                });
            }*/

            var saved = await _context.SaveChangesAsync(ct) > 0;
            if (!saved)
                return Result<PartyDto2>.Failure("Failed to create Lead");

            await transaction.CommitAsync(ct);

            return Result<PartyDto2>.Success(new PartyDto2
            {
                PartyId     = party.PartyId,
                Description  = party.Description,
                PartyTypeDescription = "Lead"
            });
        }
    }
}