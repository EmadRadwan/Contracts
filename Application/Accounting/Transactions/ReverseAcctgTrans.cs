using Application.Accounting.Services;
using Application.Core;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Transactions;

/// <summary>
/// Cancels a posted accounting transaction by writing its linked mirror image. This replaces
/// physical deletion of posted journal entries (auditor soft-delete requirement, step 3).
/// Unposted transactions are still deleted through <see cref="DeleteAcctgTrans"/>.
/// </summary>
public class ReverseAcctgTrans
{
    public class Command : IRequest<Result<ReverseAcctgTransResult>>
    {
        public string AcctgTransId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateOnly? ReversalDate { get; set; }
    }

    public class ReverseAcctgTransResult
    {
        public string OriginalAcctgTransId { get; set; } = null!;
        public string ReversalAcctgTransId { get; set; } = null!;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.AcctgTransId).NotEmpty().WithMessage("Accounting transaction ID is required.");
            RuleFor(x => x.Reason).NotEmpty().WithMessage("سبب العكس مطلوب. (A reason is required to reverse a transaction.)")
                .MaximumLength(500);
        }
    }

    public class Handler : IRequestHandler<Command, Result<ReverseAcctgTransResult>>
    {
        private readonly DataContext _context;
        private readonly IAcctgTransReversalService _reversal;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, IAcctgTransReversalService reversal, ILogger<Handler> logger)
        {
            _context = context;
            _reversal = reversal;
            _logger = logger;
        }

        public async Task<Result<ReverseAcctgTransResult>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var result = await _reversal.ReverseAsync(request.AcctgTransId,
                    new ReverseAcctgTransOptions { Reason = request.Reason, ReversalDate = request.ReversalDate }, ct);

                if (!result.IsSuccess)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<ReverseAcctgTransResult>.Failure(result.ErrorMessage);
                }

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return Result<ReverseAcctgTransResult>.Success(new ReverseAcctgTransResult
                {
                    OriginalAcctgTransId = request.AcctgTransId,
                    ReversalAcctgTransId = result.ResultData
                });
            }
            catch (ClosedAccountingPeriodException ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<ReverseAcctgTransResult>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error reversing accounting transaction {AcctgTransId}", request.AcctgTransId);
                return Result<ReverseAcctgTransResult>.Failure(
                    $"حدث خطأ أثناء عكس القيد: {ex.GetBaseException().Message}");
            }
        }
    }
}
