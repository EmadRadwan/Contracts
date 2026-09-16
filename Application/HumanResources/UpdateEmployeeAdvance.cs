using Application.Accounting.Payments;
using Application.Accounting.Services;
using Application.Catalog.ProductStores;
using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.HumanResources;

public class UpdateEmployeeAdvance
{
    public class Command : IRequest<Results<EmployeeAdvanceDto>>
    {
        public EmployeeAdvanceDto AdvanceDto { get; set; } = null!;
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Command, Results<EmployeeAdvanceDto>>
    {
        private readonly DataContext _context;
        private readonly IProductStoreService _productStoreService;
        private readonly IPaymentHelperService _paymentHelperService;

        public Handler(
            DataContext context,
            IProductStoreService productStoreService,
            IPaymentHelperService paymentHelperService)
        {
            _context = context;
            _productStoreService = productStoreService;
            _paymentHelperService = paymentHelperService;
        }

        public async Task<Results<EmployeeAdvanceDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.AdvanceDto;

            // ────────────────────────────────────────────────────────────────
            // 1. Load existing advance + schedules
            // ────────────────────────────────────────────────────────────────
            var advance = await _context.EmployeeAdvances
                .Include(a => a.EmployeeAdvanceSchedules)
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(x => x.AdvanceId == dto.AdvanceId, ct);

            if (advance == null)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    $"No employee advance found with ID {dto.AdvanceId}.",
                    "ADVANCE_NOT_FOUND");
            }

