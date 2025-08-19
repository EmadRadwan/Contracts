using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Shipments.OrganizationGlSettings;

public class ListCustomTimePeriodTypesLov
{
    public class Query : IRequest<Result<List<PeriodTypeDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<PeriodTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PeriodTypeDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var periodTypes = await _context.PeriodTypes
                .Select(x => new PeriodTypeDto
                {
                    PeriodTypeId = x.PeriodTypeId,
                    PeriodTypeDescription = x.Description,
                }).ToListAsync(cancellationToken);


            return Result<List<PeriodTypeDto>>.Success(periodTypes!);
        }
    }
}