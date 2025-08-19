using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Services;

public class ListVehicle
{
    public class Query : IRequest<Result<PagedList<VehicleDto>>>
    {
        public VehicleParams Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PagedList<VehicleDto>>>
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

        public async Task<Result<PagedList<VehicleDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // select * from Vehicles and include all the related entities
            // and create a queryable of VehicleDto

            var query = (from veh in _context.Vehicles
                    join pty in _context.Parties on veh.FromPartyId equals pty.PartyId
                    join catMake in _context.ProductCategories on veh.MakeId equals catMake.ProductCategoryId
                    join catModel in _context.ProductCategories on veh.ModelId equals catModel.ProductCategoryId
                    join catType in _context.ProductCategories on veh.VehicleTypeId equals catType.ProductCategoryId
                    join catTrans in _context.ProductCategories on veh.TransmissionTypeId equals catTrans
                        .ProductCategoryId
                    join catExtColor in _context.ProductCategories on veh.ExteriorColorId equals catExtColor
                        .ProductCategoryId
                    join catIntColor in _context.ProductCategories on veh.InteriorColorId equals catIntColor
                        .ProductCategoryId
                    select new VehicleDto
                    {
                        VehicleId = veh.VehicleId,
                        ChassisNumber = veh.ChassisNumber,
                        Vin = veh.Vin,
                        Year = veh.Year,
                        PlateNumber = veh.PlateNumber,
                        FromPartyName = pty.Description,
                        MakeId = veh.MakeId,
                        MakeDescription = catMake.Description,
                        ModelId = veh.ModelId,
                        ModelDescription = catModel.Description,
                        VehicleTypeId = veh.VehicleTypeId,
                        VehicleTypeDescription = catType.Description,
                        TransmissionTypeId = veh.TransmissionTypeId,
                        TransmissionTypeDescription = catTrans.Description,
                        ExteriorColorId = veh.ExteriorColorId,
                        ExteriorColorDescription = catExtColor.Description,
                        InteriorColorId = veh.InteriorColorId,
                        InteriorColorDescription = catIntColor.Description,
                        ServiceDate = veh.ServiceDate,
                        Mileage = veh.Mileage,
                        NextServiceDate = veh.NextServiceDate,
                        FromPartyId = new VehiclePartyDto
                        {
                            FromPartyId = pty.PartyId,
                            FromPartyName = pty.Description
                        }
                    }
                ).AsQueryable();

            query = query.Sort(request.Params?.OrderBy);


            return Result<PagedList<VehicleDto>>.Success(
                await PagedList<VehicleDto>.ToPagedList(query, request.Params.PageNumber,
                    request.Params.PageSize)
            );
        }
    }
}