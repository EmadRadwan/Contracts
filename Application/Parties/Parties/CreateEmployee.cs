using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class CreateEmployee
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

            // REFACTOR: Fetch the required employee role type
            // Purpose: Ensure the new party is assigned the EMPLOYEE role
            // Improvement: Centralizes role fetching and validation
            var roleTypeEmployee = await _context.RoleTypes.SingleOrDefaultAsync(
                x => x.RoleTypeId == "EMPLOYEE", cancellationToken);

            if (roleTypeEmployee == null)
            {
                transaction.Rollback();
                return Result<PartyDto2>.Failure("Required employee role type 'EMPLOYEE' is missing in the database.");
            }

            var contactMechPurposeTypePhoneMobile = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "PRIMARY_PHONE", cancellationToken);

            var contactMechPurposeTypeGeneralLocation = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "GENERAL_LOCATION", cancellationToken);

            var contactMechPurposeTypePrimaryEmail = await _context.ContactMechPurposeTypes.SingleOrDefaultAsync(
                x => x.ContactMechPurposeTypeId == "PRIMARY_EMAIL", cancellationToken);

            var stamp = DateTime.UtcNow;
            var newPartyId = await _utilityService.GetNextSequence("Party");

            var party = new Party
            {
                PartyId = newPartyId,
                PartyType = partyType,
                Status = partyStatusPartyEnabled,
                MainRole = roleTypeEmployee.RoleTypeId,
                Description = request.PartyDto.FirstName,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.Parties.Add(party);

            // REFACTOR: Add PartyRole entry for EMPLOYEE
            // Purpose: Assign the EMPLOYEE role to the new party
            // Improvement: Explicit and consistent role assignment
            var partyRole = new PartyRole
            {
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp,
                Party = party,
                RoleType = roleTypeEmployee
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

                // REFACTOR: Use the EMPLOYEE role's PartyRole for contact mechanisms
                var partyRoleEmployee =
                    _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeEmployee);

                var partyContactMech = new PartyContactMech
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    Party = party,
                    PartyRole = partyRoleEmployee,
                    RoleType = roleTypeEmployee
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

                var partyRoleEmployee =
                    _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeEmployee);

                var partyContactMech = new PartyContactMech
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    Party = party,
                    PartyRole = partyRoleEmployee,
                    RoleType = roleTypeEmployee
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

                var partyRoleEmployee =
                    _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeEmployee);

                var partyContactMech = new PartyContactMech
                {
                    FromDate = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp,
                    ContactMech = contactMech,
                    Party = party,
                    PartyRole = partyRoleEmployee,
                    RoleType = roleTypeEmployee
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

            // ────────────────────────────────────────────────────────────────
            // 2. NEW: Employment
            // ────────────────────────────────────────────────────────────────
            var employment = new Employment
            {
                PartyIdFrom = "Company",
                PartyIdTo = newPartyId,
                FromDate = stamp,
                ThruDate = null,
                RoleTypeIdFrom = "INTERNAL_ORGANIZATIO",
                RoleTypeIdTo = "EMPLOYEE",
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.Employments.Add(employment);

            // ────────────────────────────────────────────────────────────────
            // 3. NEW: Position Fulfillment
            // ────────────────────────────────────────────────────────────────
            string positionId;

            if (string.IsNullOrEmpty(request.PartyDto.EmplPositionTypeId))
                return Result<PartyDto2>.Failure("EmplPositionTypeId is required.");

            // Validate type exists
            var typeExists = await _context.EmplPositionTypes
                .AnyAsync(t => t.EmplPositionTypeId == request.PartyDto.EmplPositionTypeId, cancellationToken);
            if (!typeExists)
                return Result<PartyDto2>.Failure($"Position type {request.PartyDto.EmplPositionTypeId} not found.");

            positionId = await _utilityService.GetNextSequence("EmplPosition"); // or custom naming

            var newPosition = new EmplPosition
            {
                EmplPositionId = positionId,
                StatusId = "EMPL_POS_ACTIVE",
                PartyId = "Company",
                EmplPositionTypeId = request.PartyDto.EmplPositionTypeId,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.EmplPositions.Add(newPosition);

            var fulfillment = new EmplPositionFulfillment
            {
                EmplPositionId = positionId,
                PartyId = newPartyId,
                FromDate = stamp,
                ThruDate = null,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.EmplPositionFulfillments.Add(fulfillment);

            // ────────────────────────────────────────────────────────────────
            // 4. NEW: Personal RateAmount (monthly override)
            // ────────────────────────────────────────────────────────────────
            if (request.PartyDto.MonthlyBaseSalary.HasValue && request.PartyDto.MonthlyBaseSalary > 0)
            {
                var rate = new RateAmount
                {
                    RateTypeId = "AVERAGE_PAY_RATE", // or your preferred type
                    RateCurrencyUomId = "EGP",
                    PeriodTypeId = "RATE_MONTH",
                    WorkEffortId = "_NA_",
                    PartyId = newPartyId, // personal
                    EmplPositionTypeId = "_NA_",
                    FromDate = stamp,
                    Amount = request.PartyDto.MonthlyBaseSalary.Value,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.RateAmounts.Add(rate);
            }

            // ────────────────────────────────────────────────────────────────
            // 5. NEW: Reporting Structure
            // ────────────────────────────────────────────────────────────────
            if (!string.IsNullOrEmpty(request.PartyDto.ReportingToPartyId))
            {
                var managerPosition = await _context.EmplPositions
                    .Where(p => p.PartyId == "Company" && p.StatusId == "EMPL_POS_ACTIVE")
                    .Join(_context.EmplPositionFulfillments,
                        p => p.EmplPositionId,
                        f => f.EmplPositionId,
                        (p, f) => new { p, f })
                    .Where(pf => pf.f.PartyId == request.PartyDto.ReportingToPartyId && pf.f.ThruDate == null)
                    .Select(pf => pf.p)
                    .FirstOrDefaultAsync(cancellationToken);

                if (managerPosition != null)
                {
                    var reportingStruct = new EmplPositionReportingStruct
                    {
                        EmplPositionIdManagedBy = positionId,
                        EmplPositionIdReportingTo = managerPosition.EmplPositionId,
                        FromDate = stamp,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };
                    _context.EmplPositionReportingStructs.Add(reportingStruct);
                }
            }

            // ────────────────────────────────────────────────────────────────
            // CREATE TWO GL ACCOUNTS
            // ────────────────────────────────────────────────────────────────

            var glAccountsCreated =
                new List<(string GlAccountId, string Type, string Name, string ArabicName, string Parent)>();

            // Helper to generate unique ID
            async Task<string?> GenerateUniqueGlId(string prefix, int digits = 4, int maxAttempts = 900)
            {
                int suffix = 1;
                for (int i = 0; i < maxAttempts; i++)
                {
                    var candidate = $"{prefix}{suffix.ToString().PadLeft(digits, '0')}";
                    if (!await _context.GlAccounts.AnyAsync(a => a.GlAccountId == candidate, cancellationToken))
                        return candidate;
                    suffix++;
                }

                return null;
            }

            // 1. Loans Receivable - Employees (Asset)
            var loanPrefix = "12";
            var loanId = await GenerateUniqueGlId(loanPrefix, 4);
            if (loanId == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure("Could not generate unique Loan Receivable GL ID.");
            }

            var loanAccount = new GlAccount
            {
                GlAccountId = loanId,
                GlAccountTypeId = "ACCOUNTS_RECEIVABLE",
                GlAccountClassId = "CURRENT_ASSET",
                GlResourceTypeId = "MONEY",
                ParentGlAccountId = "124100",
                AccountCode = loanId,
                AccountName = $"Loans Receivable - {request.PartyDto.FirstName} ({newPartyId})",
                AccountNameArabic = $"ذمم الموظفين - {request.PartyDto.FirstName}",
                Description = $"Employee loans receivable sub-ledger",
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.GlAccounts.Add(loanAccount);

            var loanOrg = new GlAccountOrganization
            {
                GlAccountId = loanId,
                OrganizationPartyId = "Company",
                RoleTypeId = null,
                FromDate = stamp,
                ThruDate = null,
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.GlAccountOrganizations.Add(loanOrg);

            var loanPartyGl = new PartyGlAccount
            {
                OrganizationPartyId = "Company",
                PartyId = newPartyId,
                RoleTypeId = "EMPLOYEE",
                GlAccountTypeId = "ACCOUNTS_RECEIVABLE",
                GlAccountId = loanId,
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.PartyGlAccounts.Add(loanPartyGl);

            glAccountsCreated.Add((loanId, "Loans Receivable", loanAccount.AccountName, loanAccount.AccountNameArabic,
                "124100"));

            // 2. Accrued Expenses - Employees (Liability)
            var accruedPrefix = "22";
            var accruedId = await GenerateUniqueGlId(accruedPrefix);
            if (accruedId == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure("Could not generate unique Accrued Expenses GL ID.");
            }

            var accruedAccount = new GlAccount
            {
                GlAccountId = accruedId,
                GlAccountTypeId = "ACCOUNTS_PAYABLE",
                GlAccountClassId = "CURRENT_LIABILITY",
                GlResourceTypeId = "MONEY",
                ParentGlAccountId = "220000",
                AccountCode = accruedId,
                AccountName = $"Accrued Salaries - {request.PartyDto.FirstName} ({newPartyId})",
                AccountNameArabic = $"مستحقات رواتب - {request.PartyDto.FirstName}",
                Description = $"Employee accrued expenses / salaries payable",
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.GlAccounts.Add(accruedAccount);

            var accruedOrg = new GlAccountOrganization
            {
                GlAccountId = accruedId,
                OrganizationPartyId = "Company",
                RoleTypeId = null,
                FromDate = stamp,
                ThruDate = null,
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.GlAccountOrganizations.Add(accruedOrg);

            var accruedPartyGl = new PartyGlAccount
            {
                OrganizationPartyId = "Company",
                PartyId = newPartyId,
                RoleTypeId = "EMPLOYEE",
                GlAccountTypeId = "ACCOUNTS_PAYABLE",
                GlAccountId = accruedId,
                CreatedStamp = stamp,
                CreatedTxStamp = stamp,
                LastUpdatedStamp = stamp,
                LastUpdatedTxStamp = stamp
            };
            _context.PartyGlAccounts.Add(accruedPartyGl);

            glAccountsCreated.Add((accruedId, "Accrued Expenses", accruedAccount.AccountName,
                accruedAccount.AccountNameArabic, "220000"));


            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure("Failed to create Employee");
            }

            await transaction.CommitAsync(cancellationToken);

            var partyToReturn = new PartyDto2
            {
                PartyId = newPartyId,
                Description = request.PartyDto.FirstName + " ( " + roleTypeEmployee.RoleTypeId + " )",
                PartyTypeDescription = partyStatus.PartyId,
                FromPartyId = new FromPartyDto
                {
                    FromPartyId = party.PartyId,
                    FromPartyName = party.Description
                },
                CreatedLoanGlAccountId = loanId,
                CreatedAccruedGlAccountId = accruedId,
                CreatedLoanGlAccountName = $"AP - {request.PartyDto.GroupName} ({newPartyId})",
                CreatedLoanGlAccountArabicName = $"ذمم الموظفين - {request.PartyDto.FirstName}",
                CreatedAccruedGlAccountName = $"AP - {request.PartyDto.GroupName} ({newPartyId})",
                CreatedAccruedGlAccountArabicName = $"مستحقات رواتب - {request.PartyDto.FirstName}",
            };

            return Result<PartyDto2>.Success(partyToReturn);
        }
    }
}