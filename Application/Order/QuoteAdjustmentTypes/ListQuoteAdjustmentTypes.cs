using Application.Order.OrderAdjustmentTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.QuoteAdjustmentTypes;

public class ListQuoteAdjustmentTypes
{
    public class Query : IRequest<Result<List<QuoteAdjustmentTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<QuoteAdjustmentTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<QuoteAdjustmentTypeDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var orderAdjustmentTypes = await _context.OrderAdjustmentTypes
                .Select(x => new QuoteAdjustmentTypeDto
                {
                    QuoteAdjustmentTypeId = x.OrderAdjustmentTypeId,
                    Description = x.Description
                })
                .ToListAsync();

            return Result<List<QuoteAdjustmentTypeDto>>.Success(orderAdjustmentTypes);
        }
    }
}