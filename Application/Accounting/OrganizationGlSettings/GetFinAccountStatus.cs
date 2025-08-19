using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings;

public class GetFinAccountStatus
{
    public class Query : IRequest<Result<List<FinAccountStatusDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<FinAccountStatusDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<FinAccountStatusDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var finAccountStatuses = await (from sts in _context.StatusItems
                where sts.StatusTypeId == "FINACCT_STATUS"
                select new FinAccountStatusDto
                {
                    FinAccountStatusId = sts.StatusId,
                    FinAccountStatusDescription = sts.Description
                }
                ).ToListAsync(cancellationToken);

                return Result<List<FinAccountStatusDto>>.Success(finAccountStatuses);
            }
            catch (Exception ex)
            {
                // Handle exception and return an error result
                return Result<List<FinAccountStatusDto>>.Failure(ex.Message);
            }
        }
    }
}