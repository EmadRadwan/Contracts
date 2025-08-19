using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings
{
    public class GetFinAccountTypeGlAccounts
    {
        public class Query : IRequest<Result<List<GetFinAccountTypeGlAccountDto>>>
        {
            public string CompanyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<GetFinAccountTypeGlAccountDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<GetFinAccountTypeGlAccountDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var finAccountTypeGlAccounts = await (from fatga in _context.FinAccountTypeGlAccounts
                    join a in _context.GlAccounts
                        on fatga.GlAccountId equals a.GlAccountId
                    join fat in _context.FinAccountTypes
                        on fatga.FinAccountTypeId equals fat.FinAccountTypeId
                    where fatga.OrganizationPartyId == request.CompanyId
                    select new GetFinAccountTypeGlAccountDto
                    {
                        FinAccountTypeId = fat.FinAccountTypeId,
                        FinAccountTypeDescription = fat.Description, // Assuming Description is the description field
                        GlAccountId = fatga.GlAccountId,
                        GlAccountName = fatga.GlAccountId + " - " + a.AccountName // Concatenate GlAccountId and GlAccountName
                    }).ToListAsync(cancellationToken);

                return Result<List<GetFinAccountTypeGlAccountDto>>.Success(finAccountTypeGlAccounts!);
            }
        }
    }
}