using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class GetVehicleTypes
{
    public class Query : IRequest<Result<List<VehicleTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<VehicleTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<VehicleTypeDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var vehicleTypes = await _context.ProductCategories
                .Where(x => x.PrimaryParentCategoryId == "VEHICLE_TYPE")
                .Select(x => new VehicleTypeDto
                {
                    VehicleTypeId = x.ProductCategoryId,
                    VehicleTypeDescription = x.Description
                })
                .ToListAsync();

            return Result<List<VehicleTypeDto>>.Success(vehicleTypes);
        }
    }
}