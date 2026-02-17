using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class CreateSupplier
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

            var stamp = DateTime.UtcNow;
            var newPartyId = await _utilityService.GetNextSequence("Party");

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
                    PartyRole = _context.PartyRoles.FirstOrDefault(pr =>
                        pr.Party == party && pr.RoleType == roleTypeSupplier), // Use SUPPLIER role
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
                    PartyRole = _context.PartyRoles.FirstOrDefault(pr =>
                        pr.Party == party && pr.RoleType == roleTypeSupplier), // Use SUPPLIER role
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
                    PartyRole = _context.PartyRoles.FirstOrDefault(pr =>
                        pr.Party == party && pr.RoleType == roleTypeSupplier), // Use SUPPLIER role
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

            bool apCreated = false;
            string? newApGlAccountId = null;

            const string prefix = "21"; // ← choose your prefix: 2105, 2110, 2001 etc.210000
            const int digits = 4;
            const int maxAttempts = 900;
            int suffix = 1;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                string candidate = $"{prefix}{suffix.ToString().PadLeft(digits, '0')}";

                bool exists = await _context.GlAccounts
                    .AnyAsync(a => a.GlAccountId == candidate, cancellationToken);

                if (!exists)
                {
                    newApGlAccountId = candidate;
                    break;
                }

                suffix++;
            }

            if (newApGlAccountId == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure(
                    $"Could not generate unique AP GL account ID after {maxAttempts} attempts.");
            }

            // 1. Create GlAccount
            var newApAccount = new GlAccount
            {
                GlAccountId = newApGlAccountId,
                GlAccountTypeId = "ACCOUNTS_PAYABLE",
                GlAccountClassId = "CURRENT_LIABILITY", // or LIABILITY – check your chart
                GlResourceTypeId = "MONEY",
                ParentGlAccountId = "210000", // or "210000" – your AP parent
                AccountCode = newApGlAccountId,
                AccountName = $"AP - {request.PartyDto.GroupName} ({newPartyId})",
                AccountNameArabic = $"الدائنون - {request.PartyDto.GroupName}",
                Description = $"Accounts Payable sub-ledger for supplier {newPartyId} - {request.PartyDto.GroupName}",
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.GlAccounts.Add(newApAccount);

            // 2. GlAccountOrganization
            var glOrgAp = new GlAccountOrganization
            {
                GlAccountId = newApGlAccountId,
                OrganizationPartyId = "Company",
                RoleTypeId = null,
                FromDate = stamp,
                ThruDate = null,
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.GlAccountOrganizations.Add(glOrgAp);

            // 3. PartyGlAccount
            var partyGlAp = new PartyGlAccount
            {
                OrganizationPartyId = "Company",
                PartyId = newPartyId,
                RoleTypeId = "BILL_FROM_VENDOR",
                GlAccountTypeId = "ACCOUNTS_PAYABLE",
                GlAccountId = newApGlAccountId,
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.PartyGlAccounts.Add(partyGlAp);

            apCreated = true;

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure("Failed to create Supplier");
            }

            await transaction.CommitAsync(cancellationToken);

            var partyToReturn = new PartyDto2
            {
                PartyId = newPartyId,
                Description = request.PartyDto.FirstName + " ( " + roleTypeSupplier?.RoleTypeId + " )",
                PartyTypeDescription = partyStatus.PartyId,
                CreatedApGlAccountId = apCreated ? newApGlAccountId : null,
                CreatedApGlAccountName = apCreated ? $"AP - {request.PartyDto.GroupName} ({newPartyId})" : null,
                CreatedApGlAccountArabicName = apCreated ? $"الدائنون - {request.PartyDto.GroupName}" : null,
            };
            return Result<PartyDto2>.Success(partyToReturn);
        }
    }
}