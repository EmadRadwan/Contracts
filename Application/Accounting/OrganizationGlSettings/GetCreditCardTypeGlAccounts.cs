using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings
{
    public class GetCreditCardTypeGlAccounts
    {
        public class Query : IRequest<Result<List<GetCreditCardTypeGlAccountDto>>>
        {
            public string CompanyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<GetCreditCardTypeGlAccountDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<GetCreditCardTypeGlAccountDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var creditCardTypeGlAccounts = await (from cctga in _context.CreditCardTypeGlAccounts
                    join a in _context.GlAccounts
                        on cctga.GlAccountId equals a.GlAccountId
                    where cctga.OrganizationPartyId == request.CompanyId
                    select new GetCreditCardTypeGlAccountDto
                    {
                        CardType = cctga.CardType,
                        GlAccountId = cctga.GlAccountId,
                        GlAccountName = cctga.GlAccountId + " - " + a.AccountName // Concatenate GlAccountId and GlAccountName
                    }).ToListAsync(cancellationToken);

                return Result<List<GetCreditCardTypeGlAccountDto>>.Success(creditCardTypeGlAccounts!);
            }
        }
    }
}