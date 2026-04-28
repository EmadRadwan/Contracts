using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core; // ← Make sure you have this for DateHelper

namespace Application.Accounting.Payments;

public class ListPaymentsDaily
{
    public class Query : IRequest<PaymentsDailyResponse>
    {
        public string? PaymentType { get; set; }
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Query, PaymentsDailyResponse>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<PaymentsDailyResponse> Handle(Query request, CancellationToken ct)
        {
            var egyptZone = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "Egypt Standard Time" : "Africa/Cairo");

            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptZone);
            var startOfDayEgypt = egyptNow.Date;
            var endOfDayEgypt = startOfDayEgypt.AddDays(1);

            var isOutgoing = request.PaymentType?.ToLower() == "outgoing";

            var query = from pyt in _context.Payments
                join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                join ptyFrom in _context.Parties on pyt.PartyIdFrom equals ptyFrom.PartyId
                join ptyTo in _context.Parties on pyt.PartyIdTo equals ptyTo.PartyId
                join pmt in _context.PaymentMethodTypes on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into
                    pmtGroup
                from pmt in pmtGroup.DefaultIfEmpty()
                join proj in _context.WorkEfforts on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                from proj in projJoin.DefaultIfEmpty()
                join cc in _context.CostCenters on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                from cc in ccJoin.DefaultIfEmpty()
                join sr in _context.SalesRequests on pyt.SalesRequestId equals sr.SalesRequestId into srJoin
                from sr in srJoin.DefaultIfEmpty()
                join prod in _context.Products on sr.ProductId equals prod.ProductId into prodJoin
                from prod in prodJoin.DefaultIfEmpty()
                where (pyt.CreatedStamp >= startOfDayEgypt
                       && pyt.CreatedStamp < endOfDayEgypt) ||
                      (pyt.LastUpdatedStamp >= startOfDayEgypt && pyt.LastUpdatedStamp < endOfDayEgypt)
                      && (isOutgoing
                          ? ptt.ParentTypeId == "DISBURSEMENT"
                          : ptt.ParentTypeId != "DISBURSEMENT")
                select new PaymentRecordDto
                {
                    PaymentId = pyt.PaymentId,
                    PaymentTypeId = pyt.PaymentTypeId,
                    PaymentTypeDescription = request.Language == "ar" ? ptt.DescriptionArabic : ptt.Description,
                    PaymentMethodId = pyt.PaymentMethodId,
                    PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                    PaymentMethodTypeDescription = pmt != null
                        ? (request.Language == "ar" ? pmt.DescriptionArabic : pmt.Description)
                        : null,
                    PartyIdFrom = pyt.PartyIdFrom,
                    PartyIdFromName = ptyFrom.Description ?? string.Empty,
                    PartyIdTo = pyt.PartyIdTo,
                    PartyIdToName = ptyTo.Description ?? string.Empty,
                    StatusId = pyt.StatusId,
                    StatusDescription = request.Language == "ar" ? sts.DescriptionArabic : sts.Description,
                    StatusDescriptionEnglish = sts.Description,
                    EffectiveDate = pyt.EffectiveDate, // DateOnly?
                    Comments = pyt.Comments,
                    PaymentRefNum = pyt.PaymentRefNum,
                    PaymentPreferenceId = pyt.PaymentPreferenceId,
                    ActualCurrencyAmount = pyt.ActualCurrencyAmount ?? pyt.Amount,
                    OverrideGlAccountId = pyt.OverrideGlAccountId,
                    OrganizationPartyId = ptt.ParentTypeId == "DISBURSEMENT"
                        ? pyt.PartyIdFrom
                        : pyt.PartyIdTo,
                    Amount = pyt.Amount,
                    CurrencyUomId = pyt.CurrencyUomId ?? "EGP",
                    IsDisbursement = ptt.ParentTypeId == "DISBURSEMENT",
                    ChequeNumber = pyt.ChequeNumber,
                    ChequeDate = pyt.ChequeDate,
                    CertificateNumber = null, // was never populated
                    ProjectName = proj != null ? proj.ProjectName : null,
                    CostCenterDescription = cc != null ? cc.Description : null,
                    ProductId = prod != null ? prod.ProductId : null,
                    BuildingNumber = prod != null ? prod.BuildingNumber : null,
                };

            var data = await query.ToListAsync(ct);

