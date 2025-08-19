using Application.Interfaces;
using Application.order.Orders;
using Application.Order.Orders;
using Application.Order.Quotes;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.JobOrders;

public class CreateJobOrderFromQuote
{
    public class Command : IRequest<Result<OrderDto>>
    {
        public QuoteDto QuoteDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.QuoteDto).SetValidator(new QuoteValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<OrderDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IUserAccessor _userAccessor;


        public Handler(DataContext context, IUserAccessor userAccessor, IOrderService orderService,
            ILogger<CreateJobOrderFromQuote> logger)
        {
            _userAccessor = userAccessor;
            _context = context;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<Result<OrderDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var transaction = _context.Database.BeginTransaction();

                _logger.LogInformation("Order Creation Started. {Transaction} for {QuoteId}",
                    "complete sales order", request.QuoteDto.QuoteId);
                var newJobOrder = await _orderService.CreateJobOrderFromQuote(request.QuoteDto);
                _logger.LogInformation("Order Created successfully. {Transaction} for {QuoteId}",
                    "complete sales order", request.QuoteDto.QuoteId);


                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<OrderDto>.Failure("Failed to create Job Order");
                }

                await transaction.CommitAsync(cancellationToken);


                var orderToReturn = new OrderDto
                {
                    OrderId = newJobOrder.OrderId,
                    FromPartyId = request.QuoteDto.FromPartyId,
                    StatusDescription = "Created"
                };
                return Result<OrderDto>.Success(orderToReturn);
            }
            catch (Exception ex)
            {
                return Result<OrderDto>.Failure("An error occurred while creating job order: " + ex.Message);
            }
        }
    }
}