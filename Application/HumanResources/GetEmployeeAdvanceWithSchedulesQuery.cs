using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.HumanResources
{
    public class GetEmployeeAdvanceWithSchedulesQuery
    {
        public class Query : IRequest<Results<EmployeeAdvanceDetailDto>>
        {
            public string AdvanceId { get; set; } = null!;
            public string Language { get; set; } = "en";
        }

        public class Handler : IRequestHandler<Query, Results<EmployeeAdvanceDetailDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Results<EmployeeAdvanceDetailDto>> Handle(Query request, CancellationToken ct)
            {
                var advance = await _context.EmployeeAdvances
                    .AsNoTracking()
                    .Include(a => a.EmployeeAdvanceSchedules)
                    .FirstOrDefaultAsync(a => a.AdvanceId == request.AdvanceId, ct);

                if (advance == null)
                {
                    return Results<EmployeeAdvanceDetailDto>.Failure(
                        $"Employee advance with ID {request.AdvanceId} not found.",
                        "ADVANCE_NOT_FOUND");
                }

                // Status description (translated)
                var statusDesc = await _context.StatusItems
                    .Where(s => s.StatusId == advance.StatusId)
                    .Select(s => request.Language == "ar"
                        ? (s.DescriptionArabic ?? s.Description ?? s.StatusId)
                        : (s.Description ?? s.StatusId))
                    .FirstOrDefaultAsync(ct) ?? advance.StatusId;

                // Employee name
                var employeeName = await _context.Parties
                    .Where(p => p.PartyId == advance.PartyId)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(ct) ?? advance.PartyId;

                var dto = new EmployeeAdvanceDetailDto
                {
                    AdvanceId = advance.AdvanceId,
                    PartyId = advance.PartyId,
                    EmployeeName = employeeName,
                    AdvanceTypeId = advance.AdvanceTypeId,
                    AdvanceDate = advance.AdvanceDate,
                    Amount = advance.Amount,
                    InstallmentCount = advance.InstallmentCount,
                    StartDate = advance.StartDate,
                    StatusId = advance.StatusId,
                    StatusDescription = statusDesc,
                    Description = advance.Description,

                    // Schedules – ordered by InstallmentNumber
                    Schedules = advance.EmployeeAdvanceSchedules
                        .OrderBy(s => s.InstallmentNumber)
                        .Select(s => new ScheduleDto
                        {
                            ScheduleId = s.ScheduleId,
                            InstallmentNumber = s.InstallmentNumber,
                            DueDate = s.DueDate,
                            ScheduledAmount = s.ScheduledAmount,
                            DeductedAmount = s.DeductedAmount,
                            StatusId = s.StatusId,
                            PayrollInvoiceId = s.PayrolInvoiceId,
                            Notes = s.Notes
                        })
                        .ToList()
                };

                return Results<EmployeeAdvanceDetailDto>.Success(dto);
            }
        }
    }

    // ────────────────────────────────────────────────
    // DTOs
    // ────────────────────────────────────────────────

    public class EmployeeAdvanceDetailDto
    {
        public string AdvanceId { get; set; } = null!;
        public string PartyId { get; set; } = null!;
        public string? EmployeeName { get; set; }
        public string? AdvanceTypeId { get; set; }
        public DateOnly? AdvanceDate { get; set; }
        public decimal Amount { get; set; }
        public int? InstallmentCount { get; set; }
        public DateOnly? StartDate { get; set; }
        public string? StatusId { get; set; }
        public string? StatusDescription { get; set; }
        public string? Description { get; set; }

        public List<ScheduleDto> Schedules { get; set; } = new();
    }

    public class ScheduleDto
    {
        public string ScheduleId { get; set; } = null!;
        public int InstallmentNumber { get; set; }
        public DateOnly? DueDate { get; set; }
        public decimal ScheduledAmount { get; set; }
        public decimal DeductedAmount { get; set; }
        public string? StatusId { get; set; }
        public string? PayrollInvoiceId { get; set; }
        public string? Notes { get; set; }
    }
}