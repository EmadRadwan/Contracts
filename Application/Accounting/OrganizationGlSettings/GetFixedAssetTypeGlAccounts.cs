using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings
{
    public class GetFixedAssetTypeGlAccounts
    {
        public class Query : IRequest<Result<List<GetFixedAssetTypeGlAccountDto>>>
        {
            public string CompanyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<GetFixedAssetTypeGlAccountDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<GetFixedAssetTypeGlAccountDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var fixedAssetTypeGlAccounts = await (from f in _context.FixedAssetTypeGlAccounts
                    join a in _context.GlAccounts
                        on f.AssetGlAccountId equals a.GlAccountId into aGroup
                    from asset in aGroup.DefaultIfEmpty()
                    join ad in _context.GlAccounts
                        on f.AccDepGlAccountId equals ad.GlAccountId into adGroup
                    from accDep in adGroup.DefaultIfEmpty()
                    join d in _context.GlAccounts
                        on f.DepGlAccountId equals d.GlAccountId into dGroup
                    from dep in dGroup.DefaultIfEmpty()
                    join p in _context.GlAccounts
                        on f.ProfitGlAccountId equals p.GlAccountId into pGroup
                    from profit in pGroup.DefaultIfEmpty()
                    join l in _context.GlAccounts
                        on f.LossGlAccountId equals l.GlAccountId into lGroup
                    from loss in lGroup.DefaultIfEmpty()
                    join org in _context.Parties
                        on f.OrganizationPartyId equals org.PartyId
                    where f.OrganizationPartyId == request.CompanyId
                    select new GetFixedAssetTypeGlAccountDto
                    {
                        FixedAssetTypeId = f.FixedAssetTypeId,
                        FixedAssetId = f.FixedAssetId,
                        OrganizationPartyId = f.OrganizationPartyId,
                        AssetGlAccountId = f.AssetGlAccountId,
                        AssetGlAccountName = asset.AccountName,
                        AccDepGlAccountId = f.AccDepGlAccountId,
                        AccDepGlAccountName = accDep.AccountName,
                        DepGlAccountId = f.DepGlAccountId,
                        DepGlAccountName = dep.AccountName,
                        ProfitGlAccountId = f.ProfitGlAccountId,
                        ProfitGlAccountName = profit.AccountName,
                        LossGlAccountId = f.LossGlAccountId,
                        LossGlAccountName = loss.AccountName
                    }).ToListAsync(cancellationToken);

                return Result<List<GetFixedAssetTypeGlAccountDto>>.Success(fixedAssetTypeGlAccounts!);
            }
        }
    }
}
