using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class CreateCustomer
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
                x.UserName == _userAccessor.GetUsername(), cancellationToken);

            var partyStatusPartyEnabled = await _context.StatusItems.SingleOrDefaultAsync(x =>
                x.StatusId == "PARTY_ENABLED", cancellationToken);

            var partyType = await _context.PartyTypes.SingleOrDefaultAsync(
                x => x.PartyTypeId == "PERSON", cancellationToken);

            var contactMechTypeTelCommNumber = await _context.ContactMechTypes.SingleOrDefaultAsync(
                x => x.ContactMechTypeId == "TELECOM_NUMBER", cancellationToken);

            var contactMechTypeEmailAddress = await _context.ContactMechTypes.SingleOrDefaultAsync(
                x => x.ContactMechTypeId == "EMAIL_ADDRESS", cancellationToken);

            var contactMechTypePostalAddress = await _context.ContactMechTypes.SingleOrDefaultAsync(
                x => x.ContactMechTypeId == "POSTAL_ADDRESS", cancellationToken);

            // REFACTOR: Fetch all required customer role types to match the target party's roles
            // Purpose: Ensure the new party is assigned all roles (BILL_TO_CUSTOMER, CONTACT, CUSTOMER, END_USER_CUSTOMER, PLACING_CUSTOMER, SHIP_TO_CUSTOMER)
            // Improvement: Centralizes role fetching for efficiency and ensures all roles are available
            var roleTypeIds = new[] { "BILL_TO_CUSTOMER", "CONTACT", "CUSTOMER", "END_USER_CUSTOMER", "PLACING_CUSTOMER", "SHIP_TO_CUSTOMER" };
            var roleTypes = await _context.RoleTypes
                .Where(x => roleTypeIds.Contains(x.RoleTypeId))
                .ToListAsync(cancellationToken);

            // REFACTOR: Validate that all required roles are found
            // Purpose: Prevent partial role assignment if any role type is missing
            // Improvement: Adds robustness by ensuring all expected roles exist in the database
            if (roleTypes.Count != roleTypeIds.Length)
            {
                transaction.Rollback();
                return Result<PartyDto>.Failure("One or more required customer role types are missing in the database.");
            }

            var roleTypeCustomer = roleTypes.SingleOrDefault(x => x.RoleTypeId == "CUSTOMER");

            var contactMechPurposeTypePhoneMobile = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "PRIMARY_PHONE", cancellationToken);

            var contactMechPurposeTypeGeneralLocation = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "GENERAL_LOCATION", cancellationToken);

            var contactMechPurposeTypeShippingLocation = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "SHIPPING_LOCATION", cancellationToken);

            var contactMechPurposeTypePrimaryEmail = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "PRIMARY_EMAIL", cancellationToken);

            var stamp = DateTime.Now; // e.g., 2025-07-16 15:53:00 EEST
            var newPartyId = Guid.NewGuid().ToString();

            var party = new Party
            {
                PartyId = newPartyId,
                PartyType = partyType,
                Status = partyStatusPartyEnabled,
                MainRole = roleTypeCustomer?.RoleTypeId ?? "CUSTOMER", // Keep CUSTOMER as MainRole
                Description = request.PartyDto.FirstName,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.Parties.Add(party);

            // REFACTOR: Add PartyRole entries for all required customer roles
            // Purpose: Assigns all roles from the target party to ensure consistency
            // Improvement: Loops through roleTypes to create PartyRole entries, making the code scalable for role changes
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

            var person = new Person
            {
                FirstName = request.PartyDto.FirstName,
                MiddleName = request.PartyDto.MiddleName,
                PersonalTitle = request.PartyDto.PersonalTitle,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp,
                Party = party
            };
            _context.Persons.Add(person);

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

                // REFACTOR: Use the CUSTOMER role's PartyRole for contact mechanisms
                // Purpose: Ensure contact mechanisms are associated with the CUSTOMER role, consistent with the original logic
                // Improvement: Maintains consistency with the primary role while supporting multiple role assignments
                var partyRoleCustomer = _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeCustomer);
                var partyContactMech = new PartyContactMech
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    Party = party,
                    PartyRole = partyRoleCustomer,
                    RoleType = roleTypeCustomer
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

                // REFACTOR: Use the CUSTOMER role's PartyRole for contact mechanisms
                // Purpose: Ensure email contact mechanism is associated with the CUSTOMER role
                // Improvement: Consistent role usage across contact mechanisms
                var partyRoleCustomer = _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeCustomer);
                var partyContactMech = new PartyContactMech
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    Party = party,
                    PartyRole = partyRoleCustomer,
                    RoleType = roleTypeCustomer
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

                // REFACTOR: Use the CUSTOMER role's PartyRole for contact mechanisms
                // Purpose: Ensure address contact mechanism is associated with the CUSTOMER role
                // Improvement: Maintains consistency with the primary role for contact mechanisms
                var partyRoleCustomer = _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeCustomer);
                var partyContactMech = new PartyContactMech
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    Party = party,
                    PartyRole = partyRoleCustomer,
                    RoleType = roleTypeCustomer
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

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                transaction.Rollback();
                return Result<PartyDto>.Failure("Failed to create Customer");
            }

            transaction.Commit();

            var partyToReturn = new PartyDto
            {
                PartyId = newPartyId,
                Description = request.PartyDto.FirstName + " ( " + roleTypeCustomer?.RoleTypeId + " )",
                PartyTypeDescription = partyStatus.PartyId,
                FromPartyId = new FromPartyDto
                {
                    FromPartyId = party.PartyId,
                    FromPartyName = party.Description
                }
            };
            return Result<PartyDto>.Success(partyToReturn);
        }
    }
}