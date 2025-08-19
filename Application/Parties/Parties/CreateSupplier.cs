using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class CreateSupplier
{
    public class Command : IRequest<Result<PartyDto>>
    {
        public PartyDto PartyDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
    }

    public class Handler : IRequestHandler<Command, Result<PartyDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
            _context = context;
        }

        public async Task<Result<PartyDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());

            var partyStatusPartyEnabled = await _context.StatusItems.SingleOrDefaultAsync(x =>
                x.StatusId == "PARTY_ENABLED");

            var partyType = await _context.PartyTypes.SingleOrDefaultAsync(
                x => x.PartyTypeId == "PARTY_GROUP");

            var contactMechTypeTelCommNumber = await _context.ContactMechTypes.SingleOrDefaultAsync(
                x => x.ContactMechTypeId == "TELECOM_NUMBER");

            var contactMechTypeEmailAddress = await _context.ContactMechTypes.SingleOrDefaultAsync(
                x => x.ContactMechTypeId == "EMAIL_ADDRESS");

            var contactMechTypePostalAddress = await _context.ContactMechTypes.SingleOrDefaultAsync(
                x => x.ContactMechTypeId == "POSTAL_ADDRESS");

            // REFACTOR: Fetch all required role types for the party to match the first party's roles
            // Purpose: Ensure the new party is assigned all roles (ACCOUNT, BILL_FROM_VENDOR, SHIP_FROM_VENDOR, SUPPLIER, SUPPLIER_AGENT)
            // Improvement: Centralizes role fetching for consistency and prepares for adding multiple roles
            var roleTypeIds = new[] { "SUPPLIER", "ACCOUNT", "BILL_FROM_VENDOR", "SHIP_FROM_VENDOR", "SUPPLIER_AGENT" };
            var roleTypes = await _context.RoleTypes
                .Where(x => roleTypeIds.Contains(x.RoleTypeId))
                .ToListAsync(cancellationToken);

            var roleTypeSupplier = roleTypes.SingleOrDefault(x => x.RoleTypeId == "SUPPLIER");

            var contactMechPurposeTypePhoneMobile = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "PRIMARY_PHONE");

            var contactMechPurposeTypeGeneralLocation = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "GENERAL_LOCATION");

            var contactMechPurposeTypeShippingLocation = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "SHIPPING_LOCATION");

            var contactMechPurposeTypePrimaryEmail = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "PRIMARY_EMAIL");

            var stamp = DateTime.Now;
            var newPartyId = Guid.NewGuid().ToString();

            var party = new Party
            {
                PartyId = newPartyId,
                PartyType = partyType,
                Status = partyStatusPartyEnabled,
                MainRole = roleTypeSupplier?.RoleTypeId, // Keep SUPPLIER as MainRole
                Description = request.PartyDto.GroupName,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.Parties.Add(party);

            // REFACTOR: Add PartyRole entries for all required roles instead of just SUPPLIER
            // Purpose: Assigns all roles from the first party to ensure consistency
            // Improvement: Loops through roleTypes to create PartyRole entries, ensuring all roles are assigned
            foreach (var roleType in roleTypes)
            {
                var partyRole = new PartyRole
                {
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp,
                    Party = party,
                    RoleType = roleType
                };
                _context.PartyRoles.Add(partyRole);
            }

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

            // Add mobile
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
                    PartyRole = _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeSupplier), // Use SUPPLIER role
                    RoleType = roleTypeSupplier
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

            // Add email
            if (!string.IsNullOrEmpty(request.PartyDto.InfoString))
            {
                var contactMech = new ContactMech
                {
                    ContactMechId = Guid.NewGuid().ToString(),
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    InfoString = request.PartyDto.InfoString,
                    ContactMechType = contactMechTypeEmailAddress
                };
                _context.ContactMeches.Add(contactMech);

                var partyContactMech = new PartyContactMech
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    Party = party,
                    PartyRole = _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeSupplier), // Use SUPPLIER role
                    RoleType = roleTypeSupplier
                };
                _context.PartyContactMeches.Add(partyContactMech);

                var partyContactMechPurpose = new PartyContactMechPurpose
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    ContactMechPurposeType = contactMechPurposeTypePrimaryEmail,
                    Party = party
                };
                _context.PartyContactMechPurposes.Add(partyContactMechPurpose);
            }

            // Add address
            if (!string.IsNullOrEmpty(request.PartyDto.Address1))
            {
                var contactMech = new ContactMech
                {
                    ContactMechId = Guid.NewGuid().ToString(),
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMechType = contactMechTypePostalAddress
                };
                _context.ContactMeches.Add(contactMech);

                var partyContactMech = new PartyContactMech
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    Party = party,
                    PartyRole = _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeSupplier), // Use SUPPLIER role
                    RoleType = roleTypeSupplier
                };
                _context.PartyContactMeches.Add(partyContactMech);

                var partyContactMechPurposeGeneralLocation = new PartyContactMechPurpose
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    ContactMechPurposeType = contactMechPurposeTypeGeneralLocation,
                    Party = party
                };
                _context.PartyContactMechPurposes.Add(partyContactMechPurposeGeneralLocation);

                var partyContactMechPurposeShippingLocation = new PartyContactMechPurpose
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    ContactMechPurposeType = contactMechPurposeTypeShippingLocation,
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

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                transaction.Rollback();
                return Result<PartyDto>.Failure("Failed to create Supplier");
            }

            transaction.Commit();

            var partyToReturn = new PartyDto
            {
                PartyId = newPartyId,
                Description = request.PartyDto.FirstName + " ( " + roleTypeSupplier?.RoleTypeId + " )",
                PartyTypeDescription = partyStatus.PartyId
            };
            return Result<PartyDto>.Success(partyToReturn);
        }
    }
}