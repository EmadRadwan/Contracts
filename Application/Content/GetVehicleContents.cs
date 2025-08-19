using MediatR;
using Persistence;

namespace Application.Content;

public class GetVehicleContents
{
    public class Query : IRequest<Result<List<VehicleContentDto>>>
    {
        public string VehicleId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<VehicleContentDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<VehicleContentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var vehicleContents = (from cont in _context.Contents
                join ds in _context.DataResources on cont.DataResourceId equals ds.DataResourceId
                join vc in _context.VehicleContents on cont.ContentId equals vc.ContentId
                where vc.VehicleId == request.VehicleId
                select new VehicleContentDto
                {
                    VehicleId = vc.VehicleId,
                    ContentId = vc.ContentId,
                    DataResourceId = cont.DataResourceId,
                    MimeTypeId = ds.MimeTypeId,
                    DataResourceName = ds.DataResourceName,
                    ObjectInfo = ds.ObjectInfo
                }).ToList();

            return Result<List<VehicleContentDto>>.Success(vehicleContents);
        }
    }
}