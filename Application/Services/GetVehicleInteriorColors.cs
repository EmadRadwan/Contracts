using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class GetVehicleInteriorColors
{
    public class Query : IRequest<Result<List<VehicleInteriorColorDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<VehicleInteriorColorDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<VehicleInteriorColorDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var vehicleTypes = await _context.ProductCategories
                .Where(x => x.PrimaryParentCategoryId == "VEHICLE_INTERIOR_COLOR")
                .Select(x => new VehicleInteriorColorDto
                {
                    InteriorColorId = x.ProductCategoryId,
                    InteriorColorDescription = x.Description
                })
                .ToListAsync();

            return Result<List<VehicleInteriorColorDto>>.Success(vehicleTypes);
        }
    }
}