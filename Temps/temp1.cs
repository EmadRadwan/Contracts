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
            _context = context;
            _userAccessor = userAccessor;
            _utilityService = utilityService;
        }

        public async Task<Result<PartyDto2>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var stamp = DateTime.UtcNow; // ← prefer UTC in most cases

            var partyStatusPartyEnabled = await _context.StatusItems
                .SingleOrDefaultAsync(x => x.StatusId == "PARTY_ENABLED", cancellationToken);

            if (partyStatusPartyEnabled == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure("PARTY_ENABLED status not found.");
            }

            var mainRole = request.PartyDto.MainRole?.Trim().ToUpperInvariant();

            if (string.IsNullOrEmpty(mainRole) ||
                !new[] { "CUSTOMER", "SUPPLIER", "CONTRACTOR" }.Contains(mainRole))
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure("Invalid or missing MainRole.");
            }

            // ───────────────────────────────────────────────
            // Determine party type & GL settings
            // ───────────────────────────────────────────────
            bool isPerson = mainRole == "CUSTOMER";
            string partyTypeId = isPerson ? "PERSON" : "PARTY_GROUP";

            var partyType = await _context.PartyTypes
                .SingleOrDefaultAsync(x => x.PartyTypeId == partyTypeId, cancellationToken);

            if (partyType == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure($"PartyType {partyTypeId} not found.");
            }

            // GL configuration per role
            (string Prefix, string GlType, string RoleForLink, string AccountNamePrefix, string ArabicPrefix, string ParentAccountId) glConfig = mainRole switch
            {
                "CUSTOMER"   => ("12",  "ACCOUNTS_RECEIVABLE", "BILL_TO_CUSTOMER",   "AR - ", "مدينون - ", "121100"),
                "SUPPLIER"   => ("21",  "ACCOUNTS_PAYABLE",    "BILL_FROM_VENDOR",   "AP - ", "الدائنون - ", "210000"),
                "CONTRACTOR" => ("21",  "ACCOUNTS_PAYABLE",    "BILL_FROM_VENDOR",   "AP - ", "المقاولون - ", "210000"),
                _            => throw new InvalidOperationException("Unhandled mainRole")
            };

            // ───────────────────────────────────────────────
            // Role assignment
            // ───────────────────────────────────────────────
            string[] roleTypeIds = mainRole switch
            {
                "CUSTOMER"   => new[] { "CUSTOMER", "BILL_TO_CUSTOMER", "CONTACT", "END_USER_CUSTOMER", "PLACING_CUSTOMER", "SHIP_TO_CUSTOMER" },
                "SUPPLIER"   => new[] { "SUPPLIER", "ACCOUNT", "BILL_FROM_VENDOR", "SHIP_FROM_VENDOR", "SUPPLIER_AGENT" },
                "CONTRACTOR" => new[] { "CONTRACTOR", "ACCOUNT", "BILL_FROM_VENDOR", "SHIP_FROM_VENDOR", "SUPPLIER_AGENT" },
                _            => Array.Empty<string>()
            };

            var roleTypes = await _context.RoleTypes
                .Where(x => roleTypeIds.Contains(x.RoleTypeId))
                .ToListAsync(cancellationToken);

            if (roleTypes.Count != roleTypeIds.Length)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure("One or more required roles are missing in database.");
            }

            var primaryRoleType = roleTypes.Single(x => x.RoleTypeId == mainRole);

            // ───────────────────────────────────────────────
            // Create Party
            // ───────────────────────────────────────────────
            var newPartyId = await _utilityService.GetNextSequence("Party");

            string description = request.PartyDto.FirstName?.Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                await transaction.RollbackAsync(cancellationToken);
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

            // Assign roles
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

            _context.PartyStatuses.Add(new PartyStatus
            {
                Party = party,
                Status = partyStatusPartyEnabled,
                StatusDate = stamp,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });

            // Person / Group
            if (isPerson)
            {
                _context.Persons.Add(new Person
                {
                    Party = party,
                    FirstName = description,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });
            }
            else
            {
                _context.PartyGroups.Add(new PartyGroup
                {
                    Party = party,
                    GroupName = description,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });
            }

            // ───────────────────────────────────────────────
            // Contact mechanisms (same as before)
            // ───────────────────────────────────────────────
            var contactMechTypes = await _context.ContactMechTypes
                .Where(x => x.ContactMechTypeId.In("TELECOM_NUMBER", "EMAIL_ADDRESS", "POSTAL_ADDRESS"))
                .ToDictionaryAsync(x => x.ContactMechTypeId, cancellationToken);

            var purposeTypes = await _context.ContactMechPurposeTypes
                .Where(x => x.ContactMechPurposeTypeId.In("PRIMARY_PHONE", "PRIMARY_EMAIL", "GENERAL_LOCATION", "SHIPPING_LOCATION"))
                .ToDictionaryAsync(x => x.ContactMechPurposeTypeId, cancellationToken);

            var primaryPartyRole = new PartyRole { Party = party, RoleType = primaryRoleType }; // temp – will be replaced after save if needed

            // Mobile
            if (!string.IsNullOrWhiteSpace(request.PartyDto.MobileContactNumber))
            {
                var cm = new ContactMech { ContactMechId = Guid.NewGuid().ToString(), ContactMechTypeId = "TELECOM_NUMBER", CreatedStamp = stamp, LastUpdatedStamp = stamp };
                _context.ContactMeches.Add(cm);

                _context.TelecomNumbers.Add(new TelecomNumber
                {
                    ContactMech = cm,
                    ContactNumber = request.PartyDto.MobileContactNumber,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });

                var pcm = new PartyContactMech { Party = party, ContactMech = cm, RoleTypeId = primaryRoleType.RoleTypeId, FromDate = stamp, CreatedStamp = stamp, LastUpdatedStamp = stamp };
                _context.PartyContactMeches.Add(pcm);

                _context.PartyContactMechPurposes.Add(new PartyContactMechPurpose
                {
                    Party = party,
                    ContactMech = cm,
                    ContactMechPurposeTypeId = "PRIMARY_PHONE",
                    FromDate = stamp,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });
            }

            // Email + Address → same pattern (omitted for brevity – copy from your original or previous version)

            // ───────────────────────────────────────────────
            // Create sub-ledger account
            // ───────────────────────────────────────────────
            string? createdGlId = null;
            string? createdGlName = null;
            string? createdGlArabic = null;

            string candidate = null;
            const int maxAttempts = 900;
            int suffix = 1;

            for (int i = 0; i < maxAttempts; i++)
            {
                candidate = $"{glConfig.Prefix}{suffix.ToString().PadLeft(4, '0')}";
                if (!await _context.GlAccounts.AnyAsync(a => a.GlAccountId == candidate, cancellationToken))
                    break;
                suffix++;
            }

            if (candidate == null || suffix > maxAttempts)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure($"Could not generate unique GL ID after {maxAttempts} attempts.");
            }

            var glAccount = new GlAccount
            {
                GlAccountId        = candidate,
                GlAccountTypeId    = glConfig.GlType,
                GlAccountClassId   = mainRole == "CUSTOMER" ? "CURRENT_ASSET" : "CURRENT_LIABILITY",
                GlResourceTypeId   = "MONEY",
                ParentGlAccountId  = glConfig.ParentAccountId,
                AccountCode        = candidate,
                AccountName        = $"{glConfig.AccountNamePrefix}{description} ({newPartyId})",
                AccountNameArabic  = $"{glConfig.ArabicPrefix}{description}",
                Description        = $"{glConfig.GlType} sub-ledger for {mainRole.ToLower()} {newPartyId} - {description}",
                CreatedStamp       = stamp,
                LastUpdatedStamp   = stamp,
                CreatedTxStamp     = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.GlAccounts.Add(glAccount);

            _context.GlAccountOrganizations.Add(new GlAccountOrganization
            {
                GlAccountId = candidate,
                OrganizationPartyId = "Company",
                FromDate = stamp,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });

            _context.PartyGlAccounts.Add(new PartyGlAccount
            {
                OrganizationPartyId = "Company",
                PartyId = newPartyId,
                RoleTypeId = glConfig.RoleForLink,
                GlAccountTypeId = glConfig.GlType,
                GlAccountId = candidate,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });

            createdGlId     = candidate;
            createdGlName   = glAccount.AccountName;
            createdGlArabic = glAccount.AccountNameArabic;

            // ───────────────────────────────────────────────
            // Final save
            // ───────────────────────────────────────────────
            var success = await _context.SaveChangesAsync(cancellationToken) > 0;
            if (!success)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure($"Failed to create {mainRole}.");
            }

            await transaction.CommitAsync(cancellationToken);

            // Return DTO
            var dto = new PartyDto2
            {
                PartyId = newPartyId,
                Description = $"{description} ({mainRole})",
                // ... other fields you need ...
                CreatedGlAccountId = createdGlId,
                CreatedGlAccountName = createdGlName,
                CreatedGlAccountArabicName = createdGlArabic,
                // Add more fields like CreatedApGlAccountId if you want to unify naming
            };

            return Result<PartyDto2>.Success(dto);
        }
    }
}