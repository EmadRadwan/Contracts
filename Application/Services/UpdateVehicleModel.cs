using Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class UpdateVehicleModel
{
    public class Command : IRequest<Result<VehicleModelDto>>
    {
        public VehicleModelDto VehicleModelDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.VehicleModelDto).SetValidator(new VehicleModelValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<VehicleModelDto>>
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

        public async Task<Result<VehicleModelDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());


            //check if VehicleModel with the same properties exists
            var existingVehicleModel = await _context.ProductCategories
                .SingleOrDefaultAsync(x => x.ProductCategoryId == request.VehicleModelDto.ModelId
                                           && x.PrimaryParentCategoryId == request.VehicleModelDto.MakeId
                    , cancellationToken);

            if (existingVehicleModel != null)
                return Result<VehicleModelDto>.Failure("Vehicle Make / Model with Same Name Already Exists");


            var vehicleModel = _vehicleService.UpdateVehicleModel(request.VehicleModelDto);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Result<VehicleModelDto>.Failure("Failed to update VehicleModel");


            return Result<VehicleModelDto>.Success(request.VehicleModelDto);
        }
    }
}