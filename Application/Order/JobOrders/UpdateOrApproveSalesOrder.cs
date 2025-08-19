using Application.Interfaces;
using Application.order.Orders;
using Application.Order.Orders;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.jobOrders;

public class UpdateOrApproveSalesOrder
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
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IUserAccessor _userAccessor;


        public Handler(DataContext context, IUserAccessor userAccessor, IOrderService orderService,
            ILogger<Handler> logger)
        {
            _userAccessor = userAccessor;
            _context = context;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<Result<OrderDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            var updateMode = request.OrderDto.ModificationType == "UPDATE" ? "UPDATE" : "APPROVE";

            _logger.LogInformation("Order {Modification_Type} Started. {Transaction} for {OrderId}",
                updateMode, "update job order", request.OrderDto.OrderId);


            await _orderService.UpdateOrApproveSalesOrder(request.OrderDto, updateMode);

            _logger.LogInformation("Order {Modification_Type}  successfully. {Transaction} for {OrderId}",
                updateMode, "update job order",
                request.OrderDto.OrderId);


            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<OrderDto>.Failure($"Failed to {updateMode} Job Order");
            }

            await transaction.CommitAsync(cancellationToken);

            var orderToReturn = new OrderDto
            {
                OrderId = request.OrderDto.OrderId,
                FromPartyId = request.OrderDto.FromPartyId,
                CurrentMileage = request.OrderDto.CurrentMileage,
                CustomerRemarks = request.OrderDto.CustomerRemarks,
                InternalRemarks = request.OrderDto.InternalRemarks,
                StatusDescription = updateMode == "Update" ? "Created" : "Approved"
            };


            return Result<OrderDto>.Success(orderToReturn);
        }
    }
}