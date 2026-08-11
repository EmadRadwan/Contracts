using Application.Accounting.Services;
using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class CreateCustomer
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
            var roleTypeIds = new[]
            {
                "BILL_TO_CUSTOMER", "CONTACT", "CUSTOMER", "END_USER_CUSTOMER", "PLACING_CUSTOMER", "SHIP_TO_CUSTOMER"
            };
            var roleTypes = await _context.RoleTypes
                .Where(x => roleTypeIds.Contains(x.RoleTypeId))
                .ToListAsync(cancellationToken);

            // REFACTOR: Validate that all required roles are found
            // Purpose: Prevent partial role assignment if any role type is missing
            // Improvement: Adds robustness by ensuring all expected roles exist in the database
            if (roleTypes.Count != roleTypeIds.Length)
            {
                transaction.Rollback();
                return Result<PartyDto2>.Failure(
                    "One or more required customer role types are missing in the database.");
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

            var stamp = DateTime.UtcNow; // e.g., 2025-07-16 15:53:00 EEST
            var newPartyId = await _utilityService.GetNextSequence("Party");

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
                var partyRoleCustomer =
                    _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeCustomer);
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
                var partyRoleCustomer =
                    _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeCustomer);
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
                var partyRoleCustomer =
                    _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeCustomer);
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

            // === Automatic creation of per-customer AR sub-account ===

            // 1. Generate unique GL Account ID
            const string prefix = "12";
            const int digits = 4; // controls zero-padding: D4 → 0001, D6 → 000001, etc.
            const int maxAttempts = 900; // adjust higher if you expect many collisions
            int suffix = 1;
            string newGlAccountId = null;

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
                transaction.Rollback();
                return Result<PartyDto2>.Failure(
                    $"Failed to generate unique GL account ID under {prefix} after {maxAttempts} attempts."
                );
            }

            // 2. Create GlAccount
            var newGlAccount = new GlAccount
            {
                GlAccountId = newGlAccountId,
                GlAccountTypeId = "ACCOUNTS_RECEIVABLE",
                GlAccountClassId = "CURRENT_ASSET",
                GlResourceTypeId = "MONEY",
                GlXbrlClassId = null,
                ParentGlAccountId = "121100", // or "121100" — your choice
                AccountCode = newGlAccountId,
                AccountName = $"AR - {request.PartyDto.FirstName} ({newPartyId})",
                AccountNameArabic = $"مدينون - {request.PartyDto.FirstName}",
                Description = $"Accounts Receivable sub-ledger for customer {newPartyId}",
                ProductId = null,
                ExternalId = null,
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            // Stamp the six reporting levels so the account is visible to Dim_gl_account
            // (and therefore to Power BI). Derived from ParentGlAccountId — see
            // GlAccountClassificationDefaults for why this is centralised.
            GlAccountClassificationDefaults.Apply(newGlAccount);
            _context.GlAccounts.Add(newGlAccount);

            // 3. Attach to organization
            var glOrg = new GlAccountOrganization
            {
                GlAccountId = newGlAccountId,
                OrganizationPartyId = "Company",
                RoleTypeId = null,
                FromDate = stamp, // or new DateTime(2001, 1, 1)
                ThruDate = null,
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.GlAccountOrganizations.Add(glOrg);

                // 4. Link in PartyGlAccount
            var partyGl = new PartyGlAccount
            {
                OrganizationPartyId = "Company",
                PartyId = newPartyId,
                RoleTypeId = "BILL_TO_CUSTOMER",
                GlAccountTypeId = "ACCOUNTS_RECEIVABLE",
                GlAccountId = newGlAccountId,
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.PartyGlAccounts.Add(partyGl);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure("Failed to create Customer");
            }

            await transaction.CommitAsync(cancellationToken);

            var partyToReturn = new PartyDto2
            {
                PartyId = newPartyId,
                Description = request.PartyDto.FirstName + " ( " + roleTypeCustomer?.RoleTypeId + " )",
                PartyTypeDescription = partyStatus.PartyId,
                FromPartyId = new FromPartyDto
                {
                    FromPartyId = party.PartyId,
                    FromPartyName = party.Description
                },
                CreatedGlAccountId = newGlAccountId,                    // from the generation logic
                CreatedGlAccountName = $"AR - {request.PartyDto.FirstName} ({newPartyId})",
                CreatedGlAccountArabicName = $"مدينون - {request.PartyDto.FirstName}",
                GlAccountType = "ACCOUNTS_RECEIVABLE",
                ParentGlAccountId = "121100"
            };
            return Result<PartyDto2>.Success(partyToReturn);
        }
    }
}