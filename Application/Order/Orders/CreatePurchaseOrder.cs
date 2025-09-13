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
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            // create purchase order
            var newPurchaseOrder = await _orderService.CreatePurchaseOrder(request.OrderDto);


            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                transaction.Rollback();
                return Result<OrderDto>.Failure("Failed to create Purchase Order");
            }

            transaction.Commit();


            var orderToReturn = new OrderDto
            {
                OrderId = newPurchaseOrder.OrderId,
                FromPartyId = request.OrderDto.FromPartyId,
                StatusDescription = "Created",
                InternalRemarks = request.OrderDto.InternalRemarks,
                AgreementId = request.OrderDto.AgreementId,
                CurrencyUomId = request.OrderDto.CurrencyUomId
            };
            
            var affectedRecords = _context.ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .Select(e => new ChangeRecord
                {
                    TableName = e.Entity.GetType().Name,
                    PKValues = string.Join(", ", e.Properties
                        .Where(p => p.Metadata.IsPrimaryKey())
                        .Select(p => $"{p.Metadata.Name}: {p.CurrentValue}")),
                    Operation = e.State.ToString()
                })
                .ToList();
       
            foreach (var record in affectedRecords)
            {
                Console.WriteLine(record);
            }

            return Result<OrderDto>.Success(orderToReturn);
        }
    }
}