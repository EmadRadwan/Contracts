using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings
{
    public class GetTaxAuthorityGlAccounts
    {
        public class Query : IRequest<Result<List<GetTaxAuthorityGlAccountDto>>>
        {
            public string CompanyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<GetTaxAuthorityGlAccountDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<GetTaxAuthorityGlAccountDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var taxAuthorityGlAccounts = await (from t in _context.TaxAuthorityGlAccounts
                    join a in _context.GlAccounts
                        on t.GlAccountId equals a.GlAccountId
                    join p in _context.Parties
                        on t.TaxAuthPartyId equals p.PartyId
                    where t.OrganizationPartyId == request.CompanyId
                    select new GetTaxAuthorityGlAccountDto
                    {
                        TaxAuthGeoId = t.TaxAuthGeoId,
                        TaxAuthPartyId = t.TaxAuthPartyId,
                        TaxAuthPartyName = p.Description,
                        GlAccountId = t.GlAccountId,
                        GlAccountName = t.GlAccountId + " - " + a.AccountName // Concatenate GlAccountId and AccountName
                    }).ToListAsync(cancellationToken);

                return Result<List<GetTaxAuthorityGlAccountDto>>.Success(taxAuthorityGlAccounts!);
            }
        }
    }
}