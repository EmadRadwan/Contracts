using Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services;

public class UpdateServiceRate
{
    public class Command : IRequest<Result<ServiceRateDto>>
    {
        public ServiceRateDto ServiceRateDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ServiceRateDto).SetValidator(new ServiceRateValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<ServiceRateDto>>
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

        public async Task<Result<ServiceRateDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());


            //check if ServiceRate with the same properties exists
            var existingServiceRate = await _context.ServiceRates
                .SingleOrDefaultAsync(x => x.MakeId == request.ServiceRateDto.MakeId
                                           && x.ModelId == request.ServiceRateDto.ModelId
                                           && x.ProductStoreId == request.ServiceRateDto.ProductStoreId
                                           && x.FromDate == request.ServiceRateDto.FromDate
                                           && x.Rate == request.ServiceRateDto.Rate
                    , cancellationToken);

            if (existingServiceRate != null)
                return Result<ServiceRateDto>.Failure("Service Rate with Same Properties Already Exists");


            var serviceRate = _vehicleService.UpdateServiceRate(request.ServiceRateDto);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Result<ServiceRateDto>.Failure("Failed to update ServiceRate");


            return Result<ServiceRateDto>.Success(request.ServiceRateDto);
        }
    }
}