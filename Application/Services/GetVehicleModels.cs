using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class GetVehicleModels
{
    public class Query : IRequest<Result<List<VehicleModelDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<VehicleModelDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<VehicleModelDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var vehicleMakes = await _context.ProductCategories
                .Where(x => x.PrimaryParentCategoryId == "VEHICLE_MAKE")
                .Select(x => new VehicleMakeDto
                {
                    MakeId = x.ProductCategoryId,
                    MakeDescription = x.Description
                })
                .ToListAsync(cancellationToken);

            // get all vehicle models for each vehicle make
            var vehicleModels = new List<VehicleModelDto>();
            foreach (var vehicleMake in vehicleMakes)
            {
                var models = await _context.ProductCategories
                    .Where(x => x.PrimaryParentCategoryId == vehicleMake.MakeId)
                    .Select(x => new VehicleModelDto
                    {
                        MakeId = vehicleMake.MakeId,
                        ModelId = x.ProductCategoryId,
                        ModelDescription = x.Description
                    })
                    .ToListAsync(cancellationToken);
                vehicleModels.AddRange(models);
            }


            return Result<List<VehicleModelDto>>.Success(vehicleModels);
        }
    }
}