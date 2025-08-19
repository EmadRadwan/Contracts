using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.PaymentTypes;

public class List
{
    public class Query : IRequest<Result<List<PaymentTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<PaymentTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PaymentTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.PaymentTypes
                .OrderBy(x => x.Description)
                .Select(x => new PaymentTypeDto
                {
                    PaymentTypeId = x.PaymentTypeId,
                    ParentTypeId = x.ParentTypeId,
                    Description = x.Description
                })
                .AsQueryable();


            var paymentTypes = await query
                .ToListAsync();

            return Result<List<PaymentTypeDto>>.Success(paymentTypes);
        }
    }
}