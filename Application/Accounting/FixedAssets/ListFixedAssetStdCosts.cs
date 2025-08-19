using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.FixedAssets;

public class ListFixedAssetStdCosts
{
    public class Query : IRequest<Result<List<FixedAssetStdCostDto>>>
    {
        public string FixedAssetId { get; set; }
    }


    public class Handler : IRequestHandler<Query, Result<List<FixedAssetStdCostDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<FixedAssetStdCostDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.FixedAssetStdCosts
                .Where(c => c.FixedAssetId == request.FixedAssetId )
                .Select(x => new FixedAssetStdCostDto
                {
                    FixedAssetId = x.FixedAssetId,
                    FixedAssetStdCostTypeId = x.FixedAssetStdCostTypeId,
                    FromDate = x.FromDate,
                    ThruDate = x.ThruDate,
                    AmountUomId = x.AmountUomId,
                    Amount = x.Amount
                })
                .OrderBy(c => c.FixedAssetId);

            var queryString = query.ToQueryString();
            var fixedAssetStdCostTypes = await query
                .ToListAsync(cancellationToken: cancellationToken);

            return Result<List<FixedAssetStdCostDto>>.Success(fixedAssetStdCostTypes);
        }
    }
}