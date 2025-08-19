using Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class UpdateVehicleMake
{
    public class Command : IRequest<Result<VehicleMakeDto>>
    {
        public VehicleMakeDto VehicleMakeDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.VehicleMakeDto).SetValidator(new VehicleMakeValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<VehicleMakeDto>>
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

        public async Task<Result<VehicleMakeDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());


            //check if VehicleMake with the same properties exists
            var existingVehicleMake = await _context.ProductCategories
                .SingleOrDefaultAsync(x => x.ProductCategoryId == request.VehicleMakeDto.MakeId
                    , cancellationToken);

            if (existingVehicleMake != null)
                return Result<VehicleMakeDto>.Failure("Vehicle Make with Same Name Already Exists");


            var vehicleMake = _vehicleService.UpdateVehicleMake(request.VehicleMakeDto);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Result<VehicleMakeDto>.Failure("Failed to update VehicleMake");


            return Result<VehicleMakeDto>.Success(request.VehicleMakeDto);
        }
    }
}