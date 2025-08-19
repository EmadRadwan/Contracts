using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;


public class ListOutgoingPayments
{
    public class Query : IRequest<Result<PaymentsEnvelope>>
    {
        public PaymentsLovParams? Params { get; set; }
    }

    public class PaymentsEnvelope
    {
        public List<PaymentDto>? Payments { get; set; }
        public int PaymentCount { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PaymentsEnvelope>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<PaymentsEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            var allowedTypes = new List<string>
            {
                "VENDOR_PAYMENT", "CUSTOMER_REFUND", "COMMISSION_PAYMENT", "INCOME_TAX_PAYMENT", "PAY_CHECK",
                "PAYROL_PAYMENT", "PAYROLL_TAX_PAYMENT", "SALES_TAX_PAYMENT", "TAX_PAYMENT", "COMMISSION_PAYMENT"
            };

            var searchTerm = request.Params?.SearchTerm;
            var skip = request.Params?.Skip ?? 0;
            var pageSize = request.Params?.PageSize ?? 20;

            var query = (from pmt in _context.Payments
                         where
                            allowedTypes.Contains(pmt.PaymentTypeId) &&
                            (string.IsNullOrEmpty(searchTerm) || (pmt.PaymentId != null && pmt.PaymentId.Contains(searchTerm)))
                         select new PaymentDto
                         {
                             PaymentId = pmt.PaymentId,
                             PaymentTypeId = pmt.PaymentTypeId,
                             PaymentTypeDescription = pmt.PaymentType.Description,
                             PaymentMethodTypeId = pmt.PaymentMethodTypeId,
                             PaymentDate = pmt.EffectiveDate,
                             StatusDescription = pmt.Status.Description,
                             Amount = pmt.Amount,
                            LovText = $"{pmt.PaymentId} - {pmt.PaymentType.Description} - {pmt.Amount}",
                         }).AsQueryable();

            var payments = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var paymentEnvelope = new PaymentsEnvelope
            {
                Payments = payments,
                PaymentCount = query.Count()
            };

            return Result<PaymentsEnvelope>.Success(paymentEnvelope);
        }
    }
}