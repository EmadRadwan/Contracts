using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Domain;

namespace Application.Accounting.PaymentGroup;

public class GetPaymentGroupTypes
{
    public class Query : IRequest<Result<List<PaymentGroupTypeDto>>>
    {
        public string? Language { get; set; }
    }


    public class Handler : IRequestHandler<Query, Result<List<PaymentGroupTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PaymentGroupTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {   
            var language = request.Language ?? "en";
            var paymentGroupTypes = await (from pgt in _context.PaymentGroupTypes
            select new PaymentGroupTypeDto
            {
                PaymentGroupTypeId = pgt.PaymentGroupTypeId,
                Description = pgt.Description
            }).ToListAsync(cancellationToken);

            return Result<List<PaymentGroupTypeDto>>.Success(paymentGroupTypes);
        }
    }
}