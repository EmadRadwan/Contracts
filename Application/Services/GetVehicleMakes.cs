using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class GetVehicleMakes
{
    public class Query : IRequest<Result<List<VehicleMakeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<VehicleMakeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<VehicleMakeDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var vehicleMakes = await _context.ProductCategories
                .Where(x => x.PrimaryParentCategoryId == "VEHICLE_MAKE")
                .Select(x => new VehicleMakeDto
                {
                    MakeId = x.ProductCategoryId,
                    MakeDescription = x.Description
                })
                .ToListAsync();

            return Result<List<VehicleMakeDto>>.Success(vehicleMakes);
        }
    }
}