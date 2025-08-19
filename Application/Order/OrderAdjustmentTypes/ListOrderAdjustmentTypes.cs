using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.OrderAdjustmentTypes;

public class ListOrderAdjustmentTypes
{
    public class Query : IRequest<Result<List<OrderAdjustmentTypeDto>>>
    {
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<OrderAdjustmentTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<OrderAdjustmentTypeDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var language = request.Language;
            var taxExclusions = new[] { "SALES_TAX", "VAT_PRICE_CORRECT", "VAT_TAX" };
            var orderAdjustmentTypes = await _context.OrderAdjustmentTypes
                .Where(x => !taxExclusions.Contains(x.OrderAdjustmentTypeId))
                .Select(x => new OrderAdjustmentTypeDto
                {
                    OrderAdjustmentTypeId = x.OrderAdjustmentTypeId,
                    Description = language == "ar" ? x.DescriptionArabic : language == "tr" ? x.DescriptionTurkish : x.Description
                })
                .ToListAsync();

            return Result<List<OrderAdjustmentTypeDto>>.Success(orderAdjustmentTypes);
        }
    }
}