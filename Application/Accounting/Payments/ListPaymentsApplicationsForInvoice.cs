using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Accounting.Payments;


public class ListPaymentsApplicationsForInvoice
{
    public class Query : IRequest<Result<List<PaymentApplicationDto>>>
    {
        public string InvoiceId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<PaymentApplicationDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<PaymentApplicationDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = from pyt in _context.PaymentApplications
                where pyt.InvoiceId == request.InvoiceId
                select new PaymentApplicationDto
                {
                    PaymentApplicationId = pyt.PaymentApplicationId,
                    PaymentId = pyt.PaymentId,
                    InvoiceId = pyt.InvoiceId,
                    InvoiceItemSeqId = pyt.InvoiceItemSeqId,
                    BillingAccountId = pyt.BillingAccountId,
                    OverrideGlAccountId = pyt.OverrideGlAccountId,
                    ToPaymentId = pyt.ToPaymentId,
                    TaxAuthGeoId = pyt.TaxAuthGeoId,
                    AmountApplied = pyt.AmountApplied
                };


            List<PaymentApplicationDto> results = query.ToList();


            return Result<List<PaymentApplicationDto>>.Success(results);
        }
    }
}