using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Accounting.Services;
using Application.Core;

namespace Application.Accounting.Payments;

public class SetPaymentStatusToReceived
{
    public class Command : IRequest<Results<PaymentChangeStatusDto>>
    {
        public PaymentChangeStatusDto PaymentChangeStatusDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.PaymentChangeStatusDto).SetValidator(new UpdatePaymentStatusValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Results<PaymentChangeStatusDto>>
    {
        private readonly DataContext _context;
        private readonly IPaymentHelperService _paymentHelperService;

        public Handler(DataContext context, IPaymentHelperService paymentHelperService)
        {
            _context = context;
            _paymentHelperService = paymentHelperService;
        }

        public async Task<Results<PaymentChangeStatusDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Call SetPaymentStatus and capture the result
                var paymentResult = await _paymentHelperService.SetPaymentStatus(
                    request.PaymentChangeStatusDto.PaymentId,
                    request.PaymentChangeStatusDto.StatusId);

                // REFACTOR: Check PaymentStatusChangeResult for success before proceeding
                if (!paymentResult.Success)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    // REFACTOR: Use Results.Failure with ErrorMessage and ErrorCode
                    return Results<PaymentChangeStatusDto>.Failure(
                        paymentResult.ErrorMessage,
                        paymentResult.ErrorCode
                    );
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Retrieve the new status description
                var newStatus = await _context.StatusItems
                    .SingleOrDefaultAsync(x => x.StatusId == paymentResult.UpdatedPayment.StatusId, cancellationToken);
                
                // REFACTOR: Handle case where status item is not found
                if (newStatus == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    // REFACTOR: Use Results.Failure with ErrorMessage and ErrorCode
                    return Results<PaymentChangeStatusDto>.Failure(
                        "Status description not found.",
                        "STATUS_DESCRIPTION_NOT_FOUND"
                    );
                }

                var paymentToReturn = new PaymentChangeStatusDto
                {
                    StatusId = paymentResult.UpdatedPayment.StatusId,
                    StatusDescription = newStatus.Description,
                };

                return Results<PaymentChangeStatusDto>.Success(paymentToReturn);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                // REFACTOR: Use Results.Failure with ErrorMessage and ErrorCode for unexpected failures
                return Results<PaymentChangeStatusDto>.Failure(
                    ex.Message ?? "An unexpected error occurred while setting payment status.",
                    "UNEXPECTED_ERROR"
                );
            }
        }
    }
}