            // ────────────────────────────────────────────────────────────────
            // 2. Prevent modification of closed/final states or processed parts
            // ────────────────────────────────────────────────────────────────
            if (advance.StatusId is "ADVANCE_FULLY_PAID" or "ADVANCE_CANCELLED" or "ADVANCE_REJECTED")
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "Cannot modify a closed, fully paid, cancelled or rejected advance.",
                    "ADVANCE_CLOSED");
            }

            // A schedule row is "processed" once a payroll run has deducted it. The run sets
            // PayrolInvoiceId + "PAID" (BatchCreatePayrollInvoices); the invoice READY hook may also
            // stamp "SCHED_PAID" (ApplyPayrollDeductionsToAdvances). Those rows are facts, never input.
            var processedSchedules = advance.EmployeeAdvanceSchedules
                .Where(EmployeeAdvanceScheduleRules.IsProcessed)
                .ToList();
            var pendingDbSchedules = advance.EmployeeAdvanceSchedules
                .Where(s => !EmployeeAdvanceScheduleRules.IsProcessed(s))
                .ToList();

            var isProcessedInPayroll = advance.PayrollInvoiceId != null || processedSchedules.Any();

            // Once the disbursement payment has left PMNT_NOT_PAID it is posted to the GL
            // (CreateAccountingTransactionForEmployeeAdvance). Changing the money fields here would
            // silently desync the ledger, so lock them; the deduction plan stays editable.
            var isPaymentSent = advance.Payment != null && advance.Payment.StatusId != "PMNT_NOT_PAID";
            if (isPaymentSent)
            {
                var moneyFieldChanged =
                    (dto.Amount ?? advance.Amount) != advance.Amount
                    || dto.AdvanceDate != advance.AdvanceDate
                    || dto.PartyId != advance.PartyId
                    || dto.AdvanceTypeId != advance.AdvanceTypeId;

                if (moneyFieldChanged)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        "تم صرف السلفة بالفعل؛ لا يمكن تعديل المبلغ أو التاريخ أو الموظف أو النوع. يمكن تعديل جدول الخصم والوصف فقط.",
                        "ADVANCE_PAYMENT_SENT");
                }
            }

            if (advance.AdvanceTypeId == "EMPLOYEE_ADVANCE" && advance.PayrollInvoiceId != null)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "This advance has been processed in a payroll run and cannot be edited.",
                    "ADVANCE_PROCESSED");
            }
            
            if (isProcessedInPayroll && dto.PartyId != advance.PartyId)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "Cannot change the employee because this advance has been partially or fully processed in a payroll run.",
                    "CANNOT_CHANGE_EMPLOYEE");
            }

            if (isProcessedInPayroll && dto.AdvanceTypeId != advance.AdvanceTypeId)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "Cannot change the advance type because it has been partially or fully processed in a payroll run.",
                    "CANNOT_CHANGE_TYPE");
            }

            var isLongTerm = dto.AdvanceTypeId == "EMPLOYEE_LONG_TERM_ADVANCE";
            var wasLongTerm = advance.AdvanceTypeId == "EMPLOYEE_LONG_TERM_ADVANCE";

            // ────────────────────────────────────────────────────────────────
            // 3. Core validations — same spirit as Create
            // ────────────────────────────────────────────────────────────────

            // 3.1 Employee must exist and have EMPLOYEE role
            if (string.IsNullOrEmpty(dto.PartyId))
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "معرف الموظف (PartyId) مطلوب.",
                    "EMPLOYEE_ID_REQUIRED");
            }

            var employeeId = dto.PartyId;

            var hasEmployeeRole = await _context.PartyRoles
                .AnyAsync(pr => pr.PartyId == employeeId && pr.RoleTypeId == "EMPLOYEE", ct);

            if (!hasEmployeeRole)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "هذا الطرف ليس موظفًا مسجلاً (لا يوجد دور EMPLOYEE).",
                    "EMPLOYEE_ROLE_MISSING");
            }

            // 3.2 Must have active employment
            var hasActiveEmployment = await _context.Employments
                .AnyAsync(e => e.PartyIdTo == employeeId
                               && e.FromDate <= DateTime.UtcNow
                               && (e.ThruDate == null || e.ThruDate > DateTime.UtcNow), ct);

            if (!hasActiveEmployment)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "لا يوجد سجل توظيف نشط لهذا الموظف.",
                    "NO_ACTIVE_EMPLOYMENT");
            }

            // 3.3 GL accounts linkage (AR & AP)
            var glTypes = await _context.PartyGlAccounts
                .Where(pga => pga.PartyId == employeeId
                              && pga.OrganizationPartyId == "Company"
                              && pga.RoleTypeId == "EMPLOYEE"
                              && (pga.GlAccountTypeId == "ACCOUNTS_RECEIVABLE" ||
                                  pga.GlAccountTypeId == "ACCOUNTS_PAYABLE"))
                .Select(pga => pga.GlAccountTypeId)
                .Distinct()
                .ToListAsync(ct);

            var hasReceivable = glTypes.Contains("ACCOUNTS_RECEIVABLE");
            var hasPayable = glTypes.Contains("ACCOUNTS_PAYABLE");

            if (!hasReceivable || !hasPayable)
            {
                var missing = new List<string>();
                if (!hasReceivable) missing.Add("ACCOUNTS_RECEIVABLE");
                if (!hasPayable) missing.Add("ACCOUNTS_PAYABLE");

                return Results<EmployeeAdvanceDto>.Failure(
                    $"حساب/حسابات دفتر الأستاذ غير مرتبطة بالموظف: {string.Join(" و ", missing)}.",
                    "GL_ACCOUNTS_NOT_LINKED");
            }

            // 3.4 Valid monthly salary exists
            var salaryRecord = await _context.RateAmounts
                .Where(ra => ra.PartyId == employeeId
                             && ra.PeriodTypeId == "RATE_MONTH"
                             && ra.FromDate <= DateTime.UtcNow
                             && (ra.ThruDate == null || ra.ThruDate > DateTime.UtcNow))
                .OrderByDescending(ra => ra.FromDate)
                .Select(ra => ra.Amount)
                .FirstOrDefaultAsync(ct);

            if (salaryRecord <= 0)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "لم يتم العثور على راتب شهري صالح أو قيمته صفر.",
                    "INVALID_OR_MISSING_SALARY");
            }

            decimal monthlySalary = (decimal)salaryRecord;
            decimal maxAllowedShortTerm = monthlySalary * 0.50m;

            // ────────────────────────────────────────────────────────────────
            // 4. Type-specific validations
            // ────────────────────────────────────────────────────────────────

            // Amount must be positive
            if (dto.Amount <= 0)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "المبلغ يجب أن يكون أكبر من صفر.",
                    "INVALID_AMOUNT");
            }

            // Short-term: monthly limit (considering existing + new amount)
            if (dto.AdvanceTypeId == "EMPLOYEE_ADVANCE" && dto.AdvanceDate.HasValue)
            {
                var advanceDate = dto.AdvanceDate.Value;
                var monthStart = new DateOnly(advanceDate.Year, advanceDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var previousAdvancesThisMonth = await _context.EmployeeAdvances
                    .Where(ea => ea.PartyId == employeeId
                                 && ea.AdvanceDate >= monthStart
                                 && ea.AdvanceDate <= monthEnd
                                 && ea.AdvanceId != advance.AdvanceId
                                 && ea.StatusId != "ADVANCE_CANCELLED"
                                 && ea.StatusId != "ADVANCE_REJECTED")
                    .SumAsync(ea => ea.Amount, ct);

                var totalThisMonth = previousAdvancesThisMonth + (dto.Amount ?? 0);

                if (totalThisMonth > maxAllowedShortTerm)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        $"المبلغ المطلوب + السلف الموجودة في الشهر الحالي يتجاوز الحد الأقصى (50% من الراتب = {maxAllowedShortTerm:N2}).",
                        "MONTHLY_ADVANCE_LIMIT_EXCEEDED");
                }
            }

            // Long-term: require and validate schedule if type is long-term.
            // The form echoes already-deducted rows back with their PayrollInvoiceId; those are
            // ignored as input — the DB copy is authoritative. Only the pending rows are validated.
            List<EmployeeAdvanceScheduleDto> pendingDtoSchedules = new();
            if (isLongTerm)
            {
                pendingDtoSchedules = (dto.CustomDeductionSchedules ?? new List<EmployeeAdvanceScheduleDto>())
                    .Where(s => string.IsNullOrEmpty(s.PayrollInvoiceId))
                    .ToList();

                if (!pendingDtoSchedules.Any() && !processedSchedules.Any())
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        "السلفة طويلة الأجل تتطلب جدول سداد.",
                        "LONG_TERM_REQUIRES_SCHEDULE");
                }

                if (pendingDtoSchedules.Any(s => !s.DueDate.HasValue))
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        "كل قسط في جدول الخصم يجب أن يكون له تاريخ استحقاق.",
                        "SCHEDULE_DUE_DATE_REQUIRED");
                }

                var totalScheduled = processedSchedules.Sum(s => s.DeductedAmount > 0 ? s.DeductedAmount : s.ScheduledAmount)
                                     + pendingDtoSchedules.Sum(s => s.ScheduledAmount);
                if (Math.Abs(totalScheduled - (dto.Amount ?? 0)) > 0.01m)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        $"مجموع المبالغ المجدولة ({totalScheduled:N2}) لا يتطابق مع إجمالي السلفة ({dto.Amount:N2}).",
                        "SCHEDULE_TOTAL_MISMATCH");
                }

                // The payroll run (and PayrollRun.tsx) deduct exactly one installment per month, so a
                // second row in the same month would silently never be collected.
                var monthKeys = processedSchedules.Where(s => s.DueDate.HasValue).Select(s => EmployeeAdvanceScheduleRules.MonthKey(s.DueDate!.Value))
                    .Concat(pendingDtoSchedules.Select(s => EmployeeAdvanceScheduleRules.MonthKey(s.DueDate!.Value)))
                    .ToList();
                var duplicateMonth = monthKeys.GroupBy(k => k).FirstOrDefault(g => g.Count() > 1);
                if (duplicateMonth != null)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        $"لا يمكن جدولة أكثر من قسط واحد في نفس الشهر ({duplicateMonth.Key:MM/yyyy}).",
                        "DUPLICATE_MONTH_INSTALLMENT");
                }

                // Every pending row must sit in a month a future payroll run can still collect:
                // after the advance date, after the employee's latest run, after the last deduction.
                var earliestMonth = await EmployeeAdvanceScheduleRules.EarliestSchedulableMonthAsync(
                    _context, employeeId, dto.AdvanceDate, processedSchedules, ct);

                var tooEarly = pendingDtoSchedules
                    .OrderBy(s => s.DueDate)
                    .FirstOrDefault(s => EmployeeAdvanceScheduleRules.MonthKey(s.DueDate!.Value) < earliestMonth);
                if (tooEarly != null)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        EmployeeAdvanceScheduleRules.MonthNotSchedulableMessage(tooEarly.DueDate!.Value, earliestMonth),
                        "PAST_DUE_DATE_NOT_ALLOWED");
                }
            }

            // Prevent increasing amount if deductions already started
            var totalDeducted = advance.EmployeeAdvanceSchedules.Sum(s => s.DeductedAmount);
            if (dto.Amount < totalDeducted)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    $"المبلغ الجديد ({dto.Amount:N2}) لا يمكن أن يكون أقل من المبالغ التي تم خصمها بالفعل ({totalDeducted:N2}).",
                    "AMOUNT_LESS_THAN_DEDUCTED");
            }

            // ────────────────────────────────────────────────────────────────
            // 5. Apply changes
            // ────────────────────────────────────────────────────────────────
            if (advance.Payment != null)
            {
                if (!isPaymentSent)
                {
                    var companyPartyId = await _productStoreService.GetProductStorePayToPartId();
                    CreatePaymentParam paymentsToUpdate = new CreatePaymentParam
                    {
                        PaymentId = advance.PaymentId,
                        PartyIdFrom = companyPartyId,
                        PartyIdTo = dto.PartyId,
                        Amount = dto.Amount,
                        EffectiveDate = dto.AdvanceDate,
                        PaymentTypeId = dto.AdvanceTypeId,
                        StatusId = "PMNT_NOT_PAID",
                        Comments = dto.Description,
                    };
                    await _paymentHelperService.UpdatePayment(paymentsToUpdate);
                }
                else if (dto.Description != null)
                {
                    // Posted payment: money fields are locked (checked above) and UpdatePayment would
                    // null the cheque/ref fields, so only keep the narrative in sync.
                    advance.Payment.Comments = dto.Description;
                    advance.Payment.LastUpdatedStamp = DateTime.UtcNow;
                }
            }

            advance.PartyId = dto.PartyId;
            advance.AdvanceDate = dto.AdvanceDate;
            advance.Amount = (decimal)dto.Amount;
            advance.AdvanceTypeId = dto.AdvanceTypeId;
            advance.Description = dto.Description ?? advance.Description;
            advance.LastUpdatedStamp = DateTime.UtcNow;

            if (isLongTerm)
            {
                // Rebuild the plan: keep processed rows as-is, replace every pending row with the
                // submitted pending rows, then renumber the whole plan by due date.
                _context.EmployeeAdvanceSchedules.RemoveRange(pendingDbSchedules);

                var allFinalSchedules = new List<EmployeeAdvanceSchedule>(processedSchedules);
                foreach (var schedDto in pendingDtoSchedules.OrderBy(s => s.DueDate))
                {
                    allFinalSchedules.Add(new EmployeeAdvanceSchedule
                    {
                        ScheduleId = Guid.NewGuid().ToString(),
                        AdvanceId = advance.AdvanceId,
                        InstallmentNumber = 0, // Fixed below
                        DueDate = schedDto.DueDate,
                        ScheduledAmount = schedDto.ScheduledAmount,
                        DeductedAmount = 0m,
                        StatusId = "SCHEDULED",
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow,
                    });
                }

                // Re-assign installment numbers based on DueDate
                int seq = 1;
                foreach (var s in allFinalSchedules.OrderBy(x => x.DueDate))
                {
                    s.InstallmentNumber = seq++;
                    if (_context.Entry(s).State == EntityState.Detached)
                    {
                        _context.EmployeeAdvanceSchedules.Add(s);
                    }
                }

                advance.InstallmentCount = allFinalSchedules.Count;
                advance.StartDate = allFinalSchedules.Any() ? allFinalSchedules.Min(s => s.DueDate) : advance.StartDate;
            }
            else
            {
                advance.InstallmentCount = dto.InstallmentCount ?? advance.InstallmentCount;
                advance.StartDate = dto.StartDate;

                // If changing from long-term to short-term → remove old schedules
                if (wasLongTerm)
                {
                    _context.EmployeeAdvanceSchedules.RemoveRange(advance.EmployeeAdvanceSchedules);
                }
            }

            // ────────────────────────────────────────────────────────────────
            // 6. Save & return updated record
            // ────────────────────────────────────────────────────────────────
            var success = await _context.SaveChangesAsync(ct) > 0;
            if (!success)
            {
                return Results<EmployeeAdvanceDto>.Failure("فشل تحديث السلفة.");
            }

            // Re-fetch related data for response
            var party = await _context.Parties
                .FirstOrDefaultAsync(p => p.PartyId == advance.PartyId, ct);

            var status = await _context.StatusItems
                .FirstOrDefaultAsync(s => s.StatusId == advance.StatusId, ct);

            var resultRecord = new EmployeeAdvanceDto
            {
                AdvanceId = advance.AdvanceId,
                PartyId = advance.PartyId,
                EmployeeName = party?.Description ?? advance.PartyId,
                PaymentId = advance.PaymentId,
                AdvanceDate = advance.AdvanceDate,
                Amount = advance.Amount,
                InstallmentCount = advance.InstallmentCount,
                StartDate = advance.StartDate,
                StatusId = advance.StatusId,
                StatusDescription = request.Language == "ar"
                    ? (status?.DescriptionArabic ?? status?.Description ?? advance.StatusId)
                    : (status?.Description ?? advance.StatusId),
                Description = advance.Description,
                AdvanceTypeId = advance.AdvanceTypeId,
                AdvanceTypeDescription = advance.AdvanceTypeId == "EMPLOYEE_ADVANCE"
                    ? "سلفة راتب"
                    : "سلفة طويلة الأجل",
            };

            return Results<EmployeeAdvanceDto>.Success(resultRecord);
        }
    }
}