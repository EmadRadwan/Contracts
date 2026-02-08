using Application.Accounting.Services;
using Application.Core;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        private readonly IMediator _mediator;


        public Handler(DataContext context, IFinAccountService finAccountService,
            IPaymentHelperService paymentHelperService, IMediator mediator)
        {
            _context = context;
            _finAccountService = finAccountService;
            _paymentHelperService = paymentHelperService;
            _mediator = mediator;
        }

        public async Task<Results<PaymentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var dto = request.PaymentDto;

            // ────────────────────────────────────────────────────────────────
            // 1. Load original payment (critical for comparisons)
            // ────────────────────────────────────────────────────────────────
            var original = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentId == dto.PaymentId, cancellationToken);

            if (original == null)
                return Results<PaymentDto>.Failure("الدفعة غير موجودة", "PAYMENT_NOT_FOUND");

            DateTime effectiveDate = dto.EffectiveDate ?? dto.ChequeDate ?? original.EffectiveDate ?? DateTime.UtcNow;

            // ────────────────────────────────────────────────────────────────
            // 2. Employee Advance specific validations & updates
            // ────────────────────────────────────────────────────────────────
            EmployeeAdvance? advance = null;
            bool isShortTermAdvance = original.PaymentTypeId == "EMPLOYEE_ADVANCE";

            if (original.PaymentTypeId is "EMPLOYEE_ADVANCE" or "EMPLOYEE_LONG_TERM_ADVANCE")
            {
                // Prevent changing critical identifiers
                if (dto.PartyIdTo != original.PartyIdTo)
                    return Results<PaymentDto>.Failure("لا يمكن تغيير الموظف بعد إنشاء السلفة",
                        "CANNOT_CHANGE_EMPLOYEE");

                if (dto.PaymentTypeId != original.PaymentTypeId)
                    return Results<PaymentDto>.Failure("لا يمكن تغيير نوع الدفعة للسلفة", "CANNOT_CHANGE_PAYMENT_TYPE");

                if (isShortTermAdvance)
                {
                    advance = await _context.EmployeeAdvances
                        .FirstOrDefaultAsync(ea => ea.PaymentId == dto.PaymentId, cancellationToken);

                    if (advance == null)
                        return Results<PaymentDto>.Failure("سجل السلفة المرتبط مفقود", "ADVANCE_RECORD_MISSING");

                    // ── Amount change logic ──
                    decimal oldAmount = original.Amount;
                    decimal newAmount = dto.Amount;

                    if (newAmount <= 0)
                        return Results<PaymentDto>.Failure("المبلغ يجب أن يكون أكبر من صفر", "INVALID_AMOUNT");

                    // Optional: stricter rule — prevent increase after deduction start
                    bool deductionsStarted = advance.StartDate <= DateTime.UtcNow;
                    if (newAmount > oldAmount && deductionsStarted)
                        return Results<PaymentDto>.Failure("لا يمكن زيادة السلفة بعد بدء الخصومات",
                            "CANNOT_INCREASE_AFTER_DEDUCTION");

                    // Re-check monthly limit only when increasing (short-term only)
                    if (newAmount > oldAmount)
                    {
                        var monthStart = new DateTime(effectiveDate.Year, effectiveDate.Month, 1);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                        var otherAdvancesSum = await _context.EmployeeAdvances
                            .Where(ea => ea.PartyId == dto.PartyIdTo &&
                                         ea.AdvanceDate >= monthStart &&
                                         ea.AdvanceDate <= monthEnd &&
                                         ea.PaymentId != dto.PaymentId &&
                                         ea.StatusId != "ADVANCE_CANCELLED" &&
                                         ea.StatusId != "ADVANCE_REJECTED")
                            .SumAsync(ea => ea.Amount, cancellationToken);

                        var salaryRecord = await _context.RateAmounts
                            .Where(ra => ra.PartyId == dto.PartyIdTo &&
                                         ra.PeriodTypeId == "RATE_MONTH" &&
                                         ra.FromDate <= DateTime.UtcNow &&
                                         (ra.ThruDate == null || ra.ThruDate > DateTime.UtcNow))
                            .OrderByDescending(ra => ra.FromDate)
                            .Select(ra => ra.Amount)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (salaryRecord <= 0)
                            return Results<PaymentDto>.Failure("الراتب الشهري غير صالح أو مفقود", "INVALID_SALARY");

                        decimal maxAllowed = (decimal)salaryRecord * 0.50m;
                        decimal newTotal = otherAdvancesSum + newAmount;

                        if (newTotal > maxAllowed)
                        {
                            return Results<PaymentDto>.Failure(
                                $"المبلغ الجديد ({newAmount:N2}) + السلف الأخرى في الشهر ({otherAdvancesSum:N2}) " +
                                $"يتجاوز الحد الأقصى الشهري ({maxAllowed:N2}).",
                                "MONTHLY_ADVANCE_LIMIT_EXCEEDED_ON_UPDATE");
                        }
                    }

                    // ── Update EmployeeAdvance fields ──
                    advance.Amount = newAmount;
                    advance.InstallmentAmount = newAmount; // still full amount for short-term
                    advance.AdvanceDate = effectiveDate;
                    advance.Description = dto.Comments ?? advance.Description ?? "سلفة راتب قصيرة الأجل";
                    advance.CurrencyUomId = dto.ActualCurrencyUomId ?? original.CurrencyUomId ?? "EGP";
                    advance.LastUpdatedStamp = DateTime.UtcNow;
                    advance.LastUpdatedTxStamp = DateTime.UtcNow;

                    _context.EmployeeAdvances.Update(advance);
                }
            }


            string finAccountTransId = null!;
            dto = request.PaymentDto;
            effectiveDate = (DateTime)(dto.ChequeDate
                                       ?? dto.EffectiveDate
                                       ??
                                       dto.EffectiveDate); // fallback preserves original if both are null (though unlikely)


            try
            {
                // get payment method
                var paymentMethod = await _context.PaymentMethods.SingleOrDefaultAsync(x =>
                    x.PaymentMethodId == request.PaymentDto.PaymentMethodId, cancellationToken);

                if (paymentMethod != null)
                {
                    // check if the payment method is related to a financial account
                    if (paymentMethod.FinAccountId != null)
                    {
                        // check if a finAcctTrans exists for the payment
                        var finAccountTrans = await _context.FinAccountTrans.SingleOrDefaultAsync(x =>
                            x.PaymentId == request.PaymentDto.PaymentId, cancellationToken);
                        finAccountTransId = finAccountTrans?.FinAccountTransId;
                        if (finAccountTrans != null)
                        {
                            // populate a CreateFinAccountTransParam object
                            var createFinAccountTransParam = new CreateFinAccountTransParam
                            {
                                FinAccountId = paymentMethod.FinAccountId,
                                PartyId = finAccountTrans.FinAccountTransTypeId == "DEPOSIT"
                                    ? request.PaymentDto.PartyIdFrom
                                    : request.PaymentDto.PartyIdTo,
                                Amount = request.PaymentDto.Amount,
                                EffectiveDate = effectiveDate,
                                FinAccountTransTypeId = finAccountTrans.FinAccountTransTypeId,
                                FinAccountTransId = finAccountTrans.FinAccountTransId
                            };
                            await _finAccountService.UpdateFinAccountTrans(
                                createFinAccountTransParam);
                        }
                        else
                        {
                            // populate a CreateFinAccountTransParam object
                            // based on payment type, determine if its a deposit or withdrawal
                            var incomingPaymentTypes = new List<string> { "CUSTOMER_DEPOSIT", "CUSTOMER_PAYMENT" };
                            var outgoingPaymentTypes = new List<string>
                            {
                                "VENDOR_PAYMENT", "CUSTOMER_REFUND", "COMMISSION_PAYMENT", "INCOME_TAX_PAYMENT",
                                "PAY_CHECK",
                                "PAYROL_PAYMENT", "PAYROLL_TAX_PAYMENT", "SALES_TAX_PAYMENT", "TAX_PAYMENT",
                                "COMMISSION_PAYMENT"
                            };
                            var paymentTypeId = request.PaymentDto.PaymentTypeId;
                            var createFinAccountTransParam = new CreateFinAccountTransParam();
                            if (incomingPaymentTypes.Contains(paymentTypeId))
                                // deposit
                                // prepare data for financial account transaction creation
                                createFinAccountTransParam = new CreateFinAccountTransParam
                                {
                                    FinAccountId = paymentMethod.FinAccountId,
                                    FinAccountTransTypeId = "DEPOSIT",
                                    StatusId = "FINACT_TRNS_CREATED",
                                    PartyId = request.PaymentDto.PartyIdFrom,
                                    Amount = request.PaymentDto.Amount,
                                    EffectiveDate = request.PaymentDto.EffectiveDate
                                };
                            else if (outgoingPaymentTypes.Contains(paymentTypeId))
                                // withdrawal
                                // prepare data for financial account transaction creation
                                createFinAccountTransParam = new CreateFinAccountTransParam
                                {
                                    FinAccountId = paymentMethod.FinAccountId,
                                    FinAccountTransTypeId = "WITHDRAWAL",
                                    StatusId = "FINACT_TRNS_CREATED",
                                    PartyId = request.PaymentDto.PartyIdTo,
                                    Amount = request.PaymentDto.Amount,
                                    EffectiveDate = request.PaymentDto.EffectiveDate
                                };

                            finAccountTransId =
                                await _finAccountService.CreateFinAccountTrans(createFinAccountTransParam);
                        }
                    }
                    else
                    {
                        // check if there was there a transaction from the add phase
                        // in the database and if there is then delete it
                        var finAcctTrans = _context.FinAccountTrans.SingleOrDefault(x =>
                            x.PaymentId == request.PaymentDto.PaymentId);
                        if (finAcctTrans != null) _context.FinAccountTrans.Remove(finAcctTrans);
                    }
                }
                else
                {
                    // else only update the payment, populate a CreatePaymentParam object and ignore the financial account transaction
                    var updatePaymentParam = new CreatePaymentParam
                    {
                        PaymentId = request.PaymentDto.PaymentId,
                        StatusId = request.PaymentDto.StatusId,
                        PaymentMethodId = request.PaymentDto.PaymentMethodId,
                        IsBankTransfer = request.PaymentDto.IsBankTransfer,
                        PaymentMethodTypeId = paymentMethod.PaymentMethodTypeId,
                        EffectiveDate = effectiveDate,
                        Amount = request.PaymentDto.Amount,
                        Comments = request.PaymentDto.Comments,
                        ActualCurrencyAmount = request.PaymentDto.ActualCurrencyAmount,
                        ActualCurrencyUomId = request.PaymentDto.ActualCurrencyUomId,
                        PartyIdFrom = request.PaymentDto.PartyIdFrom,
                        PartyIdTo = request.PaymentDto.PartyIdTo,
                        PaymentTypeId = request.PaymentDto.PaymentTypeId,
                        FinAccountTransId = finAccountTransId,
                        OverrideGlAccountId = request.PaymentDto.OverrideGlAccountId,
                        ProjectId = request.PaymentDto.ProjectId,
                        CostCenterId = request.PaymentDto.CostCenterId,
                        PaymentRefNum = request.PaymentDto.PaymentRefNum,
                    };
                    // update the payment itself
                    await _paymentHelperService.UpdatePayment(updatePaymentParam);
                }


                // populate a CreatePaymentParam object
                var updatePaymentParam2 = new CreatePaymentParam
                {
                    PaymentId = request.PaymentDto.PaymentId,
                    StatusId = request.PaymentDto.StatusId,
                    PaymentMethodId = request.PaymentDto.PaymentMethodId,
                    IsBankTransfer = request.PaymentDto.IsBankTransfer,
                    PaymentMethodTypeId = paymentMethod.PaymentMethodTypeId,
                    EffectiveDate = effectiveDate,
                    Amount = request.PaymentDto.Amount,
                    ActualCurrencyAmount = request.PaymentDto.ActualCurrencyAmount,
                    ActualCurrencyUomId = request.PaymentDto.ActualCurrencyUomId == ""
                        ? null
                        : request.PaymentDto.ActualCurrencyUomId,
                    Comments = request.PaymentDto.Comments,
                    PaymentRefNum = request.PaymentDto.PaymentRefNum,
                    PartyIdFrom = request.PaymentDto.PartyIdFrom,
                    PartyIdTo = request.PaymentDto.PartyIdTo,
                    PaymentTypeId = request.PaymentDto.PaymentTypeId,
                    FinAccountTransId = finAccountTransId,
                    OverrideGlAccountId = request.PaymentDto.OverrideGlAccountId,
                    ProjectId = request.PaymentDto.ProjectId,
                    CostCenterId = request.PaymentDto.CostCenterId,
                    ChequeNumber = request.PaymentDto.ChequeNumber,
                    ChequeDate = request.PaymentDto.ChequeDate
                };
                // update the payment itself
                var payment = await _paymentHelperService.UpdatePayment(updatePaymentParam2);


                var addedFinAccountTran = null as EntityEntry<FinAccountTran>;


                if (paymentMethod!.FinAccountId != null && finAccountTransId != null)
                {
                    addedFinAccountTran = _context.ChangeTracker.Entries<FinAccountTran>()
                        .FirstOrDefault(e =>
                            e.Entity.FinAccountTransId == finAccountTransId &&
                            e.State == EntityState.Added);

                    await _context.SaveChangesAsync(cancellationToken);

                    if (addedFinAccountTran != null) addedFinAccountTran.Entity.PaymentId = payment.PaymentId;
                    payment.FinAccountTransId = finAccountTransId;
                }


                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var fromParty = await _context.Parties
                    .Where(p => p.PartyId == request.PaymentDto.PartyIdFrom)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                var toParty = await _context.Parties
                    .Where(p => p.PartyId == request.PaymentDto.PartyIdTo)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);


                var paymentToReturn = new PaymentDto
                {
                    PaymentId = payment.PaymentId,
                    StatusId = payment.StatusId,
                    StatusDescription = "Not Paid",
                    FinAccountTransId = payment.FinAccountTransId,
                    Comments = payment.Comments,
                    Amount = payment.Amount,
                    IsBankTransfer = payment.IsBankTransfer,
                    PaymentMethodId = payment.PaymentMethodId,
                    PaymentTypeId = payment.PaymentTypeId,
                    EffectiveDate = payment.EffectiveDate,
                    PartyIdFromName = fromParty,
                    PartyIdToName = toParty,
                    OverrideGlAccountId = payment.OverrideGlAccountId,
                    ChequeNumber = payment.ChequeNumber,
                    ChequeDate = payment.ChequeDate,
                    PaymentRefNum = payment.PaymentRefNum
                };


                return Results<PaymentDto>.Success(paymentToReturn);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Results<PaymentDto>.Failure("Error updating Payment");
            }
        }
    }
}