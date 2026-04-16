using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Accounting.Services;
using Application.Core;

namespace Application.Accounting.Payments;

/// <summary>
/// DTO that mirrors the response of CreatePaymentAndFinAccountTrans
/// </summary>
public class PaymentDetailsResponse
{
    public string PaymentId { get; set; }
    public string StatusId { get; set; }
    public string StatusDescription { get; set; }
    public string CurrencyUomId { get; set; }
    public string ActualCurrencyUomId { get; set; }
    public string FinAccountTransId { get; set; }
    public string ChequeNumber { get; set; }
    public DateOnly? ChequeDate { get; set; }
    public string Comments { get; set; }
    public string PartyIdFromName { get; set; }
    public string PartyIdToName { get; set; }
}

public class SetPaymentStatus
{
    // ------------------------------------------------------------
    // 1. Command
    // ------------------------------------------------------------
    public class Command : IRequest<Results<PaymentDetailsResponse>>
    {
        public PaymentChangeStatusDto PaymentChangeStatusDto { get; set; }
    }

    // ------------------------------------------------------------
    // 2. Validation (unchanged – just points to the existing validator)
    // ------------------------------------------------------------
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.PaymentChangeStatusDto).SetValidator(new UpdatePaymentStatusValidator());
        }
    }

    // ------------------------------------------------------------
    // 3. Handler
    // ------------------------------------------------------------
    public class Handler : IRequestHandler<Command, Results<PaymentDetailsResponse>>
    {
        private readonly DataContext _context;
        private readonly IPaymentHelperService _paymentHelperService;

        public Handler(DataContext context, IPaymentHelperService paymentHelperService)
        {
            _context = context;
            _paymentHelperService = paymentHelperService;
        }

        public async Task<Results<PaymentDetailsResponse>> Handle(Command request,
            CancellationToken ct)
        {
            var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // ---- 1. Change the status -------------------------------------------------
                var paymentResult = await _paymentHelperService.SetPaymentStatus(
                    request.PaymentChangeStatusDto.PaymentId,
                    request.PaymentChangeStatusDto.StatusId);

                if (!paymentResult.Success)
                {
                    await transaction.RollbackAsync(ct);
                    return Results<PaymentDetailsResponse>.Failure(
                        paymentResult.ErrorMessage,
                        paymentResult.ErrorCode);
                }

                // Save the status change (the helper already persisted the payment)
                await _context.SaveChangesAsync(ct);

                // ---- 2. Load the **full** payment with required navigation data ----------
                var payment = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.PaymentId == request.PaymentChangeStatusDto.PaymentId)
                    .Select(p => new
                    {
                        p.PaymentId,
                        p.StatusId,
                        p.CurrencyUomId,
                        p.ActualCurrencyUomId,
                       // p.FinAccountTransId,
                        p.ChequeNumber,
                        p.ChequeDate,
                        p.Comments,
                        p.PartyIdFrom,
                        p.PartyIdTo
                    })
                    .FirstOrDefaultAsync(ct);

                if (payment == null)
                {
                    await transaction.RollbackAsync(ct);
                    return Results<PaymentDetailsResponse>.Failure(
                        "Payment not found after status change.",
                        "PAYMENT_NOT_FOUND");
                }

                // ---- 3. Resolve status description ----------------------------------------
                var statusItem = await _context.StatusItems
                    .AsNoTracking()
                    .Where(s => s.StatusId == payment.StatusId)
                    .Select(s => s.Description)
                    .FirstOrDefaultAsync(ct);

                if (statusItem == null)
                {
                    await transaction.RollbackAsync(ct);
                    return Results<PaymentDetailsResponse>.Failure(
                        "Status description not found.",
                        "STATUS_DESCRIPTION_NOT_FOUND");
                }

                // ---- 4. Resolve party names ------------------------------------------------
                var fromPartyName = await _context.Parties
                    .AsNoTracking()
                    .Where(p => p.PartyId == payment.PartyIdFrom)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(ct);

                var toPartyName = await _context.Parties
                    .AsNoTracking()
                    .Where(p => p.PartyId == payment.PartyIdTo)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(ct);

                // ---- 4.1 Update Employee Advance Status if applicable ----------------------
                var paymentType = await _context.Payments
                    .Where(p => p.PaymentId == request.PaymentChangeStatusDto.PaymentId)
                    .Select(p => p.PaymentTypeId)
                    .FirstOrDefaultAsync(ct);

                if (paymentType is "EMPLOYEE_ADVANCE" or "EMPLOYEE_LONG_TERM_ADVANCE")
                {
                    var advance = await _context.EmployeeAdvances
                        .FirstOrDefaultAsync(ea => ea.PaymentId == request.PaymentChangeStatusDto.PaymentId, ct);

                    if (advance != null)
                    {
                        advance.StatusId = "ADVANCE_APPROVED";
                        advance.LastUpdatedStamp = DateTime.UtcNow;
                        await _context.SaveChangesAsync(ct);
                    }
                }

                // ---- 5. Commit transaction -------------------------------------------------
                await transaction.CommitAsync(ct);

                // ---- 6. Build the rich response --------------------------------------------
                var response = new PaymentDetailsResponse
                {
                    PaymentId = payment.PaymentId,
                    StatusId = payment.StatusId,
                    StatusDescription = statusItem,
                    CurrencyUomId = payment.CurrencyUomId,
                    ActualCurrencyUomId = payment.ActualCurrencyUomId,
                    //FinAccountTransId = payment.FinAccountTransId,
                    ChequeNumber = payment.ChequeNumber,
                    ChequeDate = payment.ChequeDate,
                    Comments = payment.Comments,
                    PartyIdFromName = fromPartyName,
                    PartyIdToName = toPartyName
                };

                return Results<PaymentDetailsResponse>.Success(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Results<PaymentDetailsResponse>.Failure(
                    ex.Message ?? "An unexpected error occurred while setting payment status.",
                    "UNEXPECTED_ERROR");
            }
        }
    }
}