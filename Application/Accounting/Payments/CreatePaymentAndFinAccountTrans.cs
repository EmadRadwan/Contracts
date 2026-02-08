using Application.Accounting.FinAccounts;
using Application.Accounting.Services;
using Application.Core;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;

public class CreatePaymentAndFinAccountTrans
{
    public class Command : IRequest<Results<CreatePaymentAndFinAccountTransResponse>>
    {
        public CreatePaymentAndFinAccountTransRequest request { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.request).SetValidator(new CreatePaymentAndFinAccountTransRequestValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Results<CreatePaymentAndFinAccountTransResponse>>
    {
        private readonly IPaymentHelperService _paymentHelperService;
        private readonly DataContext _context;

        public Handler(DataContext context, IPaymentHelperService paymentHelperService)
        {
            _paymentHelperService = paymentHelperService;
            _context = context;
        }

        public async Task<Results<CreatePaymentAndFinAccountTransResponse>> Handle(
            Command command,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var req = command.request;
                DateTime effectiveDate = req.PaymentDate ?? DateTime.UtcNow; // fallback to today

                // ────────────────────────────────────────────────────────────────
                // Validation block for employee advances
                // ────────────────────────────────────────────────────────────────
                if (req.PaymentTypeId is "EMPLOYEE_ADVANCE" or "EMPLOYEE_LONG_TERM_ADVANCE")
                {
                    if (string.IsNullOrEmpty(req.PartyIdTo))
                    {
                        return Results<CreatePaymentAndFinAccountTransResponse>.Failure(
                            "معرف الموظف (PartyIdTo) مطلوب للسلفة.",
                            errorCode: "EMPLOYEE_ID_REQUIRED"
                        );
                    }

                    var employeeId = req.PartyIdTo;

                    // 1. EMPLOYEE role missing
                    var hasEmployeeRole = await _context.PartyRoles
                        .AnyAsync(pr =>
                                pr.PartyId == employeeId &&
                                pr.RoleTypeId == "EMPLOYEE",
                            cancellationToken);

                    if (!hasEmployeeRole)
                    {
                        return Results<CreatePaymentAndFinAccountTransResponse>.Failure(
                            "هذا الطرف ليس موظفًا مسجلاً (لا يوجد دور EMPLOYEE). يرجى التأكد من إنشاء الموظف بشكل صحيح أولاً.",
                            errorCode: "EMPLOYEE_ROLE_MISSING"
                        );
                    }

                    // 2. No active employment
                    var hasActiveEmployment = await _context.Employments
                        .AnyAsync(e =>
                                e.PartyIdTo == employeeId &&
                                e.FromDate <= DateTime.UtcNow &&
                                (e.ThruDate == null || e.ThruDate > DateTime.UtcNow),
                            cancellationToken);

                    if (!hasActiveEmployment)
                    {
                        return Results<CreatePaymentAndFinAccountTransResponse>.Failure(
                            "لا يوجد سجل توظيف نشط لهذا الموظف. يرجى إكمال بيانات التوظيف قبل إصدار سلفة.",
                            errorCode: "NO_ACTIVE_EMPLOYMENT"
                        );
                    }

                    // 3. Missing GL account linkage
                    var glTypes = await _context.PartyGlAccounts
                        .Where(pga =>
                            pga.PartyId == employeeId &&
                            pga.OrganizationPartyId == "Company" &&
                            pga.RoleTypeId == "EMPLOYEE" &&
                            (pga.GlAccountTypeId == "ACCOUNTS_RECEIVABLE" ||
                             pga.GlAccountTypeId == "ACCOUNTS_PAYABLE"))
                        .Select(pga => pga.GlAccountTypeId)
                        .Distinct()
                        .ToListAsync(cancellationToken);

                    var hasReceivable = glTypes.Contains("ACCOUNTS_RECEIVABLE");
                    var hasPayable = glTypes.Contains("ACCOUNTS_PAYABLE");

                    if (!hasReceivable || !hasPayable)
                    {
                        var missing = new List<string>();
                        if (!hasReceivable) missing.Add("ACCOUNTS_RECEIVABLE (ذمم مدينة)");
                        if (!hasPayable) missing.Add("ACCOUNTS_PAYABLE (مستحقات)");

                        return Results<CreatePaymentAndFinAccountTransResponse>.Failure(
                            $"حساب/حسابات دفتر الأستاذ غير مرتبطة بالموظف: {string.Join(" و ", missing)}. " +
                            "يرجى التأكد من إكمال إعداد الحسابات الفرعية للموظف قبل إصدار السلفة.",
                            errorCode: "GL_ACCOUNTS_NOT_LINKED"
                        );
                    }

                    // 4. No valid monthly salary
                    var salaryRecord = await _context.RateAmounts
                        .Where(ra =>
                            ra.PartyId == employeeId &&
                            ra.PeriodTypeId == "RATE_MONTH" &&
                            ra.FromDate <= DateTime.UtcNow &&
                            (ra.ThruDate == null || ra.ThruDate > DateTime.UtcNow))
                        .OrderByDescending(ra => ra.FromDate)
                        .Select(ra => ra.Amount)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (salaryRecord <= 0)
                    {
                        return Results<CreatePaymentAndFinAccountTransResponse>.Failure(
                            "لم يتم العثور على راتب شهري صالح للموظف أو قيمته صفر. يرجى تسجيل الراتب الأساسي أولاً.",
                            errorCode: "INVALID_OR_MISSING_SALARY"
                        );
                    }

                    decimal monthlySalary = (decimal)salaryRecord;
                    decimal maxAllowed = monthlySalary * 0.50m;


                    // 5. Short-term advance: 50% limit exceeded
                    // ── Only for short-term: check previous advances in current month ──
                    if (req.PaymentTypeId == "EMPLOYEE_ADVANCE")
                    {
                        // Start and end of the month of effectiveDate
                        var monthStart = new DateTime(effectiveDate.Year, effectiveDate.Month, 1);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                        var previousAdvances = await _context.EmployeeAdvances
                            .Where(ea =>
                                ea.PartyId == employeeId &&
                                ea.AdvanceDate >= monthStart &&
                                ea.AdvanceDate <= monthEnd &&
                                ea.StatusId != "ADVANCE_CANCELLED" &&   // exclude cancelled
                                ea.StatusId != "ADVANCE_REJECTED")
                            .SumAsync(ea => ea.Amount, cancellationToken);

                        decimal totalThisMonth = previousAdvances + req.Amount;

                        if (totalThisMonth > maxAllowed)
                        {
                            return Results<CreatePaymentAndFinAccountTransResponse>.Failure(
                                $"المبلغ المطلوب ({req.Amount:N2}) + السلف الموجودة في الشهر الحالي ({previousAdvances:N2}) " +
                                $"يتجاوز الحد الأقصى للسلفة قصيرة الأجل (50% من الراتب = {maxAllowed:N2}).",
                                "MONTHLY_ADVANCE_LIMIT_EXCEEDED"
                            );
                        }
                    }
                }


                // ────────────────────────────────────────────────────────────────
                // All validations passed → proceed
                // ────────────────────────────────────────────────────────────────
                var paymentResult = await _paymentHelperService.CreatePaymentAndFinAccountTrans(req);
                

                var createdPayment = paymentResult.Value;

                // ────────────────────────────────────────────────────────────────
                // If short-term advance → create EmployeeAdvance record
                // ────────────────────────────────────────────────────────────────
                if (req.PaymentTypeId == "EMPLOYEE_ADVANCE")
                {
                    var advance = new EmployeeAdvance
                    {
                        AdvanceId = Guid.NewGuid().ToString(),
                        PartyId = req.PartyIdTo,
                        AdvanceDate = effectiveDate,
                        Amount = req.Amount,
                        CurrencyUomId = createdPayment.CurrencyUomId ?? "EGP",
                        InstallmentCount = 1,                   // short-term → usually 1 installment
                        InstallmentAmount = req.Amount,         // full amount next month
                        StartDate = effectiveDate.AddMonths(1), // deduction starts next month
                        StatusId = "ADVANCE_APPROVED",         // or "ADVANCE_ACTIVE" — your choice
                        Description = req.Comments ?? "سلفة راتب قصيرة الأجل",
                        PaymentId = createdPayment.PaymentId,   // ← important link
                        CreatedStamp = DateTime.UtcNow,
                        CreatedTxStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow,
                        LastUpdatedTxStamp = DateTime.UtcNow
                    };

                    _context.EmployeeAdvances.Add(advance);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Results<CreatePaymentAndFinAccountTransResponse>.Success(paymentResult.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Results<CreatePaymentAndFinAccountTransResponse>.Failure(
                    $"حدث خطأ أثناء إنشاء الدفعة: {ex.Message}",
                    errorCode: "UNEXPECTED_PAYMENT_CREATION_ERROR"
                );
            }
        }
    }
}

public class CreatePaymentAndFinAccountTransRequestValidator : AbstractValidator<CreatePaymentAndFinAccountTransRequest>
{
    public CreatePaymentAndFinAccountTransRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.StatusId).NotEmpty();
    }
}