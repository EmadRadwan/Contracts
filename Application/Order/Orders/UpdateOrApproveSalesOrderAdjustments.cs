using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders;

public class UpdateOrApproveSalesOrderAdjustments
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

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());

            var updateMode = request.OrderAdjustmentsDto.ModificationType == "UPDATE" ? "UPDATE" : "APPROVE";

            foreach (var updatedAdjustment in request.OrderAdjustmentsDto.OrderAdjustments)
            {
                var savedAdjustment = _context.OrderAdjustments.FirstOrDefault(adjustment =>
                    adjustment.OrderAdjustmentId == updatedAdjustment.OrderAdjustmentId);

                if (savedAdjustment != null)
                {
                    if (updatedAdjustment.IsAdjustmentDeleted)
                    {
                        _context.OrderAdjustments.Remove(savedAdjustment);
                    }
                    else
                    {
                        savedAdjustment.OrderAdjustmentTypeId = updatedAdjustment.OrderAdjustmentTypeId;
                        savedAdjustment.OrderItemSeqId = updatedAdjustment.OrderItemSeqId;
                        savedAdjustment.CorrespondingProductId = updatedAdjustment.CorrespondingProductId;
                        savedAdjustment.Amount = updatedAdjustment.Amount;
                        savedAdjustment.Description = updatedAdjustment.Description;
                        savedAdjustment.IsManual = updatedAdjustment.IsManual;
                        savedAdjustment.SourcePercentage = updatedAdjustment.SourcePercentage;
                        savedAdjustment.ProductPromoId = updatedAdjustment.ProductPromoId;
                        savedAdjustment.LastUpdatedStamp = stamp;
                    }
                }
                else
                {
                    var newAdjustment = new OrderAdjustment
                    {
                        OrderAdjustmentId = updatedAdjustment.OrderAdjustmentId,
                        OrderAdjustmentTypeId = updatedAdjustment.OrderAdjustmentTypeId,
                        OrderId = request.OrderAdjustmentsDto.OrderId,
                        OrderItemSeqId = updatedAdjustment.OrderItemSeqId,
                        CorrespondingProductId = updatedAdjustment.CorrespondingProductId,
                        Amount = updatedAdjustment.Amount,
                        Description = updatedAdjustment.Description,
                        IsManual = updatedAdjustment.IsManual,
                        SourcePercentage = updatedAdjustment.SourcePercentage,
                        ProductPromoId = updatedAdjustment.ProductPromoId,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };
                    _context.OrderAdjustments.Add(newAdjustment);
                }
            }


            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                transaction.Rollback();
                return Result<OrderAdjustmentsDto>.Failure($"Failed to {updateMode} Sales Order Adjustments");
            }

            transaction.Commit();

            var orderToReturn = new OrderAdjustmentsDto
            {
                OrderId = request.OrderAdjustmentsDto.OrderId,
                StatusDescription = updateMode == "Update" ? "Created" : "Approved"
            };


            return Result<OrderAdjustmentsDto>.Success(orderToReturn);
        }
    }
}