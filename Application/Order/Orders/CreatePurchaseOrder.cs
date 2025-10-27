using Application.Interfaces;
using Application.order.Orders;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders;

public class CreatePurchaseOrder
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
            // REFACTOR: Wrap the entire operation in a try-catch to handle any unexpected exceptions
            // Improves error handling by ensuring all potential failures are caught and reported
            try
            {
                var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                // create purchase order
                var newPurchaseOrder = await _orderService.CreatePurchaseOrder(request.OrderDto);

                // REFACTOR: Added try-catch around SaveChangesAsync to specifically handle database exceptions
                // This isolates the database save operation, allowing specific error handling and transaction rollback
                try
                {
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    /*if (!result)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<OrderDto>.Failure("Failed to create Purchase Order");
                    }*/

                    await transaction.CommitAsync(cancellationToken);
                }
                catch (DbUpdateException ex)
                {
                    // REFACTOR: Added specific exception handling for DbUpdateException
                    // Provides more detailed error information and ensures transaction rollback
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<OrderDto>.Failure($"Database error while saving purchase order: {ex.Message}");
                }

                var orderToReturn = new OrderDto
                {
                    OrderId = newPurchaseOrder.OrderId,
                    FromPartyId = request.OrderDto.FromPartyId,
                    StatusDescription = "Created",
                    InternalRemarks = request.OrderDto.InternalRemarks,
                    AgreementId = request.OrderDto.AgreementId,
                    CurrencyUomId = request.OrderDto.CurrencyUomId
                };
                
                return Result<OrderDto>.Success(orderToReturn);
            }
            catch (Exception ex)
            {
                // REFACTOR: Catch all other unexpected exceptions
                // Ensures the application doesn't crash and provides a user-friendly error message
                return Result<OrderDto>.Failure($"An error occurred while creating purchase order: {ex.Message}");
            }
        }
    }
}