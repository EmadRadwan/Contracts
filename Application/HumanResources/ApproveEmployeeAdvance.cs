using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.HumanResources;

public class ApproveEmployeeAdvance
{
    public class Command : IRequest<Results<EmployeeAdvanceDto>>
    {
        public string AdvanceId { get; set; } = null!;
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Command, Results<EmployeeAdvanceDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Results<EmployeeAdvanceDto>> Handle(Command request, CancellationToken ct)
        {
            var advance = await _context.EmployeeAdvances
                .FirstOrDefaultAsync(x => x.AdvanceId == request.AdvanceId, ct);

            if (advance == null)
                return Results<EmployeeAdvanceDto>.Failure("Employee advance not found", "ADVANCE_NOT_FOUND");

            if (advance.StatusId == "ADVANCE_APPROVED")
                return Results<EmployeeAdvanceDto>.Failure("Employee advance is already approved", "ADVANCE_ALREADY_APPROVED");

            advance.StatusId = "ADVANCE_APPROVED";
            advance.LastUpdatedStamp = DateTime.UtcNow;

            var success = await _context.SaveChangesAsync(ct) > 0;

            if (!success) return Results<EmployeeAdvanceDto>.Failure("Failed to approve employee advance");

            // Return the updated record
            var party = await _context.Parties.FirstOrDefaultAsync(p => p.PartyId == advance.PartyId, ct);
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
