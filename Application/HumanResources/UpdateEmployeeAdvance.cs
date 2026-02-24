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
        private readonly IUtilityService _utilityService;
        private readonly IProductStoreService _productStoreService;
        private readonly IPaymentHelperService _paymentHelperService;

        public Handler(
            DataContext context,
            IUtilityService utilityService,
            IProductStoreService productStoreService,
            IPaymentHelperService paymentHelperService)
        {
            _context = context;
            _utilityService = utilityService;
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
                .FirstOrDefaultAsync(x => x.AdvanceId == dto.AdvanceId, ct);

            if (advance == null)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    $"No employee advance found with ID {dto.AdvanceId}.",
                    "ADVANCE_NOT_FOUND");
            }

            // ────────────────────────────────────────────────────────────────
            // 2. Prevent modification of closed/final states
            // ────────────────────────────────────────────────────────────────
            if (advance.StatusId is "ADVANCE_PAID" or "ADVANCE_CANCELLED" or "ADVANCE_REJECTED")
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "Cannot modify a paid, cancelled or rejected advance.",
                    "ADVANCE_CLOSED");
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
            if (dto.AdvanceTypeId == "EMPLOYEE_ADVANCE")
            {
                var monthStart = new DateTime(dto.AdvanceDate.Year, dto.AdvanceDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                // Exclude current advance when recalculating total this month
                var previousAdvancesThisMonth = await _context.EmployeeAdvances
                    .Where(ea => ea.PartyId == employeeId
                                 && ea.AdvanceDate >= monthStart
                                 && ea.AdvanceDate <= monthEnd
                                 && ea.AdvanceId != advance.AdvanceId
                                 && ea.StatusId != "ADVANCE_CANCELLED"
                                 && ea.StatusId != "ADVANCE_REJECTED")
                    .SumAsync(ea => ea.Amount, ct);

                var totalThisMonth = previousAdvancesThisMonth + dto.Amount;

                if (totalThisMonth > maxAllowedShortTerm)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        $"المبلغ المطلوب + السلف الموجودة في الشهر الحالي يتجاوز الحد الأقصى (50% من الراتب = {maxAllowedShortTerm:N2}).",
                        "MONTHLY_ADVANCE_LIMIT_EXCEEDED");
                }
            }

            // Long-term: require and validate schedule if type is long-term
            if (isLongTerm)
            {
                if (dto.CustomDeductionSchedules == null || !dto.CustomDeductionSchedules.Any())
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        "السلفة طويلة الأجل تتطلب جدول سداد.",
                        "LONG_TERM_REQUIRES_SCHEDULE");
                }

                var totalScheduled = dto.CustomDeductionSchedules.Sum(s => s.ScheduledAmount);
                if (Math.Abs((decimal)(totalScheduled - dto.Amount)) > 0.01m)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        $"مجموع المبالغ المجدولة ({totalScheduled:N2}) لا يتطابق مع إجمالي السلفة ({dto.Amount:N2}).",
                        "SCHEDULE_TOTAL_MISMATCH");
                }

                if (dto.CustomDeductionSchedules.Any(s => s.DueDate < DateTime.UtcNow.Date))
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        "لا يمكن جدولة خصم بتاريخ سابق.",
                        "PAST_DUE_DATE_NOT_ALLOWED");
                }
            }
            else if (wasLongTerm && !isLongTerm)
            {
                // Changing from long-term → short-term: warn / require confirmation?
                // For now: allow, but clear schedules
            }

            // Prevent increasing amount if deductions already started
            if (dto.Amount > advance.Amount && advance.EmployeeAdvanceSchedules.Any(s => s.DeductedAmount > 0))
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "لا يمكن زيادة المبلغ بعد بدء الخصومات.",
                    "AMOUNT_INCREASE_AFTER_DEDUCTION");
            }

            // ────────────────────────────────────────────────────────────────
            // 5. Apply changes
            // ────────────────────────────────────────────────────────────────
            advance.PartyId = dto.PartyId;
            advance.AdvanceDate = dto.AdvanceDate;
            advance.Amount = (decimal)dto.Amount;
            advance.AdvanceTypeId = dto.AdvanceTypeId;
            advance.Description = dto.Description ?? advance.Description;
            advance.LastUpdatedStamp = DateTime.UtcNow;

            if (isLongTerm)
            {
                // Replace schedules if new ones provided
                if (dto.CustomDeductionSchedules?.Any() == true)
                {
                    _context.EmployeeAdvanceSchedules.RemoveRange(advance.EmployeeAdvanceSchedules);

                    int installmentNumber = 1;
                    foreach (var sched in dto.CustomDeductionSchedules.OrderBy(s => s.DueDate))
                    {
                        var schedule = new EmployeeAdvanceSchedule
                        {
                            ScheduleId = Guid.NewGuid().ToString(),
                            AdvanceId = advance.AdvanceId,
                            InstallmentNumber = installmentNumber++,
                            DueDate = sched.DueDate,
                            ScheduledAmount = sched.ScheduledAmount,
                            DeductedAmount = 0m, // full reset — adjust if partial updates needed
                            StatusId = "SCHEDULED",
                            CreatedStamp = DateTime.UtcNow,
                            LastUpdatedStamp = DateTime.UtcNow,
                        };
                        _context.EmployeeAdvanceSchedules.Add(schedule);
                    }

                    advance.InstallmentCount = dto.CustomDeductionSchedules.Count;
                    advance.StartDate = dto.CustomDeductionSchedules.Min(s => s.DueDate);
                }
                // If no new schedule → keep existing (but amount change without schedule is blocked above)
            }
            else
            {
                advance.InstallmentCount = dto.InstallmentCount ?? advance.InstallmentCount;
                advance.StartDate = dto.StartDate ?? advance.StartDate;

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