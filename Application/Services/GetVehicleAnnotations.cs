using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class GetVehicleAnnotations
{
    public class Query : IRequest<Result<List<VehicleAnnotationDto>>>
    {
        public string VehicleId { get; set; }
    }


    public class Handler : IRequestHandler<Query, Result<List<VehicleAnnotationDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<VehicleAnnotationDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            // get vehicleAnnotations by vehicleId from VehicleAnnotation and Annotation by joining on AnnotationId
            var vehicleMakes = await _context.VehicleAnnotations
                .Where(x => x.VehicleId == request.VehicleId)
                .Select(x => new VehicleAnnotationDto
                {
                    Vehicle = x.VehicleId,
                    AnnotationId = x.AnnotationId,
                    XCoordinate = x.Annotation.XCoordinate,
                    YCoordinate = x.Annotation.YCoordinate,
                    Note = x.Annotation.Note
                })
                .ToListAsync();


            return Result<List<VehicleAnnotationDto>>.Success(vehicleMakes);
        }
    }
}