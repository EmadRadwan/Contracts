using Application.Interfaces;
using Application.order.Orders;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Order.Orders;

public class UpdateOrApprovePurchaseOrder
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
        private readonly IOrderService _orderService;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor, IOrderService orderService)
        {
            _userAccessor = userAccessor;
            _context = context;
            _orderService = orderService;
        }

        public async Task<Result<OrderDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var updateMode = request.OrderDto.ModificationType == "UPDATE" ? "UPDATE" : "APPROVE";

            await _orderService.UpdateOrApprovePurchaseOrder(request.OrderDto, updateMode);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<OrderDto>.Failure($"Failed to {updateMode} Purchase Order");
            }


            await transaction.CommitAsync(cancellationToken);

            var orderToReturn = new OrderDto
            {
                OrderId = request.OrderDto.OrderId,
                FromPartyId = request.OrderDto.FromPartyId,
                CurrencyUomId = request.OrderDto.CurrencyUomId,
                AgreementId = request.OrderDto.AgreementId,
                StatusDescription = updateMode == "Update" ? "Created" : "Approved"
            };


            return Result<OrderDto>.Success(orderToReturn);
        }
    }
}