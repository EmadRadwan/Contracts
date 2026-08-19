using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core; // ← For DateHelper

namespace Application.Accounting.Payments;

public class ListPaymentsByDateRange
{
    public class Query : IRequest<PaymentsDailyResponse>
    {
        public string? PaymentType { get; set; }
        public DateOnly FromDate { get; set; } // Changed to DateOnly
        public DateOnly ToDate { get; set; } // Changed to DateOnly
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
            var isOutgoing = request.PaymentType?.ToLower() == "outgoing";

            var fromDate = request.FromDate;
            var toDate = request.ToDate;


            var query = from pyt in _context.Payments
                join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                join ptyFrom in _context.Parties on pyt.PartyIdFrom equals ptyFrom.PartyId
                join ptyTo in _context.Parties on pyt.PartyIdTo equals ptyTo.PartyId into ptyToJoin
                from ptyTo in ptyToJoin.DefaultIfEmpty()
                join pmt in _context.PaymentMethodTypes
                    on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into pmtGroup
                from pmt in pmtGroup.DefaultIfEmpty()
                join proj in _context.WorkEfforts
                    on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                from proj in projJoin.DefaultIfEmpty()
                join cc in _context.CostCenters
                    on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                from cc in ccJoin.DefaultIfEmpty()
                join sr in _context.SalesRequests
                    on pyt.SalesRequestId equals sr.SalesRequestId into srJoin
                from sr in srJoin.DefaultIfEmpty()
                join prod in _context.Products
                    on sr.ProductId equals prod.ProductId into prodJoin
                from prod in prodJoin.DefaultIfEmpty()
                join opp in _context.OrderPaymentPreferences
                    on pyt.PaymentPreferenceId equals opp.OrderPaymentPreferenceId into oppJoin
                from opp in oppJoin.DefaultIfEmpty()
                join ord in _context.OrderHeaders
                    on opp.OrderId equals ord.OrderId into ordJoin
                from ord in ordJoin.DefaultIfEmpty()
                join we in _context.WorkEfforts
                    on ord.OrderId equals we.RelatedOrderId into weJoin
                from we in weJoin.DefaultIfEmpty()
                join approvedBy in _context.Parties
                    on pyt.ApprovedByPartyId equals approvedBy.PartyId into approvedByJoin
                from approvedBy in approvedByJoin.DefaultIfEmpty()
                join createdBy in _context.Parties
                    on pyt.CreatedByPartyId equals createdBy.PartyId into createdByJoin
                from createdBy in createdByJoin.DefaultIfEmpty()
                join pm in _context.PaymentMethods
                    on pyt.PaymentMethodId equals pm.PaymentMethodId into pmJoin
                from pm in pmJoin.DefaultIfEmpty()
                join gl in _context.GlAccounts
                    on pyt.OverrideGlAccountId equals gl.GlAccountId into glJoin
                from gl in glJoin.DefaultIfEmpty()
                where
                    // Filter on the business payment date (EffectiveDate), not the audit stamps —
                    // matches the OData path in ListPayments so both branches of the date-range
                    // export dialog agree. CreatedStamp belongs to ListPaymentsToday, not here.
                    (
                        pyt.EffectiveDate != null &&
                        pyt.EffectiveDate >= fromDate &&
                        pyt.EffectiveDate <= toDate
                    )
                    && (isOutgoing
                        ? ptt.ParentTypeId == "DISBURSEMENT"
                        : ptt.ParentTypeId != "DISBURSEMENT")
                select new PaymentRecordDto
                {
                    PaymentId = pyt.PaymentId,
                    PaymentTypeId = pyt.PaymentTypeId,
                    PaymentTypeDescription = request.Language == "ar"
                        ? ptt.DescriptionArabic
                        : ptt.Description,
                    PaymentMethodId = pyt.PaymentMethodId,
                    PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                    PaymentMethodTypeDescription = pmt != null
                        ? (request.Language == "ar" ? pmt.DescriptionArabic : pmt.Description)
                        : null,
                    PartyIdFrom = pyt.PartyIdFrom,
                    PartyIdFromName = ptyFrom.Description ?? string.Empty,
                    PartyIdTo = pyt.PartyIdTo,
                    PartyIdToName = ptyTo != null ? ptyTo.Description : pyt.PartyIdTo,
                    StatusId = pyt.StatusId,
                    StatusDescription = request.Language == "ar"
                        ? sts.DescriptionArabic
                        : sts.Description,
                    StatusDescriptionEnglish = sts.Description,
                    EffectiveDate = pyt.EffectiveDate,
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
                    IsBankTransfer = pyt.IsBankTransfer,
                    ChequeNumber = pyt.ChequeNumber,
                    ChequeDate = pyt.ChequeDate,
                    OrderId = ord != null ? ord.OrderId : null,
                    CertificateNumber = we != null ? we.CertificateNumber : null,
                    SalesRequestId = pyt.SalesRequestId,
                    ProjectName = proj != null ? proj.ProjectName : null,
                    CostCenterDescription = cc != null ? cc.Description : null,
                    ProductId = prod != null ? prod.ProductId : null,
                    BuildingNumber = prod != null ? prod.BuildingNumber : null,
                    ApprovedByPartyName = approvedBy != null ? approvedBy.Description : null,
                    CreatedByPartyName = createdBy != null ? createdBy.Description : null,
                    PaymentMethodDescription = pm != null ? pm.Description : null,
                    AccountNameArabic = gl != null ? gl.AccountNameArabic : null,
                };

            var data = await query.ToListAsync(ct);

            // Post-processing
            foreach (var record in data)
            {
                var effectiveDate = record.EffectiveDate ?? DateHelper.Today;

                record.DaysUntilDue = effectiveDate.DayNumber - DateHelper.Today.DayNumber;

                var quarterText = GetQuarterArabic(effectiveDate);

                if (record.StatusId == "PMNT_NOT_PAID")
                {
                    var type = record.IsDisbursement ? "دفعة" : "مستحق";
                    var typePaid = record.IsDisbursement ? "دفعة مستحقة" : "مستحق";

                    if (record.DaysUntilDue < 0)
                    {
                        var daysOverdue = Math.Abs(record.DaysUntilDue);
                        record.DueStatusArabic = daysOverdue <= 30
                            ? $"{type} متأخرة منذ {daysOverdue} يوم"
                            : $"{type} متأخرة جداً {quarterText}";
                    }
                    else if (record.DaysUntilDue == 0)
                        record.DueStatusArabic = $"{typePaid} اليوم";
                    else if (record.DaysUntilDue == 1)
                        record.DueStatusArabic = $"{typePaid} غداً";
                    else if (record.DaysUntilDue <= 3)
                        record.DueStatusArabic = $"{typePaid} بعد {record.DaysUntilDue} أيام";
                    else if (record.DaysUntilDue <= 7)
                        record.DueStatusArabic = $"{typePaid} هذا الأسبوع";
                    else if (record.DaysUntilDue <= 30)
                        record.DueStatusArabic = $"{typePaid} خلال الشهر";
                    else if (record.DaysUntilDue <= 90)
                        record.DueStatusArabic = $"{typePaid} خلال 3 أشهر {quarterText}";
                    else
                        record.DueStatusArabic = $"{typePaid} لاحقاً {quarterText}";
                }
                else
                {
                    record.DueStatusArabic = record.StatusDescription;
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