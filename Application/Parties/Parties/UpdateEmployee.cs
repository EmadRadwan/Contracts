using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class UpdateEmployee
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

            var party = await _context.Parties
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.PartyId == request.PartyDto.PartyId, cancellationToken);

            if (party == null)
                return Result<PartyDto2>.Failure("Employee not found");

            var stamp = DateTime.UtcNow;

            party.LastUpdatedStamp = stamp;
            party.Description = request.PartyDto.FirstName;
            party.GlAccountIdAdvancedPayment = request.PartyDto.GlAccountIdAdvancedPayment;
            party.PreferredPayrollPaymentMethodId = request.PartyDto.PreferredPayrollPaymentMethodId;

            if (party.Person != null)
            {
                party.Person.FirstName = request.PartyDto.FirstName?.Trim();
                party.Person.MiddleName = request.PartyDto.MiddleName?.Trim();
                party.Person.PersonalTitle = request.PartyDto.PersonalTitle?.Trim();
                party.Person.LastUpdatedStamp = stamp;
            }

            var telcomNumberQuery = from prty in _context.Parties
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                select tn;


            var primaryTelcomNumber = telcomNumberQuery.FirstOrDefault(); // changed to FirstOrDefault

            if (!string.IsNullOrWhiteSpace(request.PartyDto.MobileContactNumber))
            {
                if (primaryTelcomNumber != null)
                {
                    // update existing
                    primaryTelcomNumber.ContactNumber = request.PartyDto.MobileContactNumber;
                }
                else
                {
                    // create new (copy pattern from CreateEMPLOYEE)
                    var contactMech = new ContactMech
                    {
                        ContactMechId = Guid.NewGuid().ToString(),
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMechTypeId = "TELECOM_NUMBER"
                    };
                    _context.ContactMeches.Add(contactMech);

                    var telecomNumber = new TelecomNumber
                    {
                        ContactMech = contactMech,
                        ContactNumber = request.PartyDto.MobileContactNumber,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp
                    };
                    _context.TelecomNumbers.Add(telecomNumber);

                    var partyContactMech = new PartyContactMech
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        Party = party,
                        RoleTypeId = "EMPLOYEE" // or whatever role you use consistently
                    };
                    _context.PartyContactMeches.Add(partyContactMech);

                    var partyContactMechPurpose = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "PRIMARY_PHONE",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurpose);
                }
            }

            // ───────────────────────────────────────────────
            // POSTAL ADDRESS (GENERAL_LOCATION)
            // ───────────────────────────────────────────────
            var currentPostalAddressQuery = from prty in _context.Parties
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pa in _context.PostalAddresses on cm.ContactMechId equals pa.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals new
                    { pcmp.PartyId, pcmp.ContactMechId }
                where prty.PartyId == request.PartyDto.PartyId
                      && pcmp.ContactMechPurposeTypeId == "GENERAL_LOCATION"
                select pa;

            var generalLocation = currentPostalAddressQuery.FirstOrDefault();

            bool hasAddressData = !string.IsNullOrWhiteSpace(request.PartyDto.Address1) ||
                                  !string.IsNullOrWhiteSpace(request.PartyDto.Address2) ||
                                  !string.IsNullOrWhiteSpace(request.PartyDto.GeoId);

            if (hasAddressData)
            {
                if (generalLocation != null)
                {
                    generalLocation.Address1 = request.PartyDto.Address1 ?? generalLocation.Address1;
                    generalLocation.Address2 = request.PartyDto.Address2 ?? generalLocation.Address2;
                    generalLocation.ToName = request.PartyDto.FirstName ?? generalLocation.ToName;
                    generalLocation.CountryGeoId = request.PartyDto.GeoId ?? generalLocation.CountryGeoId;
                }
                else if (!string.IsNullOrWhiteSpace(request.PartyDto.Address1)) // create only if meaningful data
                {
                    var contactMech = new ContactMech
                    {
                        ContactMechId = Guid.NewGuid().ToString(),
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMechTypeId = "POSTAL_ADDRESS"
                    };
                    _context.ContactMeches.Add(contactMech);

                    var roleTypeEmployee = await _context.RoleTypes.SingleOrDefaultAsync(
                        x => x.RoleTypeId == "EMPLOYEE", cancellationToken);


                    var partyRoleEmployee =
                        _context.PartyRoles.FirstOrDefault(pr => pr.Party == party && pr.RoleType == roleTypeEmployee);


                    var partyContactMech = new PartyContactMech
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        PartyRole = partyRoleEmployee,
                        RoleType = roleTypeEmployee
                    };
                    _context.PartyContactMeches.Add(partyContactMech);

                    var partyContactMechPurposeGeneral = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "GENERAL_LOCATION",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurposeGeneral);

                    var partyContactMechPurposeShipping = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "SHIPPING_LOCATION",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurposeShipping);

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
            }

            var currentContactMechQuery = from prty in _context.Parties
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals new
                    { pcmp.PartyId, pcmp.ContactMechId }
                where prty.PartyId == request.PartyDto.PartyId
                      && pcmp.ContactMechPurposeTypeId == "PRIMARY_EMAIL"
                select cm;

            var primaryEmail = currentContactMechQuery.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(request.PartyDto.InfoString))
            {
                if (primaryEmail != null)
                {
                    primaryEmail.InfoString = request.PartyDto.InfoString;
                }
                else
                {
                    var contactMech = new ContactMech
                    {
                        ContactMechId = Guid.NewGuid().ToString(),
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        InfoString = request.PartyDto.InfoString,
                        ContactMechTypeId = "EMAIL_ADDRESS"
                    };
                    _context.ContactMeches.Add(contactMech);

                    var partyContactMech = new PartyContactMech
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        Party = party,
                        RoleTypeId = "EMPLOYEE"
                    };
                    _context.PartyContactMeches.Add(partyContactMech);

                    var partyContactMechPurpose = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "PRIMARY_EMAIL",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurpose);
                }
            }
            //--------------

            var employment = await _context.Employments
                .FirstOrDefaultAsync(e =>
                        e.PartyIdFrom == "Company" &&
                        e.PartyIdTo == party.PartyId &&
                        e.RoleTypeIdTo == "EMPLOYEE",
                    cancellationToken);

            if (employment == null)
            {
                employment = new Employment
                {
                    PartyIdFrom = "Company",
                    PartyIdTo = party.PartyId,
                    FromDate = stamp,
                    RoleTypeIdFrom = "INTERNAL_ORGANIZATIO",
                    RoleTypeIdTo = "EMPLOYEE",
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.Employments.Add(employment);
            }
            else
            {
                employment.LastUpdatedStamp = stamp;
            }

            // B. Position Fulfillment + EmplPosition
            EmplPosition? currentPosition = null;
            if (!string.IsNullOrWhiteSpace(request.PartyDto.EmplPositionTypeId))
            {
                // Check if position type exists
                var positionTypeExists = await _context.EmplPositionTypes
                    .AnyAsync(t => t.EmplPositionTypeId == request.PartyDto.EmplPositionTypeId, cancellationToken);

                if (!positionTypeExists)
                    return Result<PartyDto2>.Failure($"Position type {request.PartyDto.EmplPositionTypeId} not found.");

                // Find current active fulfillment
                var fulfillment = await _context.EmplPositionFulfillments
                    .Include(f => f.EmplPosition)
                    .FirstOrDefaultAsync(f =>
                            f.PartyId == party.PartyId &&
                            f.ThruDate == null,
                        cancellationToken);

                if (fulfillment != null)
                {
                    // Update existing position type if changed
                    if (fulfillment.EmplPosition?.EmplPositionTypeId != request.PartyDto.EmplPositionTypeId)
                    {
                        fulfillment.EmplPosition.EmplPositionTypeId = request.PartyDto.EmplPositionTypeId;
                        fulfillment.EmplPosition.LastUpdatedStamp = stamp;
                    }

                    fulfillment.LastUpdatedStamp = stamp;
                    currentPosition = fulfillment.EmplPosition;
                }
                else
                {
                    // Create new position and fulfillment
                    var positionId = await _utilityService.GetNextSequence("EmplPosition");

                    currentPosition = new EmplPosition
                    {
                        EmplPositionId = positionId,
                        StatusId = "EMPL_POS_ACTIVE",
                        PartyId = "Company",
                        EmplPositionTypeId = request.PartyDto.EmplPositionTypeId,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };
                    _context.EmplPositions.Add(currentPosition);

                    var newFulfillment = new EmplPositionFulfillment
                    {
                        EmplPositionId = positionId,
                        PartyId = party.PartyId,
                        FromDate = stamp,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };
                    _context.EmplPositionFulfillments.Add(newFulfillment);
                }
            }

            // C. Monthly Base Salary (RateAmount)
            if (request.PartyDto.MonthlyBaseSalary.HasValue && request.PartyDto.MonthlyBaseSalary > 0)
            {
                var latestRate = await _context.RateAmounts
                    .Where(r =>
                        r.PartyId == party.PartyId &&
                        r.PeriodTypeId == "RATE_MONTH" &&
                        r.RateTypeId == "AVERAGE_PAY_RATE")
                    .OrderByDescending(r => r.FromDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (latestRate != null)
                {
                    // Update if amount changed
                    if (latestRate.Amount != request.PartyDto.MonthlyBaseSalary.Value)
                    {
                        latestRate.Amount = request.PartyDto.MonthlyBaseSalary.Value;
                        latestRate.LastUpdatedStamp = stamp;
                    }
                }
                else
                {
                    // Create new
                    var newRate = new RateAmount
                    {
                        RateTypeId = "AVERAGE_PAY_RATE",
                        RateCurrencyUomId = "EGP",
                        PeriodTypeId = "RATE_MONTH",
                        WorkEffortId = "_NA_",
                        PartyId = party.PartyId,
                        EmplPositionTypeId = "_NA_",
                        FromDate = stamp,
                        Amount = request.PartyDto.MonthlyBaseSalary.Value,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };
                    _context.RateAmounts.Add(newRate);
                }
            }

            // D. Reporting Structure
            if (currentPosition != null)
            {
                var existingReporting = await _context.EmplPositionReportingStructs
                    .FirstOrDefaultAsync(r => r.EmplPositionIdManagedBy == currentPosition.EmplPositionId, cancellationToken);

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
                        if (existingReporting != null)
                        {
                            _context.EmplPositionReportingStructs.Remove(existingReporting);
                        }

                        if (managerPosition != null)  // only if we actually have a new manager
                        {
                            var newStruct = new EmplPositionReportingStruct
                            {
                                EmplPositionIdManagedBy  = currentPosition.EmplPositionId,
                                EmplPositionIdReportingTo = managerPosition.EmplPositionId,
                                FromDate                 = stamp,
                                CreatedStamp             = stamp,
                                LastUpdatedStamp         = stamp
                            };
                            _context.EmplPositionReportingStructs.Add(newStruct);
                        }
                        else
                        {
                            var reportingStruct = new EmplPositionReportingStruct
                            {
                                EmplPositionIdManagedBy = currentPosition.EmplPositionId,
                                EmplPositionIdReportingTo = managerPosition.EmplPositionId,
                                FromDate = stamp,
                                CreatedStamp = stamp,
                                LastUpdatedStamp = stamp
                            };
                            _context.EmplPositionReportingStructs.Add(reportingStruct);
                        }
                    }
                }
                else if (existingReporting != null)
                {
                    _context.EmplPositionReportingStructs.Remove(existingReporting);
                }
            }

            //---------------

            var createdAccounts = new List<(string Id, string Type, string Name, string Arabic)>();
            bool apCreated = false;
            var loanId = string.Empty;
            var accruedId = string.Empty;

            async Task<string?> GenerateUniqueGlId(string prefix, int digits = 4, int maxAttempts = 300)
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

            if (!await _context.PartyGlAccounts.AnyAsync(p =>
                    p.OrganizationPartyId == "Company" &&
                    p.PartyId == request.PartyDto.PartyId &&
                    p.RoleTypeId == "EMPLOYEE" &&
                    p.GlAccountTypeId == "ACCOUNTS_RECEIVABLE", cancellationToken))
            {
                loanId = await GenerateUniqueGlId("1241", 2);
                if (loanId == null) throw new Exception("Cannot generate loan GL ID");

                var loanAccount = new GlAccount
                {
                    GlAccountId = loanId,
                    GlAccountTypeId = "ACCOUNTS_RECEIVABLE",
                    GlAccountClassId = "CURRENT_ASSET",
                    GlResourceTypeId = "MONEY",
                    ParentGlAccountId = "124100",
                    AccountCode = loanId,
                    AccountName = $"Loans Receivable - {request.PartyDto.FirstName} ({party.PartyId})",
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
                    PartyId = party.PartyId,
                    RoleTypeId = "EMPLOYEE",
                    GlAccountTypeId = "ACCOUNTS_RECEIVABLE",
                    GlAccountId = loanId,
                    CreatedStamp = stamp,
                    CreatedTxStamp = stamp,
                    LastUpdatedStamp = stamp,
                    LastUpdatedTxStamp = stamp
                };
                _context.PartyGlAccounts.Add(loanPartyGl);

                apCreated = true;
                createdAccounts.Add((loanId, "Loans Receivable",
                    $"Loans Receivable - {request.PartyDto.FirstName} ({request.PartyDto.PartyId})",
                    $"ذمم الموظفين - ..."));
            }

            // 2. Accrued Expenses
            if (!await _context.PartyGlAccounts.AnyAsync(p =>
                    p.OrganizationPartyId == "Company" &&
                    p.PartyId == request.PartyDto.PartyId &&
                    p.RoleTypeId == "EMPLOYEE" &&
                    p.GlAccountTypeId == "ACCOUNTS_PAYABLE", cancellationToken))
            {
                accruedId = await GenerateUniqueGlId("22");
                if (accruedId == null) throw new Exception("Cannot generate accrued GL ID");

                var accruedAccount = new GlAccount
                {
                    GlAccountId = accruedId,
                    GlAccountTypeId = "ACCOUNTS_PAYABLE",
                    GlAccountClassId = "CURRENT_LIABILITY",
                    GlResourceTypeId = "MONEY",
                    ParentGlAccountId = "220000",
                    AccountCode = accruedId,
                    AccountName = $"Accrued Salaries - {request.PartyDto.FirstName} ({party.PartyId})",
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
                    PartyId = party.PartyId,
                    RoleTypeId = "EMPLOYEE",
                    GlAccountTypeId = "ACCOUNTS_PAYABLE",
                    GlAccountId = accruedId,
                    CreatedStamp = stamp,
                    CreatedTxStamp = stamp,
                    LastUpdatedStamp = stamp,
                    LastUpdatedTxStamp = stamp
                };
                _context.PartyGlAccounts.Add(accruedPartyGl);

                apCreated = true;

                createdAccounts.Add((accruedId, "Accrued Expenses",
                    $"Accrued Salaries - {request.PartyDto.FirstName} ({request.PartyDto.PartyId})",
                    $"مستحقات رواتب - ..."));
            }

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<PartyDto2>.Failure("Failed to update Employee");
            }

            await transaction.CommitAsync(cancellationToken);

            // Re-query to build return DTO (mirroring your UpdateCustomer pattern)
            var query1 = from prty in _context.Parties
                join prs in _context.Persons on prty.PartyId equals prs.PartyId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals new
                    { pcmp.PartyId, pcmp.ContactMechId }
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                select new PartyDto2
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description + " ( EMPLOYEE )",
                    FirstName = prs.FirstName,
                    MobileContactNumber = tn.ContactNumber
                };

            var query2 = from prty in _context.Parties
                join prs in _context.Persons on prty.PartyId equals prs.PartyId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pa in _context.PostalAddresses on cm.ContactMechId equals pa.ContactMechId
                join geo in _context.Geos on pa.CountryGeoId equals geo.GeoId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals new
                    { pcmp.PartyId, pcmp.ContactMechId }
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "GENERAL_LOCATION"
                select new PartyDto2
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description + " ( EMPLOYEE )",
                    FirstName = prs.FirstName,
                    Address1 = pa.Address1,
                    Address2 = pa.Address2,
                    GeoId = geo.GeoId,
                    GeoName = geo.GeoName
                };

            var query3 = from prty in _context.Parties
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals new
                    { pcmp.PartyId, pcmp.ContactMechId }
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_EMAIL"
                select new PartyDto2
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description + " ( EMPLOYEE )",
                    InfoString = cm.InfoString
                };

            var results1 = query1.ToList();
            var results2 = query2.ToList();
            var results3 = query3.ToList();

            var partyToReturn = new PartyDto2();

            if (results1.Count > 0)
            {
                partyToReturn.PartyId = results1[0].PartyId;
                partyToReturn.Description = results1[0].Description;
                partyToReturn.FirstName = results1[0].FirstName;
                partyToReturn.MobileContactNumber = results1[0].MobileContactNumber;
            }

            if (results2.Count > 0)
            {
                partyToReturn.Address1 = results2[0].Address1;
                partyToReturn.Address2 = results2[0].Address2;
                partyToReturn.GeoId = results2[0].GeoId;
                partyToReturn.GeoName = results2[0].GeoName;
            }

            if (results3.Count > 0)
            {
                partyToReturn.InfoString = results3[0].InfoString;
            }

            if (apCreated)
            {
                partyToReturn.CreatedLoanGlAccountId = loanId;
                partyToReturn.CreatedAccruedGlAccountId = accruedId;
                partyToReturn.CreatedLoanGlAccountName = $"AP - {request.PartyDto.GroupName} ({party.PartyId})";
                partyToReturn.CreatedLoanGlAccountArabicName = $"ذمم الموظفين - {request.PartyDto.FirstName}";
                partyToReturn.CreatedAccruedGlAccountName = $"AP - {request.PartyDto.GroupName} ({party.PartyId})";
                partyToReturn.CreatedAccruedGlAccountArabicName = $"مستحقات رواتب - {request.PartyDto.FirstName}";
            }

            return Result<PartyDto2>.Success(partyToReturn);
        }
    }
}