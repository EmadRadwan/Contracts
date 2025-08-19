using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.Products;

public class GetInventoryItemColors
{
    public class Query : IRequest<Result<List<InventoryItemColorDto>>>
    {
        public string ProductId { get; set; }
    }

    public class InventoryItemColorDto
    {
        public string ColorId { get; set; }
        public string ColorName { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<InventoryItemColorDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<InventoryItemColorDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Optimized query to fetch unique colors
            // Purpose: Retrieves distinct colors for a product by joining InventoryItem, InventoryItemFeature, and ProductFeature
            // Context: Ensures only COLOR-type features are included and avoids duplicate colors
            var colors = await (from inv in _context.InventoryItems
                               join invFeature in _context.InventoryItemFeatures
                               on inv.InventoryItemId equals invFeature.InventoryItemId
                               join feature in _context.ProductFeatures
                               on invFeature.ProductFeatureId equals feature.ProductFeatureId
                               where inv.ProductId == request.ProductId
                               && feature.ProductFeatureTypeId == "COLOR"
                               select new InventoryItemColorDto
                               {
                                   ColorId = feature.ProductFeatureId,
                                   ColorName = feature.Description
                               })
                               .Distinct()
                               .ToListAsync(cancellationToken);

            return Result<List<InventoryItemColorDto>>.Success(colors);
        }
    }
}