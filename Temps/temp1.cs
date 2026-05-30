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

        public Handler(DataContext context, IProductStoreService productStoreService, IPaymentHelperService paymentHelperService)
        {
            _context = context;
            _productStoreService = productStoreService;
            _paymentHelperService = paymentHelperService;
        }

        public async Task<Results<EmployeeAdvanceDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.AdvanceDto;
            var advance = await _context.EmployeeAdvances
                .Include(a => a.EmployeeAdvanceSchedules)
                .FirstOrDefaultAsync(x => x.AdvanceId == dto.AdvanceId, ct);

            if (advance == null)
                return Results<EmployeeAdvanceDto>.Failure("Advance not found", "ADVANCE_NOT_FOUND");

            // Block updates on closed statuses
            if (advance.StatusId is "ADVANCE_FULLY_PAID" or "ADVANCE_CANCELLED" or "ADVANCE_REJECTED")
                return Results<EmployeeAdvanceDto>.Failure("Cannot modify a closed advance", "ADVANCE_CLOSED");

            bool isLongTerm = dto.AdvanceTypeId == "EMPLOYEE_LONG_TERM_ADVANCE";
            bool wasLongTerm = advance.AdvanceTypeId == "EMPLOYEE_LONG_TERM_ADVANCE";

            // Core validations (you can keep your full validation block here)
            if (dto.Amount <= 0)
                return Results<EmployeeAdvanceDto>.Failure("Amount must be greater than zero");

            decimal alreadyDeducted = advance.EmployeeAdvanceSchedules.Sum(s => s.DeductedAmount);
            if (dto.Amount < alreadyDeducted)
                return Results<EmployeeAdvanceDto>.Failure($"Cannot reduce amount below already deducted amount ({alreadyDeducted:N2})");

            // ──────────────────────────────────────
            // Long-term Schedule Update Logic
            // ──────────────────────────────────────
            if (isLongTerm)
            {
                if (dto.CustomDeductionSchedules?.Any() != true)
                    return Results<EmployeeAdvanceDto>.Failure("Long-term advance requires a deduction plan");

                var totalScheduled = dto.CustomDeductionSchedules.Sum(s => s.ScheduledAmount);
                if (Math.Abs(totalScheduled - dto.Amount) > 0.01m)
                    return Results<EmployeeAdvanceDto>.Failure("Schedule total must match advance amount");

                // Remove only pending (unprocessed) schedules
                var pendingSchedules = advance.EmployeeAdvanceSchedules
                    .Where(s => string.IsNullOrEmpty(s.PayrolInvoiceId) && s.StatusId != "PAID")
                    .ToList();

                _context.EmployeeAdvanceSchedules.RemoveRange(pendingSchedules);

                // Add new schedules from DTO
                int nextInstallment = advance.EmployeeAdvanceSchedules.Count(s => !string.IsNullOrEmpty(s.PayrolInvoiceId)) + 1;

                foreach (var sch in dto.CustomDeductionSchedules.OrderBy(s => s.DueDate))
                {
                    if (!string.IsNullOrEmpty(sch.PayrollInvoiceId)) continue; // skip already processed

                    var schedule = new EmployeeAdvanceSchedule
                    {
                        ScheduleId = Guid.NewGuid().ToString(),
                        AdvanceId = advance.AdvanceId,
                        InstallmentNumber = nextInstallment++,
                        DueDate = sch.DueDate,
                        ScheduledAmount = sch.ScheduledAmount,
                        DeductedAmount = 0,
                        StatusId = "SCHEDULED",
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow
                    };
                    _context.EmployeeAdvanceSchedules.Add(schedule);
                }

                advance.InstallmentCount = advance.EmployeeAdvanceSchedules.Count(s => !string.IsNullOrEmpty(s.PayrolInvoiceId)) 
                                         + dto.CustomDeductionSchedules.Count(s => string.IsNullOrEmpty(s.PayrollInvoiceId));
            }
            else if (wasLongTerm)
            {
                // Switching type from long-term to short-term
                _context.EmployeeAdvanceSchedules.RemoveRange(advance.EmployeeAdvanceSchedules);
                advance.InstallmentCount = dto.InstallmentCount ?? 0;
            }

            // Update linked Payment
            var companyPartyId = await _productStoreService.GetProductStorePayToPartId();
            await _paymentHelperService.UpdatePayment(new CreatePaymentParam
            {
                PaymentId = advance.PaymentId,
                PartyIdFrom = companyPartyId,
                PartyIdTo = dto.PartyId,
                Amount = dto.Amount,
                EffectiveDate = dto.AdvanceDate,
                PaymentTypeId = dto.AdvanceTypeId,
                Comments = dto.Description
            });

            // Update Advance
            advance.PartyId = dto.PartyId;
            advance.AdvanceDate = dto.AdvanceDate;
            advance.Amount = dto.Amount ?? 0;
            advance.AdvanceTypeId = dto.AdvanceTypeId;
            advance.Description = dto.Description;
            advance.LastUpdatedStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            // Return updated record
            var resultRecord = await BuildEmployeeAdvanceDto(advance, request.Language, ct);
            return Results<EmployeeAdvanceDto>.Success(resultRecord);
        }
    }
}