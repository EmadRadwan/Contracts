using Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class UpdateServiceSpecification
{
    public class Command : IRequest<Result<ServiceSpecificationDto>>
    {
        public ServiceSpecificationDto ServiceSpecificationDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ServiceSpecificationDto).SetValidator(new ServiceSpecificationValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<ServiceSpecificationDto>>
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

        public async Task<Result<ServiceSpecificationDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());


            //check if ServiceSpecification with the same properties exists
            var existingServiceSpecification = await _context.ServiceSpecifications
                .SingleOrDefaultAsync(x => x.MakeId == request.ServiceSpecificationDto.MakeId
                                           && x.ModelId == request.ServiceSpecificationDto.ModelId
                                           && x.ProductId == request.ServiceSpecificationDto.ProductId.ProductId
                                           && x.FromDate == request.ServiceSpecificationDto.FromDate
                                           && x.StandardTimeInMinutes ==
                                           request.ServiceSpecificationDto.StandardTimeInMinutes
                    , cancellationToken);

            if (existingServiceSpecification != null)
                return Result<ServiceSpecificationDto>.Failure(
                    "Service Specification with Same Properties Already Exists");


            var serviceSpecification = _vehicleService.UpdateServiceSpecification(request.ServiceSpecificationDto);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Result<ServiceSpecificationDto>.Failure("Failed to update ServiceSpecification");


            return Result<ServiceSpecificationDto>.Success(request.ServiceSpecificationDto);
        }
    }
}