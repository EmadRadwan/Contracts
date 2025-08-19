using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Services;

public class GetVehiclesLov
{
    public class VehiclesEnvelope
    {
        public List<VehicleLovDto> Vehicles { get; set; }
        public int VehicleCount { get; set; }
    }

    public class Query : IRequest<Result<VehiclesEnvelope>>
    {
        public VehicleLovParams Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<VehiclesEnvelope>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<VehiclesEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = (from veh in _context.Vehicles
                join catMake in _context.ProductCategories on veh.MakeId equals catMake.ProductCategoryId
                join catModel in _context.ProductCategories on veh.ModelId equals catModel.ProductCategoryId
                join prty in _context.Parties on veh.FromPartyId equals prty.PartyId
                where request.Params.SearchTerm == null || veh.ChassisNumber.Contains(request.Params.SearchTerm)
                select new VehicleLovDto
                {
                    VehicleId = veh.VehicleId,
                    ChassisNumber = veh.ChassisNumber,
                    PlateNumber = veh.PlateNumber,
                    FromPartyId = new VehiclePartyDto
                    {
                        FromPartyId = prty.PartyId,
                        FromPartyName = prty.Description
                    },
                    FromPartyName = prty.Description,
                    MakeDescription = catMake.Description,
                    ModelDescription = catModel.Description,
                    ServiceDate = veh.ServiceDate,
                    NextServiceDate = veh.NextServiceDate
                }).AsQueryable();


            var vehicles = await query
                .Skip(request.Params.Skip)
                .Take(request.Params.PageSize)
                .ToListAsync();

            var vehiclesEnvelop = new VehiclesEnvelope
            {
                Vehicles = vehicles,
                VehicleCount = query.Count()
            };


            return Result<VehiclesEnvelope>.Success(vehiclesEnvelop);
        }
    }
}