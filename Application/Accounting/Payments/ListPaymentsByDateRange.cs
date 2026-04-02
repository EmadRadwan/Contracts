// Application layer - new command similar to ListPaymentsDaily

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;

public class ListPaymentsByDateRange
{
    public class Query : IRequest<PaymentsDailyResponse>
    {
        public string? PaymentType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, PaymentsDailyResponse>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaymentsDailyResponse> Handle(Query request, CancellationToken ct)
        {
            var isOutgoing = request.PaymentType == "outgoing";

            var query = from pyt in _context.Payments
                        join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                        join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                        join ptyFrom in _context.Parties on pyt.PartyIdFrom equals ptyFrom.PartyId
                        join ptyTo in _context.Parties on pyt.PartyIdTo equals ptyTo.PartyId
                        join pmt in _context.PaymentMethodTypes 
                            on pyt.PaymentMethodTypeId equals pmt.PaymentMethodTypeId into pmtGroup
                        from pmt in pmtGroup.DefaultIfEmpty()

                        join opp in _context.OrderPaymentPreferences 
                            on pyt.PaymentPreferenceId equals opp.OrderPaymentPreferenceId into oppJoin
                        from opp in oppJoin.DefaultIfEmpty()
                        join ord in _context.OrderHeaders 
                            on opp.OrderId equals ord.OrderId into ordJoin
                        from ord in ordJoin.DefaultIfEmpty()
                        join we in _context.WorkEfforts 
                            on ord.OrderId equals we.RelatedOrderId into weJoin
                        from we in weJoin.DefaultIfEmpty()

                        join proj in _context.WorkEfforts 
                            on pyt.WorkEffortId equals proj.WorkEffortId into projJoin
                        from proj in projJoin.DefaultIfEmpty()

                        join cc in _context.CostCenters 
                            on pyt.CostCenterId equals cc.CostCenterId into ccJoin
                        from cc in ccJoin.DefaultIfEmpty()

                        join sr in _context.SalesRequests on pyt.SalesRequestId equals sr.SalesRequestId into srJoin
                        from sr in srJoin.DefaultIfEmpty()
                        join prod in _context.Products on sr.ProductId equals prod.ProductId into prodJoin
                        from prod in prodJoin.DefaultIfEmpty()

                        where pyt.CreatedStamp >= request.FromDate
                              && pyt.CreatedStamp <= request.ToDate
                              && (isOutgoing ? ptt.ParentTypeId == "DISBURSEMENT" : ptt.ParentTypeId != "DISBURSEMENT")
                        select new PaymentRecordDto
                        {
                            // same mapping as in ListPaymentsDaily
                            PaymentId = pyt.PaymentId,
                            PaymentTypeId = pyt.PaymentTypeId,
                            PaymentTypeDescription = request.Language == "ar" ? ptt.DescriptionArabic : ptt.Description,
                            PaymentMethodId = pyt.PaymentMethodId,
                            PaymentMethodTypeId = pyt.PaymentMethodTypeId,
                            PaymentMethodTypeDescription = pmt != null
                                ? (request.Language == "ar" ? pmt.DescriptionArabic : pmt.Description)
                                : null,
                            PartyIdFrom = pyt.PartyIdFrom,
                            PartyIdFromName = ptyFrom.Description,
                            PartyIdTo = pyt.PartyIdTo,
                            PartyIdToName = ptyTo.Description,
                            StatusId = pyt.StatusId,
                            StatusDescription = request.Language == "ar" ? sts.DescriptionArabic : sts.Description,
                            StatusDescriptionEnglish = sts.Description,
                            EffectiveDate = (DateTime)pyt.EffectiveDate,
                            Comments = pyt.Comments,
                            PaymentRefNum = pyt.PaymentRefNum,
                            PaymentPreferenceId = pyt.PaymentPreferenceId,
                            ActualCurrencyAmount = pyt.ActualCurrencyAmount ?? pyt.Amount,
                            OverrideGlAccountId = pyt.OverrideGlAccountId,
                            OrganizationPartyId = ptt.ParentTypeId == "DISBURSEMENT" ? pyt.PartyIdFrom : pyt.PartyIdTo,
                            Amount = pyt.Amount,
                            CurrencyUomId = pyt.CurrencyUomId ?? "EGP",
                            IsDisbursement = ptt.ParentTypeId == "DISBURSEMENT",
                            ChequeNumber = pyt.ChequeNumber,
                            ChequeDate = pyt.ChequeDate,
                            CertificateNumber = we != null ? we.CertificateNumber : null,
                            ProjectName = proj != null ? proj.ProjectName : null,
                            CostCenterDescription = cc != null ? cc.Description : null,
                            ProductId = prod != null ? prod.ProductId : null,
                            BuildingNumber = prod != null ? prod.BuildingNumber : null,
                        };

            var data = await query.ToListAsync(ct);

            var today = DateTime.Today;
            foreach (var record in data)
            {
                record.DaysUntilDue = (record.EffectiveDate - today).Days;

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

            return new PaymentsDailyResponse
            {
                Data = data,
                Total = data.Count
            };
        }
    }
}