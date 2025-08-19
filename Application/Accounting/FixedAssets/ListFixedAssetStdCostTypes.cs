using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.FixedAssets;

public class ListFixedAssetStdCostTypes
{
    public class Query : IRequest<Result<List<FixedAssetStdCostTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<FixedAssetStdCostTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<FixedAssetStdCostTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.FixedAssetStdCostTypes
                .Select(x => new FixedAssetStdCostTypeDto
                {
                    FixedAssetStdCostTypeId = x.FixedAssetStdCostTypeId,
                    ParentTypeId = x.ParentTypeId,
                    HasTable = x.HasTable,
                    Description = x.Description
                })
                .AsQueryable();


            var fixedAssetStdCostTypes = await query
                .ToListAsync(cancellationToken: cancellationToken);

            return Result<List<FixedAssetStdCostTypeDto>>.Success(fixedAssetStdCostTypes);
        }
    }
}