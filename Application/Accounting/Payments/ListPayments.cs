using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Order.Orders; // for OrderPartyDto

namespace Application.Accounting.Payments;

public class ListPayments
{
    public class Query : IRequest<IQueryable<PaymentRecord>>
    {
        public ODataQueryOptions<PaymentRecord> Options { get; set; } = null!;
        public string Language { get; set; } = "en";
        public string? PaymentType { get; set; } // "incoming" or "outgoing"
    }

    public class Handler : IRequestHandler<Query, IQueryable<PaymentRecord>>
    {
        private readonly DataContext _context;
        private static readonly DateTime Today = DateTime.Today;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<PaymentRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language?.ToLower() ?? "en";
            var isArabic = language == "ar";

            // REFACTOR: Single query with ALL joins (including NEW SalesRequest → Product)
            //          Filter applied server-side BEFORE projection
            var query = (from pyt in _context.Payments

                // Required inner joins
                join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                join pty in _context.Parties on pyt.PartyIdFrom equals pty.PartyId

                // LEFT JOINS (existing + NEW)
                join pmt in _context.PaymentMethodTypes on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into
                    pmtJoin
                from pmt in pmtJoin.DefaultIfEmpty()
                join ptyto in _context.Parties on pyt.PartyIdTo equals ptyto.PartyId into ptytoJoin
                from ptyto in ptytoJoin.DefaultIfEmpty()
                join opp in _context.OrderPaymentPreferences on pyt.PaymentPreferenceId equals opp
                    .OrderPaymentPreferenceId into oppJoin
                from opp in oppJoin.DefaultIfEmpty()
                join ord in _context.OrderHeaders on opp.OrderId equals ord.OrderId into ordJoin
                from ord in ordJoin.DefaultIfEmpty()
                join we in _context.WorkEfforts on ord.OrderId equals we.RelatedOrderId into weJoin
                from we in weJoin.DefaultIfEmpty()
                join cc in _context.CostCenters on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                from cc in ccJoin.DefaultIfEmpty()
                join proj in _context.WorkEfforts on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                from proj in projJoin.DefaultIfEmpty()

                // REFACTOR: NEW LEFT JOINS for ProductId & BuildingNumber (per your sample data)
                join sr in _context.SalesRequests on pyt.SalesRequestId equals sr.SalesRequestId into srJoin
                from sr in srJoin.DefaultIfEmpty()
                join prod in _context.Products on sr.ProductId equals prod.ProductId into prodJoin
                from prod in prodJoin.DefaultIfEmpty()
                select new PaymentRecord
                {
                    PaymentId = pyt.PaymentId,
                    PaymentTypeId = pyt.PaymentTypeId,
                    PaymentTypeDescription = isArabic ? ptt.DescriptionArabic : ptt.Description,

                    PaymentMethodId = pyt.PaymentMethodId,
                    PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                    PaymentMethodTypeDescription = pmt != null
                        ? (isArabic ? pmt.DescriptionArabic : pmt.Description)
                        : null,

                    PartyIdFrom = pyt.PartyIdFrom,
                    PartyIdFromName = pty.Description ?? string.Empty,

                    PartyIdTo = pyt.PartyIdTo,
                    PartyIdToName = ptyto != null
                        ? ptyto.Description
                        : (pyt.PartyIdTo == "Company" ? "Company" : pyt.PartyIdTo ?? "Unknown"),

                    StatusId = pyt.StatusId,
                    StatusDescription = isArabic ? sts.DescriptionArabic : sts.Description,
                    StatusDescriptionEnglish = sts.Description,

                    EffectiveDate = (DateTime)pyt.EffectiveDate,
                    Comments = pyt.Comments,
                    PaymentRefNum = pyt.PaymentRefNum,
                    PaymentPreferenceId = pyt.PaymentPreferenceId,
                    IsBankTransfer = pyt.IsBankTransfer,
                    Amount = pyt.Amount,
                    ActualCurrencyAmount = pyt.ActualCurrencyAmount ?? pyt.Amount,
                    CurrencyUomId = pyt.CurrencyUomId ?? "EGP",

                    //FinAccountTransId = pyt.FinAccountTransId,
                    OverrideGlAccountId = pyt.OverrideGlAccountId,

                    FromPartyId = new OrderPartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description ?? string.Empty
                    },

                    IsDisbursement = ptt.ParentTypeId == "DISBURSEMENT",
                    OrganizationPartyId = ptt.ParentTypeId == "DISBURSEMENT" ? pyt.PartyIdFrom : pyt.PartyIdTo,

                    OrderId = ord != null ? ord.OrderId : null,
                    CertificateNumber = we != null ? we.CertificateNumber : null,

                    ChequeNumber = pyt.ChequeNumber,
                    ChequeDate = pyt.ChequeDate,
                    ProjectId = pyt.WorkEffortId,
                    ProjectName = proj != null ? proj.ProjectName : null,
                    CostCenterId = pyt.CostCenterId,
                    SalesRequestId = pyt.SalesRequestId,
                    CostCenterDescription = cc != null ? cc.Description : null, // REFACTOR: null-safe

                    // REFACTOR: NEW FIELDS from your request (matches sample: "A1-01", "A1")
                    ProductId = prod != null ? prod.ProductId : null, // e.g. "A1-01"
                    BuildingNumber = prod != null ? prod.BuildingNumber : null, // e.g. "A1"

                    CreatedStamp = (DateTime)pyt.CreatedStamp,
                    //DaysUntilDue = EF.Functions.DateDiffDay(Today, (DateTime)pyt.EffectiveDate), // Positive = future, 0 = today, negative = overdue
                }).AsQueryable();

