using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class CreateExternalSalesRep
{
    public class Command : IRequest<Result<PartyDto2>>
    {
        public PartyDto2 PartyDto { get; set; }
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

        public async Task<Result<PartyDto2>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            var partyStatusPartyEnabled = await _context.StatusItems.SingleOrDefaultAsync(x =>
                x.StatusId == "PARTY_ENABLED", cancellationToken);

            var partyType = await _context.PartyTypes.SingleOrDefaultAsync(
                x => x.PartyTypeId == "PARTY_GROUP", cancellationToken);

            var contactMechTypeTelCommNumber = await _context.ContactMechTypes.SingleOrDefaultAsync(
                x => x.ContactMechTypeId == "TELECOM_NUMBER", cancellationToken);

            var roleType = await _context.RoleTypes.SingleOrDefaultAsync(
                x => x.RoleTypeId == "EXTERNAL_SALES_REP", cancellationToken);

            var contactMechPurposeTypePhoneMobile = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "PRIMARY_PHONE", cancellationToken);

            var stamp = DateTime.UtcNow;
            var newPartyId = await _utilityService.GetNextSequence("Party");

            var party = new Party
            {
                PartyId = newPartyId,
                PartyType = partyType,
                Status = partyStatusPartyEnabled,
                MainRole = "EXTERNAL_SALES_REP",
                Description = request.PartyDto.GroupName,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.Parties.Add(party);

            var partyRole = new PartyRole
            {
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp,
                Party = party,
                RoleType = roleType
            };
            _context.PartyRoles.Add(partyRole);

            var partyStatus = new PartyStatus
            {
                StatusDate = stamp,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp,
                Status = partyStatusPartyEnabled,
                Party = party
            };
            _context.PartyStatuses.Add(partyStatus);

            var partyGroup = new PartyGroup
            {
                GroupName = request.PartyDto.GroupName,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp,
                Party = party
            };
            _context.PartyGroups.Add(partyGroup);

            if (!string.IsNullOrEmpty(request.PartyDto.MobileContactNumber))
            {
                var contactMech = new ContactMech
                {
                    ContactMechId = Guid.NewGuid().ToString(),
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMechType = contactMechTypeTelCommNumber
                };
                _context.ContactMeches.Add(contactMech);

                var telecomNumber = new TelecomNumber
                {
                    ContactNumber = request.PartyDto.MobileContactNumber,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech
                };
                _context.TelecomNumbers.Add(telecomNumber);

                var partyContactMech = new PartyContactMech
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    Party = party,
                    RoleType = roleType
                };
                _context.PartyContactMeches.Add(partyContactMech);

                var partyContactMechPurpose = new PartyContactMechPurpose
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    ContactMechPurposeType = contactMechPurposeTypePhoneMobile,
                    Party = party
                };
                _context.PartyContactMechPurposes.Add(partyContactMechPurpose);
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure("Failed to create External Sales Rep");
            }

            return Result<PartyDto2>.Success(new PartyDto2
            {
                PartyId = newPartyId,
                Description = request.PartyDto.GroupName + " ( EXTERNAL_SALES_REP )"
            });
        }
    }
}
