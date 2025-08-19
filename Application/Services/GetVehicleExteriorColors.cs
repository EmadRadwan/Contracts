using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class GetVehicleExteriorColors
{
    public class Query : IRequest<Result<List<VehicleExteriorColorDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<VehicleExteriorColorDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<VehicleExteriorColorDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var vehicleTypes = await _context.ProductCategories
                .Where(x => x.PrimaryParentCategoryId == "VEHICLE_EXTERIOR_COLOR")
                .Select(x => new VehicleExteriorColorDto
                {
                    ExteriorColorId = x.ProductCategoryId,
                    ExteriorColorDescription = x.Description
                })
                .ToListAsync();

            return Result<List<VehicleExteriorColorDto>>.Success(vehicleTypes);
        }
    }
}