            // REFACTOR: Server-side filter for incoming/outgoing (before OData)
            if (!string.IsNullOrEmpty(request.PaymentType))
            {
                var isOutgoing = request.PaymentType.ToLower() == "outgoing";
                query = query.Where(p => p.IsDisbursement == isOutgoing);
            }

            // REFACTOR: Apply ONLY $filter here (safe, type-preserving)
            //          $orderby/$skip/$top handled by BaseODataController after projection
            if (request.Options?.Filter != null)
            {
                query = request.Options.Filter.ApplyTo(query, new ODataQuerySettings
                {
                    // REFACTOR: Disable stable ordering to avoid complex ORDER BY translation failures
                    EnsureStableOrdering = false
                }) as IQueryable<PaymentRecord>;
            }

            // Materialize the query to compute DueStatusArabic in memory (same logic as ListPaymentsWithDueStatus)
            var finalQuery = await query.ToListAsync(cancellationToken);

            foreach (var record in finalQuery)
            {
                record.DaysUntilDue = (record.EffectiveDate - Today).Days;
                
                var date = record.EffectiveDate;
                var quarterNum = (date.Month - 1) / 3 + 1;
                var quarterArabic = quarterNum switch
                {
                    1 => "الأول",
                    2 => "الثاني",
                    3 => "الثالث",
                    4 => "الرابع",
                    _ => string.Empty
                };
                var quarterAndYear = $"(الربع {quarterArabic} {date.Year})";

                if (record.StatusId == "PMNT_NOT_PAID")
                {
                    var type = record.IsDisbursement ? "دفعة" : "مستحق";
                    var typePaid = record.IsDisbursement ? "دفعة مستحقة" : "مستحق";

                    if (record.DaysUntilDue < 0)
                    {
                        var daysOverdue = Math.Abs(record.DaysUntilDue);
                        record.DueStatusArabic = daysOverdue <= 30
                            ? $"{type} متأخرة منذ {daysOverdue} يوم"
                            : $"{type} متأخرة جداً {quarterAndYear}";
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
                        record.DueStatusArabic = $"{typePaid} خلال 3 أشهر {quarterAndYear}";
                    else
                        record.DueStatusArabic = $"{typePaid} لاحقاً {quarterAndYear}";
                }
                else
                {
                    record.DueStatusArabic = record.StatusDescription;
                }
            }

            return finalQuery.AsQueryable();
        }
    }
}