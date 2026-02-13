using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.HumanResources;

public class UpdateEmployeeAdvance
{
    public class Command : IRequest<Result<EmployeeAdvanceRecord>>
    {
        public EmployeeAdvanceRecord AdvanceDto { get; set; } = null!;
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Command, Result<EmployeeAdvanceRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<EmployeeAdvanceRecord>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.AdvanceDto;

            var advance = await _context.EmployeeAdvances
                .FirstOrDefaultAsync(x => x.AdvanceId == dto.AdvanceId, ct);

            if (advance == null) return null;

            advance.AdvanceDate = dto.AdvanceDate ?? advance.AdvanceDate;
            advance.Amount = dto.Amount ?? advance.Amount;
            advance.CurrencyUomId = dto.CurrencyUomId ?? advance.CurrencyUomId;
            advance.InstallmentCount = dto.InstallmentCount ?? advance.InstallmentCount;
            advance.InstallmentAmount = dto.InstallmentAmount ?? advance.InstallmentAmount;
            advance.StartDate = dto.StartDate ?? advance.StartDate;
            advance.Description = dto.Description;
            advance.LastUpdatedStamp = DateTime.Now;

            var success = await _context.SaveChangesAsync(ct) > 0;

            if (!success) return Result<EmployeeAdvanceRecord>.Failure("Failed to update employee advance");

            // Return the record for UI
            var party = await _context.Parties.FirstOrDefaultAsync(p => p.PartyId == advance.PartyId, ct);
            var status = await _context.StatusItems
                .FirstOrDefaultAsync(s => s.StatusId == advance.StatusId, ct);

            var resultRecord = new EmployeeAdvanceRecord
            {
                AdvanceId = advance.AdvanceId,
                EmployeePartyId = advance.PartyId,
                EmployeeName = party?.Description ?? advance.PartyId,
                AdvanceDate = advance.AdvanceDate,
                Amount = advance.Amount,
                CurrencyUomId = advance.CurrencyUomId,
                InstallmentCount = advance.InstallmentCount,
                InstallmentAmount = advance.InstallmentAmount,
                StartDate = advance.StartDate,
                StatusId = advance.StatusId,
                StatusDescription = request.Language == "ar" 
                    ? (status?.DescriptionArabic ?? status?.Description ?? advance.StatusId) 
                    : (status?.Description ?? advance.StatusId),
                Description = advance.Description,
                CreatedStamp = advance.CreatedStamp,
                LastUpdatedStamp = advance.LastUpdatedStamp
            };

            return Result<EmployeeAdvanceRecord>.Success(resultRecord);
        }
    }
}
