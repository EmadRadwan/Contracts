using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class GetGlAccountTypeDefaults
{
    public class Query : IRequest<Result<List<GlAccountTypeDefaultDto>>>
    {
        public string CompanyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<GlAccountTypeDefaultDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<GlAccountTypeDefaultDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var accountsQuery = await (from accountDef in _context.GlAccountTypeDefaults
                join account in _context.GlAccounts
                    on accountDef.GlAccountId equals account.GlAccountId
                join accountType in _context.GlAccountTypes
                    on accountDef.GlAccountTypeId equals accountType.GlAccountTypeId
                where accountDef.OrganizationPartyId == request.CompanyId
                select new GlAccountTypeDefaultDto
                {
                    GlAccountId = accountDef.GlAccountId + " - " + account.AccountName,
                    GlAccountTypeId = account.GlAccountTypeId,
                    GlAccountTypeDescription = accountType.Description,
                    OrganizationPartyId = accountDef.OrganizationPartyId
                }).ToListAsync(cancellationToken);


            return Result<List<GlAccountTypeDefaultDto>>.Success(accountsQuery!);
        }
    }
}