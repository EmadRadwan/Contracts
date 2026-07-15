using Application.Accounting.Services;
using Application.Core;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Payments;

public class UpdatePayment
{
    public class Command : IRequest<Results<PaymentDto>>
    {
        public PaymentDto PaymentDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.PaymentDto).SetValidator(new CreatePaymentValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Results<PaymentDto>>
    {
        private readonly DataContext _context;
        private readonly IFinAccountService _finAccountService;
        private readonly IPaymentHelperService _paymentHelperService;
        private readonly IGeneralLedgerService _generalLedgerService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            DataContext context,
            IFinAccountService finAccountService,
            IPaymentHelperService paymentHelperService,
            IGeneralLedgerService generalLedgerService,
            ILogger<Handler> logger)
        {
            _context = context;
            _finAccountService = finAccountService;
            _paymentHelperService = paymentHelperService;
            _generalLedgerService = generalLedgerService;
            _logger = logger;
        }

        public async Task<Results<PaymentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var dto = request.PaymentDto;

                // Load original payment for reference
                var original = await _context.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PaymentId == dto.PaymentId, cancellationToken);

                if (original == null)
                    return Results<PaymentDto>.Failure("الدفعة غير موجودة", "PAYMENT_NOT_FOUND");

                // === NORMALIZE TO DATE-ONLY (Timezone Safe) ===
                // When IsCollectionDate is flagged the user explicitly set effectiveDate as the cheque
                // collection date, which must be later than the cheque date itself.
                if (dto.IsCollectionDate == true && dto.ChequeDate.HasValue && dto.EffectiveDate.HasValue
                    && dto.EffectiveDate.Value <= dto.ChequeDate.Value)
                    return Results<PaymentDto>.Failure("تاريخ التحصيل يجب أن يكون بعد تاريخ الشيك", "INVALID_COLLECTION_DATE");

                DateOnly effectiveDate = dto.IsCollectionDate == true
                    ? dto.EffectiveDate ?? original.EffectiveDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
                    : dto.ChequeDate ?? dto.EffectiveDate ?? original.EffectiveDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

                // Get payment type once
                var paymentType = await _context.PaymentTypes
                    .FirstOrDefaultAsync(pt => pt.PaymentTypeId == dto.PaymentTypeId, cancellationToken);

                // Get payment method
                var paymentMethod = await _context.PaymentMethods
                    .SingleOrDefaultAsync(x => x.PaymentMethodId == dto.PaymentMethodId, cancellationToken);

                string finAccountTransId = null;

                // ────────────────────────────────────────────────────────────────
                // 1. Handle Financial Account Transaction (FinAccountTrans)
                // ────────────────────────────────────────────────────────────────
                if (paymentMethod?.FinAccountId != null)
                {
                    var existingFinTrans = await _context.FinAccountTrans
                        .SingleOrDefaultAsync(x => x.PaymentId == dto.PaymentId, cancellationToken);

                    if (existingFinTrans != null)
                    {
                        // Update existing FinAccountTrans
                        var updateFinParam = new CreateFinAccountTransParam
                        {
                            FinAccountId = paymentMethod.FinAccountId,
                            PaymentId = dto.PaymentId,
                            FinAccountTransId = existingFinTrans.FinAccountTransId,
                            FinAccountTransTypeId = existingFinTrans.FinAccountTransTypeId,
                            PartyId = existingFinTrans.FinAccountTransTypeId == "DEPOSIT"
                                ? dto.PartyIdFrom
                                : dto.PartyIdTo,
                            Amount = dto.Amount,
                            EffectiveDate = effectiveDate
                        };

                        await _finAccountService.UpdateFinAccountTrans(updateFinParam);
                        finAccountTransId = existingFinTrans.FinAccountTransId;
                    }
                    else
                    {
                        // Create new FinAccountTrans
                        var isIncoming = new[] { "CUSTOMER_DEPOSIT", "CUSTOMER_PAYMENT" }
                            .Contains(dto.PaymentTypeId);

                        var createFinParam = new CreateFinAccountTransParam
                        {
                            FinAccountId = paymentMethod.FinAccountId,
                            PaymentId = dto.PaymentId,
                            FinAccountTransTypeId = isIncoming ? "DEPOSIT" : "WITHDRAWAL",
                            StatusId = "FINACT_TRNS_CREATED",
                            PartyId = isIncoming ? dto.PartyIdFrom : dto.PartyIdTo,
                            Amount = dto.Amount,
                            EffectiveDate = effectiveDate
                        };

                        finAccountTransId = await _finAccountService.CreateFinAccountTrans(createFinParam);
                    }
                }
                else
                {
                    // No FinAccount → remove any existing FinAccountTrans
                    var existingFinTrans = await _context.FinAccountTrans
                        .SingleOrDefaultAsync(x => x.PaymentId == dto.PaymentId, cancellationToken);

                    if (existingFinTrans != null)
                    {
                        _context.FinAccountTrans.Remove(existingFinTrans);
                    }
                }

