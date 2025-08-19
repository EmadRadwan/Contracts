using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.FixedAssets;

public class List
{
    public class Query : IRequest<Result<List<FixedAssetTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<FixedAssetTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<FixedAssetTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.FixedAssetTypes
                .Select(x => new FixedAssetTypeDto
                {
                    FixedAssetTypeId = x.FixedAssetTypeId,
                    Description = x.Description
                })
                .AsQueryable();


            var fixedAssetTypes = await query
                .ToListAsync(cancellationToken: cancellationToken);

            return Result<List<FixedAssetTypeDto>>.Success(fixedAssetTypes);
        }
    }
}