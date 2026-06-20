using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class CreateSalesManager
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

            var roleTypeSalesManager = await _context.RoleTypes.SingleOrDefaultAsync(
                x => x.RoleTypeId == "SALES_MANAGER", cancellationToken);

            var contactMechPurposeTypePhoneMobile = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "PRIMARY_PHONE", cancellationToken);

            var stamp = DateTime.UtcNow;
            var newPartyId = await _utilityService.GetNextSequence("Party");

            var party = new Domain.Party
            {
                PartyId = newPartyId,
                PartyType = partyType,
                Status = partyStatusPartyEnabled,
                MainRole = "SALES_MANAGER",
                Description = request.PartyDto.GroupName,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.Parties.Add(party);

            var partyRole = new Domain.PartyRole
            {
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp,
                Party = party,
                RoleType = roleTypeSalesManager
            };
            _context.PartyRoles.Add(partyRole);

            var partyStatus = new Domain.PartyStatus
            {
                StatusDate = stamp,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp,
                Status = partyStatusPartyEnabled,
                Party = party
            };
            _context.PartyStatuses.Add(partyStatus);

            var partyGroup = new Domain.PartyGroup
            {
                GroupName = request.PartyDto.GroupName,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp,
                Party = party
            };
            _context.PartyGroups.Add(partyGroup);

            if (!string.IsNullOrEmpty(request.PartyDto.MobileContactNumber))
            {
                var contactMech = new Domain.ContactMech
                {
                    ContactMechId = Guid.NewGuid().ToString(),
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMechType = contactMechTypeTelCommNumber
                };
                _context.ContactMeches.Add(contactMech);

                var telecomNumber = new Domain.TelecomNumber
                {
                    ContactNumber = request.PartyDto.MobileContactNumber,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech
                };
                _context.TelecomNumbers.Add(telecomNumber);

                var partyContactMech = new Domain.PartyContactMech
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    Party = party,
                    RoleType = roleTypeSalesManager
                };
                _context.PartyContactMeches.Add(partyContactMech);

                var partyContactMechPurpose = new Domain.PartyContactMechPurpose
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure("Failed to create Sales Manager");
            }

            return Result<PartyDto2>.Success(new PartyDto2
            {
                PartyId = newPartyId,
                Description = request.PartyDto.GroupName + " ( SALES_MANAGER )"
            });
        }
    }
}
