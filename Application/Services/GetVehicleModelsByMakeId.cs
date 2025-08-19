using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class GetVehicleModelsByMakeId
{
    public class Query : IRequest<Result<List<VehicleModelDto>>>
    {
        public string MakeId { get; set; }
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
                .Where(x => x.PrimaryParentCategoryId == request.MakeId)
                .Select(x => new VehicleModelDto
                {
                    ModelId = x.ProductCategoryId,
                    ModelDescription = x.Description
                })
                .ToListAsync();

            return Result<List<VehicleModelDto>>.Success(vehicleMakes);
        }
    }
}