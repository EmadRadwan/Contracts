using Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class CreateVehicle
{
    public class Command : IRequest<Result<VehicleDto>>
    {
        public VehicleDto VehicleDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.VehicleDto).SetValidator(new VehicleValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<VehicleDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly IVehicleService _vehicleService;

        public Handler(DataContext context, IUserAccessor userAccessor, IVehicleService vehicleService)
        {
            _userAccessor = userAccessor;
            _context = context;
            _vehicleService = vehicleService;
        }

        public async Task<Result<VehicleDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());


            //check if vehicle with the same ChassisNo exists 
            var existingVehicle = await _context.Vehicles
                .SingleOrDefaultAsync(x => x.ChassisNumber == request.VehicleDto.ChassisNumber, cancellationToken);

            if (existingVehicle != null) return Result<VehicleDto>.Failure("Chassis Number Already Exists");

            var newVehicle = await _vehicleService.CreateVehicle(request.VehicleDto);

            // update the current vehicleDto with the new vehicleId
            request.VehicleDto.VehicleId = newVehicle.VehicleId;


            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Result<VehicleDto>.Failure("Failed to create Vehicle");


            return Result<VehicleDto>.Success(request.VehicleDto);
        }
    }
}