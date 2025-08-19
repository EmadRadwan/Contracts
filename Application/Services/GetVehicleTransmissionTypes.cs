using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class GetVehicleTransmissionTypes
{
    public class Query : IRequest<Result<List<VehicleTransmissionTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<VehicleTransmissionTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<VehicleTransmissionTypeDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var vehicleTransmissionTypes = await _context.ProductCategories
                .Where(x => x.PrimaryParentCategoryId == "VEHICLE_TRANSMISSION_TYPE")
                .Select(x => new VehicleTransmissionTypeDto
                {
                    TransmissionTypeId = x.ProductCategoryId,
                    TransmissionTypeDescription = x.Description
                })
                .ToListAsync();

            return Result<List<VehicleTransmissionTypeDto>>.Success(vehicleTransmissionTypes);
        }
    }
}