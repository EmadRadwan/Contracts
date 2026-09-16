using Application.Accounting.Services;
using Application.Core;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Payments;

/// <summary>
/// Marks a payment void instead of deleting it: status PMNT_VOID, ledger reversed by linked
/// contra transactions, bank transactions cancelled, invoice applications removed. This is the
/// auditor-facing replacement for physically deleting a payment that has touched the ledger.
/// </summary>
public class VoidPayment
{
    public class Command : IRequest<Result<PaymentVoidOutcome>>
    {
        public string PaymentId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.PaymentId).NotEmpty().WithMessage("Payment ID is required.");
            RuleFor(x => x.Reason).NotEmpty().WithMessage("سبب الإلغاء مطلوب. (A reason is required to void a payment.)")
                .MaximumLength(500);
        }
    }

    public class Handler : IRequestHandler<Command, Result<PaymentVoidOutcome>>
    {
        private readonly DataContext _context;
        private readonly IPaymentVoidService _voidService;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, IPaymentVoidService voidService, ILogger<Handler> logger)
        {
            _context = context;
            _voidService = voidService;
            _logger = logger;
        }

        public async Task<Result<PaymentVoidOutcome>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var result = await _voidService.VoidPaymentAsync(request.PaymentId, request.Reason, ct);
                if (!result.IsSuccess)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<PaymentVoidOutcome>.Failure(result.ErrorMessage);
                }

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return Result<PaymentVoidOutcome>.Success(result.ResultData);
            }
            catch (ClosedAccountingPeriodException ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<PaymentVoidOutcome>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error voiding payment {PaymentId}", request.PaymentId);
                return Result<PaymentVoidOutcome>.Failure($"حدث خطأ أثناء إلغاء الدفعة: {ex.GetBaseException().Message}");
            }
        }
    }
}
