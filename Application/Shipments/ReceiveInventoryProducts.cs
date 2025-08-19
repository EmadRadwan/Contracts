using Application.Interfaces;
using Application.Order.Orders;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Shipments;

public class ReceiveInventoryProducts
{
    public class Command : IRequest<Result<OrderDto>>
    {
        public OrderDto OrderDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.OrderDto).SetValidator(new OrderValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<OrderDto>>
    {
        private readonly DataContext _context;
        private readonly IShipmentService _shipmentService;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor, IShipmentService shipmentService)
        {
            _userAccessor = userAccessor;
            _context = context;
            _shipmentService = shipmentService;
        }

        public async Task<Result<OrderDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            //wait _shipmentService.ReceiveInventoryProduct(request.OrderDto);


            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<OrderDto>.Failure("Failed to receive inventory");
            }

            transaction.Commit();


            var orderToReturn = new OrderDto
            {
                OrderId = request.OrderDto.OrderId,
                FromPartyId = request.OrderDto.FromPartyId,
                StatusDescription = "Created"
            };

            return Result<OrderDto>.Success(orderToReturn);
        }
    }
}