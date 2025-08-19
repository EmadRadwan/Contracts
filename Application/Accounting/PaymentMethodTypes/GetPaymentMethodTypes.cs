using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.PaymentMethodTypes;

public class GetPaymentMethodTypes
{
    public class Query : IRequest<Result<List<PaymentMethodTypeDto2>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<PaymentMethodTypeDto2>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PaymentMethodTypeDto2>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var paymentMethodTypes = await (from pmt in _context.PaymentMethodTypes
            select new PaymentMethodTypeDto2
            {
                PaymentMethodTypeId = pmt.PaymentMethodTypeId,
                Description = pmt.Description
            }).ToListAsync(cancellationToken);

            return Result<List<PaymentMethodTypeDto2>>.Success(paymentMethodTypes);
        }
    }
}