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
                            PaymentTypeDescription = request.Language == "ar" ? ptt.DescriptionArabic : ptt.Description,
                            PaymentMethodTypeDescription = request.Language == "ar" ? pmt.DescriptionArabic : pmt.Description,
                            PartyIdFromName = ptyFrom.Description,
                            PartyIdToName = ptyTo.Description,
                            StatusDescription = request.Language == "ar" ? sts.DescriptionArabic : sts.Description,
                            EffectiveDate = (DateTime)pyt.EffectiveDate,
                            Comments = pyt.Comments,
                            PaymentRefNum = pyt.PaymentRefNum,
                            Amount = pyt.Amount,
                            CertificateNumber = we != null ? we.CertificateNumber : null,
                            ProjectName = proj != null ? proj.ProjectName : null,
                            CostCenterDescription = cc != null ? cc.Description : null,
                            ProductId = prod != null ? prod.ProductId : null,
                            BuildingNumber = prod != null ? prod.BuildingNumber : null,
                        };

            var data = await query.ToListAsync(ct);

            return new PaymentsDailyResponse
            {
                Data = data,
                Total = data.Count
            };
        }
    }
}