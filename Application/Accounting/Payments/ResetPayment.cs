using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Microsoft.Extensions.Logging;

namespace Application.Accounting.Payments;

public class ResetPayment
{
    public class Command : IRequest<Results<PaymentDto>>
    {
        public string PaymentId { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, Results<PaymentDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Results<PaymentDto>> Handle(Command request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.PaymentId))
            {
                return Results<PaymentDto>.Failure("Payment ID is required", "INVALID_INPUT");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // 1. Fetch the current payment (to validate + get reference data)
                var original = await _context.Payments
                    .AsNoTracking()
                    .Include(p => p.PaymentPreference) // optional - if you need it later
                    .FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId, ct);

                if (original == null)
                {
                    return Results<PaymentDto>.Failure("Payment not found", "PAYMENT_NOT_FOUND");
                }

                // Optional guard: prevent reset on final/closed statuses
                if (original.StatusId is "PMNT_VOIDED" or "PMNT_CANCELLED" or "PMNT_CONFIRMED")
                {
                    return Results<PaymentDto>.Failure(
                        $"Cannot reset payment in status '{original.StatusId}'",
                        "INVALID_PAYMENT_STATUS"
                    );
                }

                // 2. Reset core fields on the payment
                var paymentToUpdate = new Payment { PaymentId = request.PaymentId };
                _context.Attach(paymentToUpdate);

                paymentToUpdate.StatusId           = "PMNT_NOT_PAID";
                paymentToUpdate.OverrideGlAccountId = null; // optional - clear override if desired
                paymentToUpdate.LastUpdatedStamp   = DateTime.UtcNow;
                paymentToUpdate.LastUpdatedTxStamp = DateTime.UtcNow;

                // 3. Delete related PaymentApplications (outgoing - payment → invoice)
                await _context.PaymentApplications
                    .Where(pa => pa.PaymentId == request.PaymentId)
                    .ExecuteDeleteAsync(ct);

                // Optional: also clean incoming side (toPaymentId) if used in your system
                // await _context.PaymentApplications
                //     .Where(pa => pa.ToPaymentId == request.PaymentId)
                //     .ExecuteDeleteAsync(ct);

                // 4. Delete accounting transactions (entries first, then headers)
                await _context.AcctgTransEntries
                    .Where(ate => _context.AcctgTrans
                        .Any(at => at.AcctgTransId == ate.AcctgTransId &&
                                   at.PaymentId == request.PaymentId))
                    .ExecuteDeleteAsync(ct);

                await _context.AcctgTrans
                    .Where(at => at.PaymentId == request.PaymentId)
                    .ExecuteDeleteAsync(ct);

                // 5. Optional: reset linked OrderPaymentPreference status
                if (!string.IsNullOrEmpty(original.PaymentPreferenceId))
                {
                    var preferenceToUpdate = new OrderPaymentPreference
                    {
                        OrderPaymentPreferenceId = original.PaymentPreferenceId
                    };
                    _context.Attach(preferenceToUpdate);
                    preferenceToUpdate.StatusId = "PMNT_NOT_PAID";
                    preferenceToUpdate.LastModifiedDate = DateTime.UtcNow;
                }

                // 6. Save all changes
                await _context.SaveChangesAsync(ct);

                // 7. Reload the updated payment entity
                var updatedPayment = await _context.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId, ct);

                if (updatedPayment == null)
                {
                    throw new InvalidOperationException("Failed to reload updated payment after reset");
                }

                // 8. Load party names for DTO (same pattern as UpdatePayment)
                var fromPartyName = await _context.Parties
                    .Where(p => p.PartyId == updatedPayment.PartyIdFrom)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(ct);

                var toPartyName = await _context.Parties
                    .Where(p => p.PartyId == updatedPayment.PartyIdTo)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(ct);

                // 9. Build DTO to return
                var paymentDto = new PaymentDto
                {
                    PaymentId          = updatedPayment.PaymentId,
                    StatusId           = updatedPayment.StatusId,
                    StatusDescription  = "Not Paid", // or load from StatusItem if needed
                    Amount             = updatedPayment.Amount,
                    EffectiveDate      = updatedPayment.EffectiveDate,
                    Comments           = updatedPayment.Comments,
                    PaymentMethodId    = updatedPayment.PaymentMethodId,
                    PaymentTypeId      = updatedPayment.PaymentTypeId,
                    PartyIdFrom        = updatedPayment.PartyIdFrom,
                    PartyIdFromName    = fromPartyName,
                    PartyIdTo          = updatedPayment.PartyIdTo,
                    PartyIdToName      = toPartyName,
                    OverrideGlAccountId = updatedPayment.OverrideGlAccountId,
                    ChequeNumber       = updatedPayment.ChequeNumber,
                    ChequeDate         = updatedPayment.ChequeDate,
                    PaymentRefNum      = updatedPayment.PaymentRefNum,
                    CostCenterId       = updatedPayment.CostCenterId,
                    IsBankTransfer     = updatedPayment.IsBankTransfer,
                };

                await transaction.CommitAsync(ct);

                _logger.LogInformation("Payment {PaymentId} successfully reset", request.PaymentId);

                return Results<PaymentDto>.Success(paymentDto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogWarning(ex, "Concurrency error while resetting payment {PaymentId}", request.PaymentId);
                return Results<PaymentDto>.Failure(
                    "The payment was modified by another user. Please refresh and try again.",
                    "CONCURRENCY_ERROR"
                );
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Database error while resetting payment {PaymentId}", request.PaymentId);

                string userMessage = "Failed to reset payment due to a database error.";
                if (ex.InnerException?.Message.Contains("FK_") == true)
                {
                    userMessage = "Cannot reset payment – it is referenced by other records.";
                }

                return Results<PaymentDto>.Failure(userMessage, "DATABASE_ERROR");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Unexpected error resetting payment {PaymentId}", request.PaymentId);
                return Results<PaymentDto>.Failure(
                    "An unexpected error occurred while resetting the payment.",
                    "UNEXPECTED_ERROR"
                );
            }
        }
    }
}