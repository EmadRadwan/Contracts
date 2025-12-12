using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.CostCenters;

public class CreateCostCenter
{
    public class Command : IRequest<Result<CostCenterDto>>
    {
        public string Description { get; set; } = string.Empty;
        public string IsOutPayment { get; set; } = "N"; // Y or N
    }

    public class Handler : IRequestHandler<Command, Result<CostCenterDto>>
    {
        private readonly DataContext _context;
        public Handler(DataContext context) => _context = context;

        public async Task<Result<CostCenterDto>> Handle(Command request, CancellationToken ct)
        {
            var lastId = await _context.CostCenters
                .MaxAsync(x => (int?)Convert.ToInt32(x.CostCenterId), ct) ?? 0;

            var newCenter = new CostCenter
            {
                CostCenterId = (lastId + 1).ToString(),
                Description = request.Description,
                IsOutPayment = request.IsOutPayment,
                CreatedStamp = DateTime.UtcNow,
                CreatedTxStamp = DateTime.UtcNow,
            };

            _context.CostCenters.Add(newCenter);
            var success = await _context.SaveChangesAsync(ct) > 0;

            if (!success) return Result<CostCenterDto>.Failure("Failed to create cost center");

            return Result<CostCenterDto>.Success(new CostCenterDto
            {
                CostCenterId = newCenter.CostCenterId,
                Description = newCenter.Description,
            });
        }
    }
}