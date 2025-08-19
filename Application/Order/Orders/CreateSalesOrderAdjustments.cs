using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Order.Orders;

public class CreateSalesOrderAdjustments
{
    public class Command : IRequest<Result<OrderAdjustmentsDto>>
    {
        public OrderAdjustmentsDto OrderAdjustmentsDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.OrderAdjustmentsDto).SetValidator(new OrderAdjustmentsValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<OrderAdjustmentsDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
            _context = context;
        }

        public async Task<Result<OrderAdjustmentsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            var stamp = DateTime.UtcNow;


            foreach (var adjustment in request.OrderAdjustmentsDto.OrderAdjustments)
                if (!adjustment.IsAdjustmentDeleted)
                {
                    var newAdjustment = new OrderAdjustment
                    {
                        OrderAdjustmentId = adjustment.OrderAdjustmentId,
                        OrderAdjustmentTypeId = adjustment.OrderAdjustmentTypeId,
                        OrderId = request.OrderAdjustmentsDto.OrderId,
                        OrderItemSeqId = adjustment.OrderItemSeqId,
                        CorrespondingProductId = adjustment.CorrespondingProductId,
                        Description = adjustment.Description,
                        IsManual = adjustment.IsManual,
                        SourcePercentage = adjustment.SourcePercentage,
                        ProductPromoId = adjustment.ProductPromoId,
                        Amount = adjustment.Amount,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp
                    };
                    _context.OrderAdjustments.Add(newAdjustment);
                }


            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                transaction.Rollback();
                return Result<OrderAdjustmentsDto>.Failure("Failed to create Sales Order Adjustment");
            }

            transaction.Commit();


            var orderToReturn = new OrderAdjustmentsDto
            {
                OrderId = request.OrderAdjustmentsDto.OrderId,
                StatusDescription = "Created"
            };

            return Result<OrderAdjustmentsDto>.Success(orderToReturn);
        }
    }
}