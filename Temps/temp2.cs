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
            var mainRole = request.PartyDto.MainRole?.Trim().ToUpper();

            // REFACTOR: Define role configurations per main role - this is the core of generalization
            // Purpose: Centralize all role logic in one place, making it easy to extend (add EMPLOYEE, etc.)
            var roleConfig = mainRole switch
            {
                "CUSTOMER" => new RoleConfig
                {
                    PartyTypeId = "PERSON",
                    RequiresPerson = true,
                    RoleTypeIds = new[] { "CUSTOMER", "BILL_TO_CUSTOMER", "CONTACT", "END_USER_CUSTOMER", "PLACING_CUSTOMER", "SHIP_TO_CUSTOMER" },
                    DescriptionSource = () => request.PartyDto.FirstName,
                    GroupNameSource = () => null
                },
                "CONTRACTOR" => new RoleConfig
                {
                    PartyTypeId = "PARTY_GROUP",
                    RequiresPerson = false,
                    RoleTypeIds = new[] { "CONTRACTOR", "ACCOUNT", "BILL_FROM_VENDOR", "SHIP_FROM_VENDOR", "SUPPLIER_AGENT" },
                    DescriptionSource = () => request.PartyDto.GroupName,
                    GroupNameSource = () => request.PartyDto.GroupName
                },
                "SUPPLIER" => new RoleConfig
                {
                    PartyTypeId = "PARTY_GROUP",
                    RequiresPerson = false,
                    RoleTypeIds = new[] { "SUPPLIER", "ACCOUNT", "BILL_FROM_VENDOR", "SHIP_FROM_VENDOR", "SUPPLIER_AGENT" },
                    DescriptionSource = () => request.PartyDto.GroupName,
                    GroupNameSource = () => request.PartyDto.GroupName
                },
                "EMPLOYEE" => new RoleConfig
                {
                    PartyTypeId = "PERSON",
                    RequiresPerson = true,
                    RoleTypeIds = new[] { "EMPLOYEE", "INTERNAL_ORGANIZATIO" }, // adjust as needed
                    DescriptionSource = () => $"{request.PartyDto.FirstName} {request.PartyDto.LastName}".Trim(),
                    GroupNameSource = () => null
                },
                _ => null
            };

            if (roleConfig == null)
                return Result<PartyDto2>.Failure("Invalid or unsupported MainRole.");

            var transaction = _context.Database.BeginTransaction();

            try
            {
                var stamp = DateTime.Now;
                var newPartyId = await _utilityService.GetNextSequence("Party");

                // Common lookups
                var partyStatusEnabled = await _context.StatusItems.SingleOrDefaultAsync(x => x.StatusId == "PARTY_ENABLED", cancellationToken);
                var partyType = await _context.PartyTypes.SingleOrDefaultAsync(x => x.PartyTypeId == roleConfig.PartyTypeId, cancellationToken);

                if (partyType == null)
                    return Result<PartyDto2>.Failure($"PartyType {roleConfig.PartyTypeId} not found.");

                // Fetch all required roles
                var roleTypes = await _context.RoleTypes
                    .Where(rt => roleConfig.RoleTypeIds.Contains(rt.RoleTypeId))
                    .ToListAsync(cancellationToken);

                if (roleTypes.Count != roleConfig.RoleTypeIds.Length)
                {
                    return Result<PartyDto2>.Failure($"One or more required roles missing for {mainRole}.");
                }

                var primaryRoleType = roleTypes.First(rt => rt.RoleTypeId == mainRole);

                // Create Party
                var party = new Party
                {
                    PartyId = newPartyId,
                    PartyType = partyType,
                    Status = partyStatusEnabled,
                    MainRole = primaryRoleType.RoleTypeId,
                    Description = roleConfig.DescriptionSource(),
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.Parties.Add(party);

                // Assign all roles
                foreach (var roleType in roleTypes)
                {
                    _context.PartyRoles.Add(new PartyRole
                    {
                        Party = party,
                        RoleType = roleType,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });
                }

                // Party Status History
                _context.PartyStatuses.Add(new PartyStatus
                {
                    Party = party,
                    Status = partyStatusEnabled,
                    StatusDate = stamp,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });

                // Person or PartyGroup?
                if (roleConfig.RequiresPerson)
                {
                    _context.Persons.Add(new Person
                    {
                        Party = party,
                        FirstName = request.PartyDto.FirstName,
                        MiddleName = request.PartyDto.MiddleName,
                        LastName = request.PartyDto.LastName,
                        PersonalTitle = request.PartyDto.PersonalTitle,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });
                }
                else
                {
                    var groupName = roleConfig.GroupNameSource();
                    if (string.IsNullOrEmpty(groupName))
                        return Result<PartyDto2>.Failure("GroupName is required for PARTY_GROUP types.");

                    _context.PartyGroups.Add(new PartyGroup
                    {
                        Party = party,
                        GroupName = groupName,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });
                }

                // REFACTOR: Reuse contact creation logic with dynamic primary role
                var primaryPartyRole = _context.PartyRoles.Local
                    .FirstOrDefault(pr => pr.Party == party && pr.RoleType == primaryRoleType)
                    ?? roleTypes.First(rt => rt.RoleTypeId == primaryRoleType); // fallback

                // Contact Mech Types
                var telecomType = await _context.ContactMechTypes.SingleOrDefaultAsync(x => x.ContactMechTypeId == "TELECOM_NUMBER", cancellationToken);
                var emailType = await _context.ContactMechTypes.SingleOrDefaultAsync(x => x.ContactMechTypeId == "EMAIL_ADDRESS", cancellationToken);
                var postalType = await _context.ContactMechTypes.SingleOrDefaultAsync(x => x.ContactMechTypeId == "POSTAL_ADDRESS", cancellationToken);

                var phonePurpose = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(x => x.ContactMechPurposeTypeId == "PRIMARY_PHONE", cancellationToken);
                var emailPurpose = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(x => x.ContactMechPurposeTypeId == "PRIMARY_EMAIL", cancellationToken);
                var generalLocationPurpose = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(x => x.ContactMechPurposeTypeId == "GENERAL_LOCATION", cancellationToken);
                var shippingLocationPurpose = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(x => x.ContactMechPurposeTypeId == "SHIPPING_LOCATION", cancellationToken);

                // Add Mobile
                if (!string.IsNullOrEmpty(request.PartyDto.MobileContactNumber))
                {
                    var cm = await AddTelecomContactMech(party, request.PartyDto.MobileContactNumber, primaryPartyRole, primaryRoleType, phonePurpose, telecomType, stamp);
                    _context.ContactMeches.Add(cm.contactMech);
                    _context.TelecomNumbers.Add(cm.telecomNumber);
                    _context.PartyContactMeches.Add(cm.partyContactMech);
                    _context.PartyContactMechPurposes.Add(cm.purpose);
                }

                // Add Email
                if (!string.IsNullOrEmpty(request.PartyDto.InfoString))
                {
                    var cm = await AddEmailContactMech(party, request.PartyDto.InfoString, primaryPartyRole, primaryRoleType, emailPurpose, emailType, stamp);
                    _context.ContactMeches.Add(cm.contactMech);
                    _context.PartyContactMeches.Add(cm.partyContactMech);
                    _context.PartyContactMechPurposes.Add(cm.purpose);
                }

                // Add Address
                if (!string.IsNullOrEmpty(request.PartyDto.Address1))
                {
                    var cm = await AddPostalContactMech(
                        party,
                        request.PartyDto.FirstName ?? request.PartyDto.GroupName,
                        request.PartyDto.Address1,
                        request.PartyDto.Address2,
                        request.PartyDto.GeoId,
                        primaryPartyRole,
                        primaryRoleType,
                        generalLocationPurpose,
                        shippingLocationPurpose,
                        postalType,
                        stamp);

                    _context.ContactMeches.Add(cm.contactMech);
                    _context.PartyContactMeches.Add(cm.partyContactMech);
                    _context.PartyContactMechPurposes.Add(cm.generalPurpose);
                    _context.PartyContactMechPurposes.Add(cm.shippingPurpose);
                    _context.PostalAddresses.Add(cm.postalAddress);
                }

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!success)
                {
                    transaction.Rollback();
                    return Result<PartyDto2>.Failure($"Failed to create {mainRole}.");
                }

                transaction.Commit();

                return Result<PartyDto2>.Success(new PartyDto2
                {
                    PartyId = newPartyId,
                    Description = $"{party.Description} ({mainRole})",
                    PartyTypeDescription = party.PartyId
                });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Result<PartyDto2>.Failure($"Error creating party: {ex.Message}");
            }
        }

        // REFACTOR: Extracted contact creation into reusable methods
        private async Task<(ContactMech contactMech, TelecomNumber telecomNumber, PartyContactMech partyContactMech, PartyContactMechPurpose purpose)> 
            AddTelecomContactMech(Party party, string number, PartyRole partyRole, RoleType roleType, ContactMechPurposeType purposeType purposeType, ContactMechType mechType, DateTime stamp)
        {
            var cm = new ContactMech
            {
                ContactMechId = Guid.NewGuid().ToString(),
                ContactMechType = mechType,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };

            var telecom = new TelecomNumber
            {
                ContactNumber = number,
                ContactMech = cm,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };

            var pcm = new PartyContactMech
            {
                Party = party,
                ContactMech = cm,
                RoleType = roleType,
                PartyRole = partyRole,
                FromDate = stamp,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };

            var pcmp = new PartyContactMechPurpose
            {
                Party = party,
                ContactMech = cm,
                ContactMechPurposeType = purposeType,
                FromDate = stamp,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };

            return (cm, telecom, pcm, pcmp);
        }

        // Similar for Email & Postal (extracted for clarity)
        // ... (same pattern)

        private class RoleConfig
        {
            public string PartyTypeId { get; set; }
            public bool RequiresPerson { get; set; }
            public string[] RoleTypeIds { get; set; }
            public Func<string> DescriptionSource { get; set; }
            public Func<string> GroupNameSource { get; set; }
        }
    }
}