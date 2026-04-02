using Application.Order.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;

public class ListPaymentsWithDueStatusByDateRange
{
    public class Query : IRequest<ListPaymentsWithDueStatusResponse>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Language { get; set; } = "en";
    }

    public class ListPaymentsWithDueStatusResponse
    {
        public List<PaymentRecord> Data { get; set; } = new();
        public int Total { get; set; }
    }

    public class Handler : IRequestHandler<Query, ListPaymentsWithDueStatusResponse>
    {
        private readonly DataContext _context;
        private static readonly DateTime Today = DateTime.Today;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<ListPaymentsWithDueStatusResponse> Handle(Query request, CancellationToken ct)
        {
            var language = request.Language?.ToLower() ?? "en";
            var isArabic = language == "ar";

            var query = from pyt in _context.Payments
                        join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                        join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                        join pty in _context.Parties on pyt.PartyIdFrom equals pty.PartyId
                        
                        join pmt in _context.PaymentMethodTypes on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into pmtJoin
                        from pmt in pmtJoin.DefaultIfEmpty()

                        join pm in _context.PaymentMethods on pyt.PaymentMethodId equals pm.PaymentMethodId into pmJoin
                        from pm in pmJoin.DefaultIfEmpty()

                        join ptyto in _context.Parties on pyt.PartyIdTo equals ptyto.PartyId into ptytoJoin
                        from ptyto in ptytoJoin.DefaultIfEmpty()

                        join opp in _context.OrderPaymentPreferences on pyt.PaymentPreferenceId equals opp.OrderPaymentPreferenceId into oppJoin
                        from opp in oppJoin.DefaultIfEmpty()
                        join ord in _context.OrderHeaders on opp.OrderId equals ord.OrderId into ordJoin
                        from ord in ordJoin.DefaultIfEmpty()
                        join we in _context.WorkEfforts on ord.OrderId equals we.RelatedOrderId into weJoin
                        from we in weJoin.DefaultIfEmpty()

                        join cc in _context.CostCenters on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                        from cc in ccJoin.DefaultIfEmpty()

                        join proj in _context.WorkEfforts on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                        from proj in projJoin.DefaultIfEmpty()

                        join sr in _context.SalesRequests on pyt.SalesRequestId equals sr.SalesRequestId into srJoin
                        from sr in srJoin.DefaultIfEmpty()

                        join prod in _context.Products on sr.ProductId equals prod.ProductId into prodJoin
                        from prod in prodJoin.DefaultIfEmpty()

                        where pyt.EffectiveDate >= request.FromDate && pyt.EffectiveDate <= request.ToDate
                        select new PaymentRecord
                        {
                            PaymentId = pyt.PaymentId,
                            PaymentTypeId = pyt.PaymentTypeId,
                            PaymentTypeDescription = isArabic ? ptt.DescriptionArabic : ptt.Description,

                            PaymentMethodId = pyt.PaymentMethodId,
                            BankName = pm != null ? pm.Description : null,
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

                            Amount = pyt.Amount,
                            ActualCurrencyAmount = pyt.ActualCurrencyAmount ?? pyt.Amount,
                            CurrencyUomId = pyt.CurrencyUomId ?? "EGP",

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
                            CostCenterDescription = cc != null ? cc.Description : null,
                            ProductId = prod != null ? prod.ProductId : null,
                            BuildingNumber = prod != null ? prod.BuildingNumber : null,
                            CreatedStamp = (DateTime)pyt.CreatedStamp,
                            DaysUntilDue = EF.Functions.DateDiffDay(Today, (DateTime)pyt.EffectiveDate),
                        };

            var data = await query.ToListAsync(ct);

            foreach (var record in data)
            {
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
                        if (daysOverdue <= 30)
                            record.DueStatusArabic = $"{type} متأخرة منذ {daysOverdue} يوم";
                        else
                            record.DueStatusArabic = $"{type} متأخرة جداً {quarterAndYear}";
                    }
                    else if (record.DaysUntilDue == 0) record.DueStatusArabic = $"{typePaid} اليوم";
                    else if (record.DaysUntilDue == 1) record.DueStatusArabic = $"{typePaid} غداً";
                    else if (record.DaysUntilDue <= 3) record.DueStatusArabic = $"{typePaid} بعد {record.DaysUntilDue} أيام";
                    else if (record.DaysUntilDue <= 7) record.DueStatusArabic = $"{typePaid} هذا الأسبوع";
                    else if (record.DaysUntilDue <= 30) record.DueStatusArabic = $"{typePaid} خلال الشهر";
                    else if (record.DaysUntilDue <= 90) record.DueStatusArabic = $"{typePaid} خلال 3 أشهر {quarterAndYear}";
                    else record.DueStatusArabic = $"{typePaid} لاحقاً {quarterAndYear}";
                }
                else
                {
                    record.DueStatusArabic = record.StatusDescription;
                }
            }

            return new ListPaymentsWithDueStatusResponse
            {
                Data = data,
                Total = data.Count
            };
        }
    }
}
