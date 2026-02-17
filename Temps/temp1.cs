using Application.Accounting.Payments;
using Application.Accounting.Services;
using Application.Catalog.ProductStores;
using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.HumanResources
{
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
                // Basic validations (common to both types)
                // ────────────────────────────────────────────────────────────────
                if (string.IsNullOrEmpty(dto.PartyId))
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        "معرف الموظف (PartyId) مطلوب للسلفة.",
                        errorCode: "EMPLOYEE_ID_REQUIRED");
                }

                var employeeId = dto.PartyId;

                // 1. Must have EMPLOYEE role
                var hasEmployeeRole = await _context.PartyRoles
                    .AnyAsync(pr => pr.PartyId == employeeId && pr.RoleTypeId == "EMPLOYEE", ct);
                if (!hasEmployeeRole)
                {
                    return Results<EmployeeAdvanceDto>.Failure(
                        "هذا الطرف ليس موظفًا مسجلاً (لا يوجد دور EMPLOYEE).",
                        "EMPLOYEE_ROLE_MISSING");
                }

                // 2. Must have active employment
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

                // 3. GL accounts linkage
                var glTypes = await _context.PartyGlAccounts
                    .Where(pga => pga.PartyId == employeeId
                               && pga.OrganizationPartyId == "Company"
                               && pga.RoleTypeId == "EMPLOYEE"
                               && (pga.GlAccountTypeId == "ACCOUNTS_RECEIVABLE" || pga.GlAccountTypeId == "ACCOUNTS_PAYABLE"))
                    .Select(pga => pga.GlAccountTypeId)
                    .Distinct()
                    .ToListAsync(ct);

                if (!glTypes.Contains("ACCOUNTS_RECEIVABLE") || !glTypes.Contains("ACCOUNTS_PAYABLE"))
                {
                    var missing = new List<string>();
                    if (!glTypes.Contains("ACCOUNTS_RECEIVABLE")) missing.Add("ACCOUNTS_RECEIVABLE");
                    if (!glTypes.Contains("ACCOUNTS_PAYABLE")) missing.Add("ACCOUNTS_PAYABLE");

                    return Results<EmployeeAdvanceDto>.Failure(
                        $"حساب/حسابات دفتر الأستاذ غير مرتبطة: {string.Join(" و ", missing)}.",
                        "GL_ACCOUNTS_NOT_LINKED");
                }

                // 4. Valid monthly salary exists
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
                        "لم يتم العثور على راتب شهري صالح.",
                        "INVALID_OR_MISSING_SALARY");
                }

                decimal monthlySalary = (decimal)salaryRecord;
                decimal maxAllowedShortTerm = monthlySalary * 0.50m;

                // 5. Short-term advance monthly limit
                if (dto.AdvanceTypeId == "EMPLOYEE_ADVANCE")
                {
                    var monthStart = new DateTime(dto.AdvanceDate.Year, dto.AdvanceDate.Month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    var previousAdvances = await _context.EmployeeAdvances
                        .Where(ea => ea.PartyId == employeeId
                                  && ea.AdvanceDate >= monthStart
                                  && ea.AdvanceDate <= monthEnd
                                  && ea.StatusId != "ADVANCE_CANCELLED"
                                  && ea.StatusId != "ADVANCE_REJECTED")
                        .SumAsync(ea => ea.Amount, ct);

                    decimal totalThisMonth = previousAdvances + (dto.Amount ?? 0);

                    if (totalThisMonth > maxAllowedShortTerm)
                    {
                        return Results<EmployeeAdvanceDto>.Failure(
                            $"المبلغ المطلوب + السلف الحالية في الشهر ({totalThisMonth:N2}) " +
                            $"يتجاوز الحد الأقصى (50% من الراتب = {maxAllowedShortTerm:N2}).",
                            "MONTHLY_ADVANCE_LIMIT_EXCEEDED");
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

                // ────────────────────────────────────────────────────────────────
                // Create payment record
                // ────────────────────────────────────────────────────────────────
                var companyPartyId = await _productStoreService.GetProductStorePayToPartId();

                var paymentParam = new CreatePaymentParam
                {
                    PartyIdFrom = companyPartyId,
                    PartyIdTo = dto.PartyId,
                    Amount = dto.Amount ?? 0,
                    EffectiveDate = dto.AdvanceDate,
                    PaymentTypeId = dto.AdvanceTypeId,
                    StatusId = "PMNT_NOT_PAID",
                    Comments = dto.Description,
                };

                var payment = await _paymentHelperService.CreatePayment(paymentParam);

                // ────────────────────────────────────────────────────────────────
                // Create main EmployeeAdvance entity
                // ────────────────────────────────────────────────────────────────
                var advanceId = await _utilityService.GetNextSequence("EmployeeAdvance");

                var advance = new EmployeeAdvance
                {
                    AdvanceId = advanceId,
                    PartyId = dto.PartyId,
                    PaymentId = payment.PaymentId,
                    AdvanceTypeId = dto.AdvanceTypeId,
                    AdvanceDate = dto.AdvanceDate,
                    Amount = dto.Amount ?? 0m,
                    StatusId = "ADVANCE_ACTIVE",
                    Description = dto.Description,
                    CreatedStamp = DateTime.UtcNow,
                    LastUpdatedStamp = DateTime.UtcNow,
                };

                // Handle legacy fields differently based on plan type
                if (isLongTerm && dto.CustomDeductionSchedules?.Any() == true)
                {
                    advance.InstallmentCount = dto.CustomDeductionSchedules.Count;
                    advance.StartDate = dto.CustomDeductionSchedules
                        .OrderBy(s => s.DueDate)
                        .FirstOrDefault()?.DueDate;

                    // InstallmentAmount = null or remove from entity in future
                    // advance.InstallmentAmount = null;
                }
                else
                {
                    advance.InstallmentCount = dto.InstallmentCount ?? 0;
                    advance.StartDate = dto.StartDate ?? DateTime.UtcNow;
                    // advance.InstallmentAmount = dto.InstallmentAmount;
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

                // ────────────────────────────────────────────────────────────────
                // Save everything in one transaction
                // ────────────────────────────────────────────────────────────────
                var success = await _context.SaveChangesAsync(ct) > 0;
                if (!success)
                {
                    return Results<EmployeeAdvanceDto>.Failure("فشل في حفظ السلفة.");
                }

                // ────────────────────────────────────────────────────────────────
                // Prepare result DTO
                // ────────────────────────────────────────────────────────────────
                var party = await _context.Parties
                    .FirstOrDefaultAsync(p => p.PartyId == dto.PartyId, ct);

                var status = await _context.StatusItems
                    .FirstOrDefaultAsync(s => s.StatusId == advance.StatusId, ct);

                var result = new EmployeeAdvanceDto
                {
                    AdvanceId = advance.AdvanceId,
                    PartyId = advance.PartyId,
                    EmployeeName = party?.Description ?? advance.PartyId,
                    PaymentId = advance.PaymentId,
                    AdvanceTypeId = advance.AdvanceTypeId,
                    AdvanceDate = advance.AdvanceDate,
                    Amount = advance.Amount,
                    InstallmentCount = advance.InstallmentCount,
                    StartDate = advance.StartDate,
                    StatusId = advance.StatusId,
                    StatusDescription = request.Language == "ar"
                        ? (status?.DescriptionArabic ?? status?.Description ?? advance.StatusId)
                        : (status?.Description ?? advance.StatusId),
                    Description = advance.Description,
                    // Optional: include schedules count or first few if you extend DTO
                };

                return Results<EmployeeAdvanceDto>.Success(result);
            }
        }
    }
}