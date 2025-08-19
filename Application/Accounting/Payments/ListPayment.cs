using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Accounting.Payments;


public class ListPayment
{
    public class Query : IRequest<Result<PaymentDto2>>
    {
        public string PaymentId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PaymentDto2>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<PaymentDto2>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from pyt in _context.Payments
                join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                join pty in _context.Parties on pyt.PartyIdFrom equals pty.PartyId
                join ptyto in _context.Parties on pyt.PartyIdTo equals ptyto.PartyId
                where pyt.PaymentId == request.PaymentId
                select new PaymentDto2
                {
                    PaymentId = pyt.PaymentId,
                    PaymentTypeId = pyt.PaymentTypeId,
                    PaymentTypeDescription = ptt.Description,
                    PartyIdFrom = new PaymentPartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description
                    },
                    PartyId = new PaymentPartyDto
                    {
                        FromPartyId = ptyto.PartyId,
                        FromPartyName = ptyto.Description
                    },
                    PaymentDate = pyt.EffectiveDate,
                    StatusDescription = sts.Description,
                    AllowSubmit = false
                };


            var results = query.ToList();

            var paymentApplications = (from pyta in _context.PaymentApplications
                where pyta.PaymentId == request.PaymentId
                select new PaymentApplicationParam
                {
                    PaymentApplicationId = pyta.PaymentApplicationId,
                    PaymentId = pyta.PaymentId,
                    InvoiceId = pyta.InvoiceId,
                    InvoiceItemSeqId = pyta.InvoiceItemSeqId,
                    BillingAccountId = pyta.BillingAccountId,
                    OverrideGlAccountId = pyta.OverrideGlAccountId,
                    ToPaymentId = pyta.ToPaymentId,
                    TaxAuthGeoId = pyta.TaxAuthGeoId,
                    AmountApplied = pyta.AmountApplied
                }).ToList();

            var paymentToReturn = new PaymentDto2();

            if (results.Any())
            {
                paymentToReturn = results[0];
                if (paymentApplications.Any()) paymentToReturn.PaymentApplications = paymentApplications;
            }

            return Result<PaymentDto2>.Success(paymentToReturn);
        }
    }
}