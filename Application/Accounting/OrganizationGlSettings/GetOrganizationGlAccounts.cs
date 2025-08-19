using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class GetOrganizationGlAccounts
{
    public class Query : IRequest<Result<List<OrganizationGlAccountDto>>>
    {
        public string CompanyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<OrganizationGlAccountDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<OrganizationGlAccountDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            try
            {
                var organizationGlAccounts = await _context.GlAccounts
                    .Join(_context.GlAccountOrganizations,
                        account => account.GlAccountId,
                        accountOrg => accountOrg.GlAccountId,
                        (account, accountOrg) => new { account, accountOrg })
                    .Where(account => account.accountOrg.OrganizationPartyId == request.CompanyId)
                    .Select(account => new OrganizationGlAccountDto
                    {
                        GlAccountId = account.accountOrg.GlAccountId,
                        AccountName = account.account.AccountName
                    }).ToListAsync(cancellationToken);

                return Result<List<OrganizationGlAccountDto>>.Success(organizationGlAccounts);
            }
            catch (Exception ex)
            {
                // Handle exception and return an error result
                return Result<List<OrganizationGlAccountDto>>.Failure(ex.Message);
            }
        }
    }
}