using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class CreateParty
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
            _userAccessor = userAccessor;
            _context = context;
            _utilityService = utilityService;
        }

        public async Task<Result<PartyDto2>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername(), cancellationToken);

            var partyStatusPartyEnabled = await _context.StatusItems.SingleOrDefaultAsync(x =>
                x.StatusId == "PARTY_ENABLED", cancellationToken);

            // REFACTOR: Dynamically determine PartyType based on MainRole
            // PERSON for CUSTOMER/EMPLOYEE, PARTY_GROUP for SUPPLIER/CONTRACTOR
            var mainRole = request.PartyDto.MainRole?.Trim().ToUpper();
            var isPerson = mainRole is "CUSTOMER" or "EMPLOYEE";
            var partyTypeId = isPerson ? "PERSON" : "PARTY_GROUP";

            var partyType = await _context.PartyTypes.SingleOrDefaultAsync(
                x => x.PartyTypeId == partyTypeId, cancellationToken);

            if (partyType == null)
            {
                transaction.Rollback();
                return Result<PartyDto2>.Failure($"PartyType {partyTypeId} not found.");
            }

            var contactMechTypeTelCommNumber = await _context.ContactMechTypes.SingleOrDefaultAsync(
                x => x.ContactMechTypeId == "TELECOM_NUMBER", cancellationToken);

            var contactMechTypeEmailAddress = await _context.ContactMechTypes.SingleOrDefaultAsync(
                x => x.ContactMechTypeId == "EMAIL_ADDRESS", cancellationToken);

            var contactMechTypePostalAddress = await _context.ContactMechTypes.SingleOrDefaultAsync(
                x => x.ContactMechTypeId == "POSTAL_ADDRESS", cancellationToken);

            // REFACTOR: Dynamic role assignment based on MainRole from frontend
            // This replaces the hardcoded customer roles
            string[] roleTypeIds = mainRole switch
            {
                "CUSTOMER" => new[] { "CUSTOMER", "BILL_TO_CUSTOMER", "CONTACT", "END_USER_CUSTOMER", "PLACING_CUSTOMER", "SHIP_TO_CUSTOMER" },
                "CONTRACTOR" => new[] { "CONTRACTOR", "ACCOUNT", "BILL_FROM_VENDOR", "SHIP_FROM_VENDOR", "SUPPLIER_AGENT" },
                "SUPPLIER" => new[] { "SUPPLIER", "ACCOUNT", "BILL_FROM_VENDOR", "SHIP_FROM_VENDOR", "SUPPLIER_AGENT" },
                "EMPLOYEE" => new[] { "EMPLOYEE", "INTERNAL_ORGANIZATIO" }, // adjust if needed
                _ => Array.Empty<string>()
            };

            if (roleTypeIds.Length == 0)
            {
                transaction.Rollback();
                return Result<PartyDto2>.Failure("Invalid or unsupported MainRole.");
            }

            var roleTypes = await _context.RoleTypes
                .Where(x => roleTypeIds.Contains(x.RoleTypeId))
                .ToListAsync(cancellationToken);

            if (roleTypes.Count != roleTypeIds.Length)
            {
                transaction.Rollback();
                return Result<PartyDto2>.Failure($"One or more required roles for {mainRole} are missing in database.");
            }

            // This is the primary role used in contact mechanisms and MainRole field
            var primaryRoleType = roleTypes.Single(x => x.RoleTypeId == mainRole);

            var contactMechPurposeTypePhoneMobile = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "PRIMARY_PHONE", cancellationToken);

            var contactMechPurposeTypeGeneralLocation = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "GENERAL_LOCATION", cancellationToken);

            var contactMechPurposeTypeShippingLocation = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "SHIPPING_LOCATION", cancellationToken);

            var contactMechPurposeTypePrimaryEmail = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "PRIMARY_EMAIL", cancellationToken);

            var stamp = DateTime.Now;
            var newPartyId = await _utilityService.GetNextSequence("Party");

            // Description logic: use FirstName for PERSON, GroupName for PARTY_GROUP
            var description = request.PartyDto.FirstName;

            if (string.IsNullOrEmpty(description))
            {
                transaction.Rollback();
                return Result<PartyDto2>.Failure("Name is required.");
            }

            var party = new Party
            {
                PartyId = newPartyId,
                PartyType = partyType,
                Status = partyStatusPartyEnabled,
                MainRole = primaryRoleType.RoleTypeId,
                Description = description,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.Parties.Add(party);

            // Assign all roles for this party type
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

            // Create Person or PartyGroup based on type
            if (isPerson)
            {
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
            }
            else
            {
                if (string.IsNullOrEmpty(request.PartyDto.FirstName))
                {
                    transaction.Rollback();
                    return Result<PartyDto2>.Failure("GroupName is required for Supplier/Contractor.");
                }

                var partyGroup = new PartyGroup
                {
                    GroupName = request.PartyDto.FirstName,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp,
                    Party = party
                };
                _context.PartyGroups.Add(partyGroup);
            }

            // Use primary role for all contact mechanisms (consistent with original logic)
            var primaryPartyRole = _context.PartyRoles
                .FirstOrDefault(pr => pr.Party == party && pr.RoleType == primaryRoleType);

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
                    PartyRole = primaryPartyRole,
                    RoleType = primaryRoleType
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
                    PartyRole = primaryPartyRole,
                    RoleType = primaryRoleType
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
                    PartyRole = primaryPartyRole,
                    RoleType = primaryRoleType
                };
                _context.PartyContactMeches.Add(partyContactMech);

                var generalPurpose = new PartyContactMechPurpose
                {
                    FromDate = stamp,
                    ContactMech = contactMech,
                    ContactMechPurposeType = contactMechPurposeTypeGeneralLocation,
                    Party = party,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.PartyContactMechPurposes.Add(generalPurpose);

                var shippingPurpose = new PartyContactMechPurpose
                {
                    FromDate = stamp,
                    ContactMech = contactMech,
                    ContactMechPurposeType = contactMechPurposeTypeShippingLocation,
                    Party = party,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.PartyContactMechPurposes.Add(shippingPurpose);

                var postalAddress = new PostalAddress
                {
                    ContactMech = contactMech,
                    ToName = isPerson ? request.PartyDto.FirstName : request.PartyDto.GroupName,
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
                return Result<PartyDto2>.Failure($"Failed to create {mainRole}.");
            }

            transaction.Commit();

            var partyToReturn = new PartyDto2
            {
                PartyId = newPartyId,
                Description = $"{description} ({mainRole})",
                PartyTypeDescription = party.PartyId
            };

            return Result<PartyDto2>.Success(partyToReturn);
        }
    }
}