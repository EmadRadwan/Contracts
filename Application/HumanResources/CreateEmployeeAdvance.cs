using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.HumanResources;

public class CreateEmployeeAdvance
{
    public class Command : IRequest<Result<EmployeeAdvanceRecord>>
    {
        public EmployeeAdvanceRecord AdvanceDto { get; set; } = null!;
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Command, Result<EmployeeAdvanceRecord>>
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;

        public Handler(DataContext context, IUtilityService utilityService)
        {
            _context = context;
            _utilityService = utilityService;
        }

        public async Task<Result<EmployeeAdvanceRecord>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.AdvanceDto;
            
            var advanceId = await _utilityService.GetNextSequence("EmployeeAdvance");

            var advance = new EmployeeAdvance
            {
                AdvanceId = advanceId,
                PartyId = dto.EmployeePartyId,
                AdvanceDate = dto.AdvanceDate ?? DateTime.Now,
                Amount = dto.Amount ?? 0,
                CurrencyUomId = dto.CurrencyUomId ?? "EGP",
                InstallmentCount = dto.InstallmentCount ?? 0,
                InstallmentAmount = dto.InstallmentAmount ?? 0,
                StartDate = dto.StartDate ?? DateTime.Now,
                StatusId = "ADVANCE_ACTIVE",
                Description = dto.Description,
                CreatedStamp = DateTime.Now,
                LastUpdatedStamp = DateTime.Now
            };

            _context.EmployeeAdvances.Add(advance);

            var success = await _context.SaveChangesAsync(ct) > 0;

            if (!success) return Result<EmployeeAdvanceRecord>.Failure("Failed to create employee advance");

            // Return the record for UI
            var party = await _context.Parties.FirstOrDefaultAsync(p => p.PartyId == dto.EmployeePartyId, ct);
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
