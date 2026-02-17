using Application.Accounting.Payments;
using Application.Accounting.Services;
using Application.Catalog.ProductStores;
using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.HumanResources;

public class CreateEmployeeAdvance
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


        public Handler(DataContext context, IUtilityService utilityService, IProductStoreService productStoreService,
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
            // Validation block for employee advances
            // ────────────────────────────────────────────────────────────────
            if (string.IsNullOrEmpty(dto.PartyId))
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "معرف الموظف (PartyIdTo) مطلوب للسلفة.",
                    errorCode: "EMPLOYEE_ID_REQUIRED"
                );
            }

            var employeeId = dto.PartyId;

            // 1. EMPLOYEE role missing
            var hasEmployeeRole = await _context.PartyRoles
                .AnyAsync(pr =>
                    pr.PartyId == employeeId &&
                    pr.RoleTypeId == "EMPLOYEE", cancellationToken: ct);

            if (!hasEmployeeRole)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "هذا الطرف ليس موظفًا مسجلاً (لا يوجد دور EMPLOYEE). يرجى التأكد من إنشاء الموظف بشكل صحيح أولاً.",
                    errorCode: "EMPLOYEE_ROLE_MISSING"
                );
            }

            // 2. No active employment
            var hasActiveEmployment = await _context.Employments
                .AnyAsync(e =>
                        e.PartyIdTo == employeeId &&
                        e.FromDate <= DateTime.UtcNow &&
                        (e.ThruDate == null),
                    cancellationToken: ct);

            if (!hasActiveEmployment)
            {
                return Results<EmployeeAdvanceDto>.Failure(
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
                .ToListAsync(cancellationToken: ct);

            var hasReceivable = glTypes.Contains("ACCOUNTS_RECEIVABLE");
            var hasPayable = glTypes.Contains("ACCOUNTS_PAYABLE");

            if (!hasReceivable || !hasPayable)
            {
                var missing = new List<string>();
                if (!hasReceivable) missing.Add("ACCOUNTS_RECEIVABLE (ذمم مدينة)");
                if (!hasPayable) missing.Add("ACCOUNTS_PAYABLE (مستحقات)");

                return Results<EmployeeAdvanceDto>.Failure(
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
                .FirstOrDefaultAsync(cancellationToken: ct);

            if (salaryRecord <= 0)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "لم يتم العثور على راتب شهري صالح للموظف أو قيمته صفر. يرجى تسجيل الراتب الأساسي أولاً.",
                    errorCode: "INVALID_OR_MISSING_SALARY"
                );
            }

            decimal monthlySalary = (decimal)salaryRecord;
            decimal maxAllowedShortTerm = monthlySalary * 0.50m;


            // 5. Short-term advance: 50% limit exceeded
            // ── Only for short-term: check previous advances in current month ──
            if (dto.AdvanceTypeId == "EMPLOYEE_ADVANCE")
            {
                // Start and end of the month of effectiveDate
                var monthStart = new DateTime(dto.AdvanceDate.Year, dto.AdvanceDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var previousAdvances = await _context.EmployeeAdvances
                    .Where(ea =>
                        ea.PartyId == employeeId &&
                        ea.AdvanceDate >= monthStart &&
                        ea.AdvanceDate <= monthEnd &&
                        ea.StatusId != "ADVANCE_CANCELLED" && // exclude cancelled
                        ea.StatusId != "ADVANCE_REJECTED")
                    .SumAsync(ea => ea.Amount, cancellationToken: ct);

                decimal totalThisMonth = (decimal)(previousAdvances + dto.Amount);

                if (totalThisMonth > maxAllowedShortTerm)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        $"المبلغ المطلوب ({dto.Amount:N2}) + السلف الموجودة في الشهر الحالي ({previousAdvances:N2}) " +
                        $"يتجاوز الحد الأقصى للسلفة قصيرة الأجل (50% من الراتب = {maxAllowedShortTerm:N2}).",
                        "MONTHLY_ADVANCE_LIMIT_EXCEEDED"
                    );
                }
            }

            // ────────────────────────────────────────────────────────────────
            // Long-term advance → custom deduction plan validation
            // ────────────────────────────────────────────────────────────────
            bool isLongTerm = dto.AdvanceTypeId == "EMPLOYEE_LONG_TERM_ADVANCE";

            if (isLongTerm)
            {
                if (dto.CustomDeductionSchedules == null || !dto.CustomDeductionSchedules.Any())
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        "السلفة طويلة الأجل تتطلب جدول سداد (deduction plan).",
                        "LONG_TERM_REQUIRES_SCHEDULE");
                }

                var totalScheduled = dto.CustomDeductionSchedules.Sum(s => s.ScheduledAmount);
                if (Math.Abs(totalScheduled - (dto.Amount ?? 0)) > 0.01m)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        $"مجموع المبالغ المجدولة ({totalScheduled:N2}) لا يتطابق مع إجمالي السلفة ({dto.Amount:N2}).",
                        "SCHEDULE_TOTAL_MISMATCH");
                }

                // Optional: check dates are in future / ordered / no duplicates, etc.
                var hasPastDue = dto.CustomDeductionSchedules.Any(s => s.DueDate < DateTime.UtcNow.Date);
                if (hasPastDue)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        "لا يمكن جدولة خصم بتاريخ سابق.",
                        "PAST_DUE_DATE_NOT_ALLOWED");
                }
            }

            var companyPartyId = await _productStoreService.GetProductStorePayToPartId();
            CreatePaymentParam paymentsToCreate = new CreatePaymentParam
            {
                PartyIdFrom = companyPartyId,
                PartyIdTo = dto.PartyId,
                Amount = dto.Amount,
                EffectiveDate = dto.AdvanceDate,
                PaymentTypeId = dto.AdvanceTypeId,
                StatusId = "PMNT_NOT_PAID",
                Comments = dto.Description,
            };
            var payment = await _paymentHelperService.CreatePayment(paymentsToCreate);


            var advanceId = await _utilityService.GetNextSequence("EmployeeAdvance");

            var advance = new EmployeeAdvance
            {
                AdvanceId = advanceId,
                PartyId = dto.PartyId,
                PaymentId = payment.PaymentId,
                AdvanceTypeId = dto.AdvanceTypeId,
                AdvanceDate = dto.AdvanceDate,
                Amount = dto.Amount ?? 0,
                InstallmentCount = dto.InstallmentCount ?? 0,
                StartDate = dto.StartDate ?? DateTime.Now,
                StatusId = "ADVANCE_ACTIVE",
                Description = dto.Description,
                CreatedStamp = DateTime.Now,
                LastUpdatedStamp = DateTime.Now
            };

            if (isLongTerm && dto.CustomDeductionSchedules?.Any() == true)
            {
                advance.InstallmentCount = dto.CustomDeductionSchedules.Count;
                advance.StartDate = dto.CustomDeductionSchedules
                    .OrderBy(s => s.DueDate)
                    .FirstOrDefault()?.DueDate;
            }
            else
            {
                advance.InstallmentCount = dto.InstallmentCount ?? 0;
                advance.StartDate = dto.StartDate ?? DateTime.UtcNow;
            }

            _context.EmployeeAdvances.Add(advance);

            // ────────────────────────────────────────────────────────────────
            // Create schedule records (only for long-term)
            // ────────────────────────────────────────────────────────────────
            if (isLongTerm && dto.CustomDeductionSchedules?.Any() == true)
            {
                int installmentNumber = 1;
                foreach (var schedDto in dto.CustomDeductionSchedules.OrderBy(s => s.DueDate))
                {
                    var schedule = new EmployeeAdvanceSchedule
                    {
                        ScheduleId = Guid.NewGuid().ToString(), // or use sequence if preferred
                        AdvanceId = advanceId,
                        InstallmentNumber = installmentNumber++,
                        DueDate = schedDto.DueDate,
                        ScheduledAmount = schedDto.ScheduledAmount,
                        DeductedAmount = 0m,
                        StatusId = "SCHEDULED",
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow,
                    };

                    _context.EmployeeAdvanceSchedules.Add(schedule);
                }
            }


            var success = await _context.SaveChangesAsync(ct) > 0;

            if (!success) return Results<EmployeeAdvanceDto>.Failure("Failed to create employee advance");

            // Return the record for UI
            var party = await _context.Parties.FirstOrDefaultAsync(p => p.PartyId == dto.PartyId, ct);
            var status = await _context.StatusItems
                .FirstOrDefaultAsync(s => s.StatusId == advance.StatusId, ct);

            var resultRecord = new EmployeeAdvanceDto
            {
                AdvanceId = advance.AdvanceId,
                PartyId = advance.PartyId,
                PaymentId = advance.PaymentId,
                EmployeeName = party?.Description ?? advance.PartyId,
                AdvanceDate = advance.AdvanceDate,
                Amount = advance.Amount,
                StartDate = advance.StartDate,
                StatusId = advance.StatusId,
                StatusDescription = request.Language == "ar"
                    ? (status?.DescriptionArabic ?? status?.Description ?? advance.StatusId)
                    : (status?.Description ?? advance.StatusId),
                Description = advance.Description
            };

            return Results<EmployeeAdvanceDto>.Success(resultRecord);
        }
    }
}