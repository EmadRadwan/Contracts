using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core;

namespace Application.Accounting.OrganizationGlSettings;

public class CheckGlAccountAssignedToOrganization
{
    public class Query : IRequest<Results<CheckGlAccountAssignedResult>>
    {
        public string GlAccountId { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Query, Results<CheckGlAccountAssignedResult>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Results<CheckGlAccountAssignedResult>> Handle(Query request, CancellationToken cancellationToken)
        {
            var exists = await _context.GlAccountOrganizations
                .AnyAsync(x => x.GlAccountId == request.GlAccountId
                            && x.OrganizationPartyId == request.CompanyId,
                    cancellationToken);

            return Results<CheckGlAccountAssignedResult>.Success(new CheckGlAccountAssignedResult
            {
                IsAssigned = exists
            });
        }
    }
}

public class CheckGlAccountAssignedResult
{
    public bool IsAssigned { get; set; }
}
