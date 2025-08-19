using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Shipments.OrganizationGlSettings;

public class ListCustomTimePeriodsLov
{
    public class Query : IRequest<Result<List<CustomTimePeriodDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<CustomTimePeriodDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CustomTimePeriodDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var customTimePeriods = await _context.CustomTimePeriods
                .OrderBy(x => x.FromDate)
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
                }).ToListAsync(cancellationToken);


            return Result<List<CustomTimePeriodDto>>.Success(customTimePeriods!);
        }
    }
}