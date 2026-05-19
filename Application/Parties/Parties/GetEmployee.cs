using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class GetEmployee
{
    public class Query : IRequest<Result<PartyDto>>
    {
        public string PartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PartyDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PartyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from prty in _context.Parties
                where prty.PartyId == request.PartyId

                // Core party info
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join st in _context.StatusItems on prty.StatusId equals st.StatusId into stGroup
                from st in stGroup.DefaultIfEmpty()
                join person in _context.Persons on prty.PartyId equals person.PartyId into personGroup
                from person in personGroup.DefaultIfEmpty()

                // PRIMARY_PHONE
                join phonePurpose in _context.PartyContactMechPurposes
                        .Where(p => p.ContactMechPurposeTypeId == "PRIMARY_PHONE")
                    on prty.PartyId equals phonePurpose.PartyId into phonePurposeGroup
                from phonePurpose in phonePurposeGroup.DefaultIfEmpty()
                join pcmPhone in _context.PartyContactMeches
                    on new { phonePurpose.PartyId, phonePurpose.ContactMechId }
                    equals new { pcmPhone.PartyId, pcmPhone.ContactMechId } into pcmPhoneGroup
                from pcmPhone in pcmPhoneGroup.DefaultIfEmpty()
                join cmPhone in _context.ContactMeches on pcmPhone.ContactMechId equals cmPhone.ContactMechId into
                    cmPhoneGroup
                from cmPhone in cmPhoneGroup.DefaultIfEmpty()
                join tn in _context.TelecomNumbers on cmPhone.ContactMechId equals tn.ContactMechId into tnGroup
                from tn in tnGroup.DefaultIfEmpty()
                join cmptPhone in _context.ContactMechPurposeTypes
                    on phonePurpose.ContactMechPurposeTypeId equals cmptPhone.ContactMechPurposeTypeId into
                    cmptPhoneGroup
                from cmptPhone in cmptPhoneGroup.DefaultIfEmpty()

                // GENERAL_LOCATION (address)
                join addrPurpose in _context.PartyContactMechPurposes
                        .Where(p => p.ContactMechPurposeTypeId == "GENERAL_LOCATION")
                    on prty.PartyId equals addrPurpose.PartyId into addrPurposeGroup
                from addrPurpose in addrPurposeGroup.DefaultIfEmpty()
                join pcmAddr in _context.PartyContactMeches
                    on new { addrPurpose.PartyId, addrPurpose.ContactMechId }
                    equals new { pcmAddr.PartyId, pcmAddr.ContactMechId } into pcmAddrGroup
                from pcmAddr in pcmAddrGroup.DefaultIfEmpty()
                join cmAddr in _context.ContactMeches on pcmAddr.ContactMechId equals cmAddr.ContactMechId into
                    cmAddrGroup
                from cmAddr in cmAddrGroup.DefaultIfEmpty()
                join pa in _context.PostalAddresses on cmAddr.ContactMechId equals pa.ContactMechId into paGroup
                from pa in paGroup.DefaultIfEmpty()
                join geo in _context.Geos on pa.CountryGeoId equals geo.GeoId into geoGroup
                from geo in geoGroup.DefaultIfEmpty()

                // PRIMARY_EMAIL
                join emailPurpose in _context.PartyContactMechPurposes
                        .Where(p => p.ContactMechPurposeTypeId == "PRIMARY_EMAIL")
                    on prty.PartyId equals emailPurpose.PartyId into emailPurposeGroup
                from emailPurpose in emailPurposeGroup.DefaultIfEmpty()
                join pcmEmail in _context.PartyContactMeches
                    on new { emailPurpose.PartyId, emailPurpose.ContactMechId }
                    equals new { pcmEmail.PartyId, pcmEmail.ContactMechId } into pcmEmailGroup
                from pcmEmail in pcmEmailGroup.DefaultIfEmpty()
                join cmEmail in _context.ContactMeches on pcmEmail.ContactMechId equals cmEmail.ContactMechId into
                    cmEmailGroup
                from cmEmail in cmEmailGroup.DefaultIfEmpty()

                // ALL LINKED GL ACCOUNTS
                join pga in _context.PartyGlAccounts on prty.PartyId equals pga.PartyId into pgaGroup
                from pga in pgaGroup.DefaultIfEmpty()
                join gla in _context.GlAccounts on pga.GlAccountId equals gla.GlAccountId into glaGroup
                from gla in glaGroup.DefaultIfEmpty()
                join role in _context.RoleTypes on pga.RoleTypeId equals role.RoleTypeId into roleGroup
                from role in roleGroup.DefaultIfEmpty()

                // EMPLOYEE POSITION (current/active fulfillment)
                join epf in _context.EmplPositionFulfillments
                    on prty.PartyId equals epf.PartyId into epfGroup
                from epf in epfGroup.DefaultIfEmpty()
                join ep in _context.EmplPositions on epf.EmplPositionId equals ep.EmplPositionId into epGroup
                from ep in epGroup.DefaultIfEmpty()
                join ept in _context.EmplPositionTypes on ep.EmplPositionTypeId equals ept.EmplPositionTypeId into
                    eptGroup
                from ept in eptGroup.DefaultIfEmpty()

                // MONTHLY BASE SALARY (latest rate for employee)
                join ra in _context.RateAmounts
                        .Where(r => r.PeriodTypeId == "RATE_MONTH")
                    on prty.PartyId equals ra.PartyId into raGroup
                from ra in raGroup.DefaultIfEmpty()
                
                // REPORTING TO
                join rs in _context.EmplPositionReportingStructs on ep.EmplPositionId equals rs.EmplPositionIdManagedBy into rsGroup
                from rs in rsGroup.DefaultIfEmpty()
                join epRep in _context.EmplPositions on rs.EmplPositionIdReportingTo equals epRep.EmplPositionId into epRepGroup
                from epRep in epRepGroup.DefaultIfEmpty()
                join epfRep in _context.EmplPositionFulfillments.Where(f => f.ThruDate == null) on epRep.EmplPositionId equals epfRep.EmplPositionId into epfRepGroup
                from epfRep in epfRepGroup.DefaultIfEmpty()
                join managerParty in _context.Parties on epfRep.PartyId equals managerParty.PartyId into managerPartyGroup
                from managerParty in managerPartyGroup.DefaultIfEmpty()

                select new
                {
                    Party = prty,
                    Person = person,
                    PartyType = pt,
                    Status = st,
                    PhonePurpose = phonePurpose,
                    TelecomNumber = tn,
                    PhonePurposeType = cmptPhone,
                    AddrPurpose = addrPurpose,
                    PostalAddress = pa,
                    Geo = geo,
                    EmailInfo = cmEmail.InfoString,
                    Pga = pga,
                    Gla = gla,
                    RoleType = role,
                    EmplPositionFulfillment = epf,
                    EmplPosition = ep,
                    EmplPositionType = ept,
                    RateAmount = ra,
                    ReportingToPartyId = epfRep.PartyId,
                    ReportingToPartyName = managerParty.Description,
                    GlAccountIdAdvancedPayment = prty.GlAccountIdAdvancedPayment,
                    PreferredPayrollPaymentMethodId = prty.PreferredPayrollPaymentMethodId,
                    DepartmentPartyId = prty.DepartmentPartyId,
                    FingerPrintAttendanceId = prty.FingerPrintAttendanceId,
                    AttendanceStartsAt = prty.AttendanceStartsAt
                };

            var rawResults = await query
                .AsNoTracking()
                .OrderByDescending(r =>
                    r.RateAmount != null ? r.RateAmount.FromDate : DateTime.MinValue) // latest rate first
                .ThenByDescending(r =>
                    r.EmplPositionFulfillment != null ? r.EmplPositionFulfillment.FromDate : DateTime.MinValue)
                .ToListAsync(cancellationToken);

            if (!rawResults.Any())
            {
                return Result<PartyDto>.Failure("Employee not found");
            }

            var firstRecord = rawResults.First();

            var dto = new PartyDto
            {
                PartyId = firstRecord.Party.PartyId,
                Description = $"{firstRecord.Party.Description ?? ""} ( {firstRecord.Party.MainRole ?? ""} )",
                GroupName = firstRecord.Party.Description?.Split(" ( ").FirstOrDefault() ??
                            firstRecord.Party.Description ?? "",
                FirstName = firstRecord.Party.Description ?? "",
                PartyTypeId = firstRecord.PartyType.PartyTypeId,
                PartyTypeDescription = firstRecord.PartyType.Description,
                StatusDescription = firstRecord.Status?.Description,
                MainRole = firstRecord.Party.MainRole,

                // Contact info
                MobileContactNumber = firstRecord.TelecomNumber?.ContactNumber,
                ContactType = firstRecord.PhonePurposeType?.Description,
                Address1 = firstRecord.PostalAddress?.Address1,
                Address2 = firstRecord.PostalAddress?.Address2,
                GeoId = firstRecord.PostalAddress?.CountryGeoId,
                GeoName = firstRecord.Geo?.GeoName,
                InfoString = firstRecord.EmailInfo,

                // Employee-specific fields
                EmplPositionTypeId = firstRecord.EmplPositionType?.EmplPositionTypeId,
                EmplPositionDescription = firstRecord.EmplPositionType?.Description,
                MonthlyBaseSalary = firstRecord.RateAmount?.Amount,
                ReportingTo = firstRecord.ReportingToPartyId != null ? new FromPartyDto
                {
                    FromPartyId = firstRecord.ReportingToPartyId,
                    FromPartyName = firstRecord.ReportingToPartyName,
                    FromPartyPhone = ""
                } : null,
                ReportingToPartyId = firstRecord.ReportingToPartyId,
                GlAccountIdAdvancedPayment = firstRecord.GlAccountIdAdvancedPayment,
                PreferredPayrollPaymentMethodId = firstRecord.PreferredPayrollPaymentMethodId,
                DepartmentPartyId = firstRecord.DepartmentPartyId,
                FingerPrintAttendanceId = firstRecord.FingerPrintAttendanceId,
                AttendanceStartsAt = firstRecord.AttendanceStartsAt,
 
                // All linked GL accounts
                LinkedGlAccounts = rawResults
                    .Where(r => r.Pga != null && r.Gla != null)
                    .Select(r => new PartyGlAccountSimpleDto
                    {
                        GlAccountId = r.Pga.GlAccountId,
                        GlAccountTypeId = r.Pga.GlAccountTypeId,
                        RoleTypeId = r.Pga.RoleTypeId,
                        RoleDescription = r.RoleType?.Description ?? r.Pga.RoleTypeId,
                        AccountName = r.Gla.AccountName,
                        AccountNameArabic = r.Gla.AccountNameArabic,
                        AccountDescription = r.Gla.Description,
                        CreatedStamp = r.Gla.CreatedStamp
                    })
                    .OrderBy(a => a.RoleTypeId)
                    .ThenBy(a => a.GlAccountId)
                    .ToList()
            };

            return Result<PartyDto>.Success(dto);
        }
    }
}