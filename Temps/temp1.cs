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

            // 1. Load existing advance with schedules
            var advance = await _context.EmployeeAdvances
                .Include(a => a.EmployeeAdvanceSchedules)
                .FirstOrDefaultAsync(x => x.AdvanceId == dto.AdvanceId, ct);

            if (advance == null)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    $"No employee advance found with ID {dto.AdvanceId}.",
                    "ADVANCE_NOT_FOUND");
            }

            // 2. Prevent modification of closed/final states
            if (advance.StatusId is "ADVANCE_PAID" or "ADVANCE_CANCELLED" or "ADVANCE_REJECTED")
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "Cannot modify a paid, cancelled or rejected advance.",
                    "ADVANCE_CLOSED");
            }

            var isLongTerm = dto.AdvanceTypeId == "EMPLOYEE_LONG_TERM_ADVANCE";
            var wasLongTerm = advance.AdvanceTypeId == "EMPLOYEE_LONG_TERM_ADVANCE";

            // 3. Core validations
            if (string.IsNullOrEmpty(dto.PartyId))
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "معرف الموظف (PartyId) مطلوب.", "EMPLOYEE_ID_REQUIRED");
            }

            var employeeId = dto.PartyId;

            // Employee role check
            if (!await _context.PartyRoles.AnyAsync(pr => 
                    pr.PartyId == employeeId && pr.RoleTypeId == "EMPLOYEE", ct))
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "هذا الطرف ليس موظفًا مسجلاً (دور EMPLOYEE غير موجود).", 
                    "EMPLOYEE_ROLE_MISSING");
            }

            // Active employment check
            if (!await _context.Employments.AnyAsync(e => 
                    e.PartyIdTo == employeeId 
                    && e.FromDate <= DateHelper.Today 
                    && (e.ThruDate == null || e.ThruDate > DateHelper.Today), ct))
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "لا يوجد سجل توظيف نشط لهذا الموظف.", "NO_ACTIVE_EMPLOYMENT");
            }

            // GL accounts check
            var glTypes = await _context.PartyGlAccounts
                .Where(pga => pga.PartyId == employeeId 
                           && pga.OrganizationPartyId == "Company"
                           && pga.RoleTypeId == "EMPLOYEE")
                .Select(pga => pga.GlAccountTypeId)
                .Distinct()
                .ToListAsync(ct);

            if (!glTypes.Contains("ACCOUNTS_RECEIVABLE") || !glTypes.Contains("ACCOUNTS_PAYABLE"))
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "يجب ربط حساب ذمم مدينة وحساب مستحقات بالموظف أولاً.", 
                    "GL_ACCOUNTS_NOT_LINKED");
            }

            // Salary check
            var monthlySalary = await _context.RateAmounts
                .Where(ra => ra.PartyId == employeeId 
                          && ra.PeriodTypeId == "RATE_MONTH"
                          && ra.FromDate <= DateHelper.Today
                          && (ra.ThruDate == null || ra.ThruDate > DateHelper.Today))
                .OrderByDescending(ra => ra.FromDate)
                .Select(ra => ra.Amount)
                .FirstOrDefaultAsync(ct);

            if (monthlySalary <= 0)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "لم يتم العثور على راتب شهري صالح.", "INVALID_OR_MISSING_SALARY");
            }

            decimal maxAllowedShortTerm = (decimal)monthlySalary * 0.50m;

            // Amount validation
            if ((dto.Amount ?? 0) <= 0)
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "المبلغ يجب أن يكون أكبر من صفر.", "INVALID_AMOUNT");
            }

            // ────────────────────────────────────────────────────────────────
            // Short-term: Monthly limit check (excluding current advance)
            // ────────────────────────────────────────────────────────────────
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
                        $"تجاوز الحد الشهري للسلف القصيرة الأجل (الحد الأقصى: {maxAllowedShortTerm:N2}).",
                        "MONTHLY_ADVANCE_LIMIT_EXCEEDED");
                }
            }

            // ────────────────────────────────────────────────────────────────
            // Long-term validations
            // ────────────────────────────────────────────────────────────────
            if (isLongTerm)
            {
                if (dto.CustomDeductionSchedules == null || !dto.CustomDeductionSchedules.Any())
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        "السلفة طويلة الأجل تتطلب جدول سداد.", "LONG_TERM_REQUIRES_SCHEDULE");
                }

                var totalScheduled = dto.CustomDeductionSchedules.Sum(s => s.ScheduledAmount);
                if (Math.Abs(totalScheduled - (dto.Amount ?? 0)) > 0.01m)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        $"مجموع المبالغ المجدولة لا يتطابق مع قيمة السلفة.", "SCHEDULE_TOTAL_MISMATCH");
                }

                if (dto.CustomDeductionSchedules.Any(s => s.DueDate < DateHelper.Today))
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        "لا يمكن جدولة خصم بتاريخ سابق.", "PAST_DUE_DATE_NOT_ALLOWED");
                }
            }

            // Prevent increasing amount after deductions started
            if ((dto.Amount ?? 0) > advance.Amount && 
                advance.EmployeeAdvanceSchedules.Any(s => s.DeductedAmount > 0))
            {
                return Results<EmployeeAdvanceDto>.Failure(
                    "لا يمكن زيادة المبلغ بعد بدء الخصومات.", "AMOUNT_INCREASE_AFTER_DEDUCTION");
            }

            // ────────────────────────────────────────────────────────────────
            // 5. Apply changes
            // ────────────────────────────────────────────────────────────────
            var companyPartyId = await _productStoreService.GetProductStorePayToPartId();

            // Update Payment
            var paymentParam = new CreatePaymentParam
            {
                PaymentId = advance.PaymentId,
                PartyIdFrom = companyPartyId,
                PartyIdTo = dto.PartyId,
                Amount = dto.Amount ?? 0,
                EffectiveDate = dto.AdvanceDate ?? DateHelper.Today,
                PaymentTypeId = dto.AdvanceTypeId,
                StatusId = "PMNT_NOT_PAID",
                Comments = dto.Description,
            };

            await _paymentHelperService.UpdatePayment(paymentParam);

            // Update Advance
            advance.PartyId = dto.PartyId;
            advance.AdvanceDate = dto.AdvanceDate ?? DateHelper.Today;
            advance.Amount = dto.Amount ?? 0;
            advance.AdvanceTypeId = dto.AdvanceTypeId;
            advance.Description = dto.Description ?? advance.Description;
            advance.LastUpdatedStamp = DateTime.UtcNow;

            // Handle schedules for long-term
            if (isLongTerm && dto.CustomDeductionSchedules?.Any() == true)
            {
                // Remove old schedules
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
                        DeductedAmount = 0m,
                        StatusId = "SCHEDULED",
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow,
                    };
                    _context.EmployeeAdvanceSchedules.Add(schedule);
                }

                advance.InstallmentCount = dto.CustomDeductionSchedules.Count;
                advance.StartDate = dto.CustomDeductionSchedules.Min(s => s.DueDate);
            }
            else if (wasLongTerm && !isLongTerm)
            {
                // Switching from long-term to short-term → clear schedules
                _context.EmployeeAdvanceSchedules.RemoveRange(advance.EmployeeAdvanceSchedules);
                advance.InstallmentCount = dto.InstallmentCount ?? 0;
                advance.StartDate = dto.StartDate;
            }
            else
            {
                advance.InstallmentCount = dto.InstallmentCount ?? advance.InstallmentCount;
                advance.StartDate = dto.StartDate;
            }

            await _context.SaveChangesAsync(ct);

            // Return updated record
            var party = await _context.Parties.FirstOrDefaultAsync(p => p.PartyId == advance.PartyId, ct);
            var status = await _context.StatusItems.FirstOrDefaultAsync(s => s.StatusId == advance.StatusId, ct);

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
            };

            return Results<EmployeeAdvanceDto>.Success(resultRecord);
        }
    }
}

dotnet ef migrations add Convert_BusinessDates_To_DateOnly  -p Persistence -s API