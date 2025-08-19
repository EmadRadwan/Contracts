using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;


public class ListIncomingPayments
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
            var allowedTypes = new List<string> { "CUSTOMER_DEPOSIT", "CUSTOMER_PAYMENT" };
            var searchTerm = request.Params?.SearchTerm;
    var skip = request.Params?.Skip ?? 0;
    var pageSize = request.Params?.PageSize ?? 20; // Default page size if not set

    var query = (from pmt in _context.Payments
    where 
        (string.IsNullOrEmpty(searchTerm) || (pmt.PaymentId != null && pmt.PaymentId.Contains(searchTerm)))
        && (pmt.PaymentTypeId == "CUSTOMER_DEPOSIT" || pmt.PaymentTypeId == "CUSTOMER_PAYMENT")
    select new PaymentDto
    {
        PaymentId = pmt.PaymentId,
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