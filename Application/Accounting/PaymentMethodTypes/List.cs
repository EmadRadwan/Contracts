using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.PaymentMethodTypes;

public class List
{
    public class Query : IRequest<Result<List<PaymentMethodTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<PaymentMethodTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PaymentMethodTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var allowedPaymentMethods = new[] { "CASH", "CREDIT_CARD" };
            var query = _context.PaymentMethodTypes
                .Where(z => allowedPaymentMethods.Contains(z.PaymentMethodTypeId))
                .Select(x => new PaymentMethodTypeDto
                {
                    Value = x.PaymentMethodTypeId,
                    Label = x.Description
                })
                .OrderBy(x => x.Label)
                .AsQueryable();


            var paymentMethodTypes = await query
                .ToListAsync();

            return Result<List<PaymentMethodTypeDto>>.Success(paymentMethodTypes);
        }
    }
}