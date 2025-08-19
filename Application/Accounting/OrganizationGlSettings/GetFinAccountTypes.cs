using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings;

public class GetFinAccountTypes
{
    public class Query : IRequest<Result<List<FinAccountTypeDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<FinAccountTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<FinAccountTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var finAccountTypes = await (from fat in _context.FinAccountTypes
                select new FinAccountTypeDto
                {
                    FinAccountTypeId = fat.FinAccountTypeId,
                    FinAccountTypeDescription = fat.Description
                }
                ).ToListAsync(cancellationToken);

                return Result<List<FinAccountTypeDto>>.Success(finAccountTypes);
            }
            catch (Exception ex)
            {
                // Handle exception and return an error result
                return Result<List<FinAccountTypeDto>>.Failure(ex.Message);
            }
        }
    }
}