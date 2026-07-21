using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Shipments.OrganizationGlSettings;

public class GetCurrentTimePeriod
{
    public class Query : IRequest<Result<CustomTimePeriodDto>>
    {
        public string OrganizationPartyId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Query, Result<CustomTimePeriodDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<CustomTimePeriodDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var customTimePeriod = await _context.CustomTimePeriods
                .Where(x => x.OrganizationPartyId == request.OrganizationPartyId && x.IsClosed != "Y")
                .OrderByDescending(x => x.FromDate)
                .Select(x => new CustomTimePeriodDto
                {
                    CustomTimePeriodId = x.CustomTimePeriodId,
                    ParentPeriodId = x.ParentPeriodId,
                    PeriodTypeId = x.PeriodTypeId,
                    PeriodTypeDescription = x.PeriodType.Description,
                    PeriodNum = x.PeriodNum,
                    PeriodName = x.PeriodName,
                    FromDate = x.FromDate,
                    ThruDate = x.ThruDate,
                    IsClosed = x.IsClosed
                }).FirstOrDefaultAsync(cancellationToken);

            if (customTimePeriod == null)
                return Result<CustomTimePeriodDto>.Failure("No open time period found for this organization");

            return Result<CustomTimePeriodDto>.Success(customTimePeriod);
        }
    }
}