                // ────────────────────────────────────────────────────────────────
                // 2. Update the Payment itself
                // ────────────────────────────────────────────────────────────────
                var updatePaymentParam = new CreatePaymentParam
                {
                    PaymentId = dto.PaymentId,
                    PartyIdFrom = dto.PartyIdFrom,
                    PartyIdTo = dto.PartyIdTo,
                    Amount = dto.Amount,
                    StatusId = dto.StatusId,
                    EffectiveDate = effectiveDate,
                    PaymentTypeId = dto.PaymentTypeId,
                    ChequeNumber = dto.ChequeNumber,
                    ChequeDate = dto.ChequeDate,
                    Comments = dto.Comments,
                    IsBankTransfer = dto.IsBankTransfer,
                    PaymentMethodId = dto.PaymentMethodId,
                    PaymentMethodTypeId = paymentMethod?.PaymentMethodTypeId,
                    OverrideGlAccountId = dto.OverrideGlAccountId,
                    ProjectId = dto.ProjectId,
                    CostCenterId = dto.CostCenterId,
                    PaymentRefNum = dto.PaymentRefNum,
                    FinAccountTransId = finAccountTransId
                };

                var updatedPayment = await _paymentHelperService.UpdatePayment(updatePaymentParam);

                // ────────────────────────────────────────────────────────────────
                // 3. Handle Postdated Cheque Accounting Transaction (CHECK_ISSUED)
                // ────────────────────────────────────────────────────────────────
                var shouldHavePostdatedTrans =
                    !string.IsNullOrEmpty(dto.OverrideGlAccountId) &&
                    paymentType?.ParentTypeId == "DISBURSEMENT" &&
                    (!string.IsNullOrEmpty(dto.ChequeNumber) || dto.ChequeDate.HasValue);

                var hasExistingPostdatedTrans = await _context.AcctgTrans
                    .AnyAsync(x => x.PaymentId == dto.PaymentId &&
                                   x.AcctgTransTypeId == "CHECK_ISSUED",
                        cancellationToken);

                if (shouldHavePostdatedTrans)
                {
                    // Delete existing one first (in case amount, GL account, cheque details changed)
                    if (hasExistingPostdatedTrans)
                    {
                        await _generalLedgerService.DeletePostdatedChequeAccountingTransaction(dto.PaymentId);
                    }

                    // Create new/updated transaction
                    try
                    {
                        var acctgTransId = await _generalLedgerService
                            .CreatePostdatedChequeAccountingTransaction(dto.PaymentId);

                        _logger.LogInformation(
                            "Postdated cheque accounting transaction {AcctgTransId} created/updated for payment {PaymentId}",
                            acctgTransId, dto.PaymentId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create postdated cheque accounting for payment {PaymentId}",
                            dto.PaymentId);
                        // We continue (do not rollback) unless you want strict failure
                    }
                }
                else if (hasExistingPostdatedTrans)
                {
                    // Conditions no longer met → remove the old transaction
                    try
                    {
                        await _generalLedgerService.DeletePostdatedChequeAccountingTransaction(dto.PaymentId);
                        _logger.LogInformation(
                            "Removed obsolete CHECK_ISSUED transaction for payment {PaymentId}",
                            dto.PaymentId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to delete obsolete postdated cheque accounting for payment {PaymentId}",
                            dto.PaymentId);
                    }
                }

                // ────────────────────────────────────────────────────────────────
                // 4. Final Save + Commit
                // ────────────────────────────────────────────────────────────────
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Load party names for response
                var fromParty = await _context.Parties
                    .Where(p => p.PartyId == dto.PartyIdFrom)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(cancellationToken);

                var toParty = await _context.Parties
                    .Where(p => p.PartyId == dto.PartyIdTo)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(cancellationToken);

                var response = new PaymentDto
                {
                    PaymentId = updatedPayment.PaymentId,
                    PaymentTypeId = updatedPayment.PaymentTypeId,
                    PaymentTypeDescription = paymentType?.Description,
                    PaymentMethodTypeId = updatedPayment.PaymentMethodTypeId,
                    StatusId = updatedPayment.StatusId,
                    StatusDescription = "Not Paid", // Adjust if you have proper status logic
                    Amount = updatedPayment.Amount,
                    IsBankTransfer = updatedPayment.IsBankTransfer,
                    IsCollectionDate = dto.IsCollectionDate,
                    PaymentMethodId = updatedPayment.PaymentMethodId,
                    EffectiveDate = updatedPayment.EffectiveDate,
                    PartyIdFrom = updatedPayment.PartyIdFrom,
                    PartyIdFromName = fromParty,
                    PartyIdTo = updatedPayment.PartyIdTo,
                    PartyIdToName = toParty,
                    RoleTypeIdTo = updatedPayment.RoleTypeIdTo,
                    OverrideGlAccountId = updatedPayment.OverrideGlAccountId,
                    ChequeNumber = updatedPayment.ChequeNumber,
                    ChequeDate = updatedPayment.ChequeDate,
                    PaymentRefNum = updatedPayment.PaymentRefNum,
                    Comments = updatedPayment.Comments,
                    CurrencyUomId = updatedPayment.CurrencyUomId,
                    ActualCurrencyAmount = updatedPayment.ActualCurrencyAmount,
                    ActualCurrencyUomId = updatedPayment.ActualCurrencyUomId,
                    FinAccountTransId = finAccountTransId,
                    ProjectId = updatedPayment.WorkEffortId,
                    CostCenterId = updatedPayment.CostCenterId
                };

                return Results<PaymentDto>.Success(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error updating payment {PaymentId}", request.PaymentDto.PaymentId);
                return Results<PaymentDto>.Failure($"Error updating Payment: {ex.Message}");
            }
        }
    }
}