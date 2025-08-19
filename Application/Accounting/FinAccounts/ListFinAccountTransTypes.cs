using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Accounting.OrganizationGlSettings;

namespace Application.Accounting.FinAccounts;

public class ListFinAccountTransTypes
{
    public class Query : IRequest<Result<List<FinAccountTransTypeDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<FinAccountTransTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<FinAccountTransTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var finAccountTransTypes = await  _context.FinAccountTransTypes
            .Select(x => new FinAccountTransTypeDto
            {
                FinAccountTransTypeId = x.FinAccountTransTypeId,
                FinAccountTransTypeDescription = x.Description
            }).ToListAsync(cancellationToken);

            return Result<List<FinAccountTransTypeDto>>.Success(finAccountTransTypes);
        }
    }
}