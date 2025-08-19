using Application.Interfaces;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Services;

public class UpdateVehicle
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
            await _vehicleService.UpdateVehicle(request.VehicleDto);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Result<VehicleDto>.Failure("Failed to update Vehicle");


            return Result<VehicleDto>.Success(request.VehicleDto);
        }
    }
}