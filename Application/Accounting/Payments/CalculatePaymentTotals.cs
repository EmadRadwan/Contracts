using API.Controllers.Accounting;
using Application.Accounting.Services;



using MediatR;
using Persistence;

namespace Application.Accounting.Payments;

public class CalculatePaymentTotals
{
    public class Query : IRequest<List<PaymentTotalsDto>>
    {
        public List<string> PaymentIds { get; set; }
    }

    public class Handler : IRequestHandler<Query, List<PaymentTotalsDto>>
    {
        private readonly IPaymentApplicationService _paymentApplicationService;
        private readonly DataContext _context;

        public Handler(IPaymentApplicationService paymentApplicationService, DataContext context)
        {
            _paymentApplicationService = paymentApplicationService;
            _context = context;
        }

        public async Task<List<PaymentTotalsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var results = new List<PaymentTotalsDto>();

            foreach (var paymentId in request.PaymentIds)
            {
                var payment = await _context.Payments.FindAsync(paymentId);
                var amountToApply = await _paymentApplicationService.GetPaymentNotApplied(payment, true);

                results.Add(new PaymentTotalsDto
                {
                    PaymentId = paymentId,
                    AmountToApply = amountToApply,
                });
            }

            return results;
        }
    }
}