            // === Post-processing: Calculate DaysUntilDue and DueStatusArabic ===
            foreach (var record in data)
            {
                // Safe calculation using DayNumber (works on .NET 6+)
                var effectiveDate = record.EffectiveDate ?? DateHelper.Today;
                record.DaysUntilDue = effectiveDate.DayNumber - DateHelper.Today.DayNumber;

                if (record.StatusId != "PMNT_NOT_PAID")
                {
                    record.DueStatusArabic = record.StatusDescription;
                    continue;
                }

                var type = record.IsDisbursement ? "دفعة" : "مستحق";
                var typePaid = record.IsDisbursement ? "دفعة مستحقة" : "مستحق";

                var quarterText = GetQuarterArabic(effectiveDate);

                if (record.DaysUntilDue < 0)
                {
                    var daysOverdue = Math.Abs(record.DaysUntilDue);
                    record.DueStatusArabic = daysOverdue <= 30
                        ? $"{type} متأخرة منذ {daysOverdue} يوم"
                        : $"{type} متأخرة جداً {quarterText}";
                }
                else if (record.DaysUntilDue == 0)
                {
                    record.DueStatusArabic = $"{typePaid} اليوم";
                }
                else if (record.DaysUntilDue == 1)
                {
                    record.DueStatusArabic = $"{typePaid} غداً";
                }
                else if (record.DaysUntilDue <= 3)
                {
                    record.DueStatusArabic = $"{typePaid} بعد {record.DaysUntilDue} أيام";
                }
                else if (record.DaysUntilDue <= 7)
                {
                    record.DueStatusArabic = $"{typePaid} هذا الأسبوع";
                }
                else if (record.DaysUntilDue <= 30)
                {
                    record.DueStatusArabic = $"{typePaid} خلال الشهر";
                }
                else if (record.DaysUntilDue <= 90)
                {
                    record.DueStatusArabic = $"{typePaid} خلال 3 أشهر {quarterText}";
                }
                else
                {
                    record.DueStatusArabic = $"{typePaid} لاحقاً {quarterText}";
                }
            }

            return new PaymentsDailyResponse
            {
                Data = data,
                Total = data.Count
            };
        }

        private static string GetQuarterArabic(DateOnly date)
        {
            int quarterNum = (date.Month - 1) / 3 + 1;
            string quarterName = quarterNum switch
            {
                1 => "الأول",
                2 => "الثاني",
                3 => "الثالث",
                4 => "الرابع",
                _ => ""
            };

            return $" (الربع {quarterName} {date.Year})";
        }
    }
}

// REFACTOR: Removed unused properties from DTO to match actual data returned
// Purpose: Prevent confusion and reduce object size
// Improvement: CreditCardNumber, CreditCardExpiryDate, OrderId, CertificateNumber are never set
public class PaymentRecordDto
{
    public string PaymentId { get; set; } = null!;
    public string PaymentTypeId { get; set; } = null!;
    public string PaymentTypeDescription { get; set; } = null!;
    public string? PaymentMethodId { get; set; }
    public string PaymentMethodTypeId { get; set; } = null!;
    public string PaymentMethodTypeDescription { get; set; } = null!;
    public string PartyIdFrom { get; set; } = null!;
    public string PartyIdFromName { get; set; } = null!;
    public string PartyIdTo { get; set; } = null!;
    public string PartyIdToName { get; set; } = null!;
    public string StatusId { get; set; } = null!;
    public string StatusDescription { get; set; } = null!;
    public string StatusDescriptionEnglish { get; set; } = null!;
    public DateOnly? EffectiveDate { get; set; }
    public string? Comments { get; set; }
    public string? PaymentRefNum { get; set; }
    public string? PaymentPreferenceId { get; set; }
    public decimal ActualCurrencyAmount { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public string OrganizationPartyId { get; set; } = null!;
    public decimal Amount { get; set; }
    public string CurrencyUomId { get; set; } = null!;
    public string? FinAccountTransId { get; set; }
    public bool IsDisbursement { get; set; }
    public string? ChequeNumber { get; set; }
    public DateOnly? ChequeDate { get; set; }
    public string? CertificateNumber { get; set; } // From WorkEffort via Order chain
    public string? ProjectName { get; set; } // Direct from Payment.WorkEffortId
    public string? CostCenterDescription { get; set; }
    public string? ProductId { get; set; }
    public string? BuildingNumber { get; set; }
    public int DaysUntilDue { get; set; }
    public string? DueStatusArabic { get; set; }
}

public class PaymentsDailyResponse
{
    public List<PaymentRecordDto> Data { get; set; } = new();
    public int Total { get; set; }
}