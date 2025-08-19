using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Accounting.OrganizationGlSettings;

namespace Application.Accounting.FinAccounts;

public class ListFinAccountTransStatus
{
    public class Query : IRequest<Result<List<FinAccountTransStatusDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<FinAccountTransStatusDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<FinAccountTransStatusDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var finAccountTransStatuses = await  _context.StatusItems
            .Where(x => x.StatusTypeId == "FINACT_TRNS_STATUS")
            .Select(x => new FinAccountTransStatusDto
            {
                FinAccountTransStatusId = x.StatusId,
                FinAccountTransStatusDescription = x.Description
            }).ToListAsync(cancellationToken);

            return Result<List<FinAccountTransStatusDto>>.Success(finAccountTransStatuses);
        }
    }
}