using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class GetVarianceReasonGlAccounts
{
    public class Query : IRequest<Result<List<GetVarianceReasonGlAccountDto>>>
    {
        public string CompanyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<GetVarianceReasonGlAccountDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<GetVarianceReasonGlAccountDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var varianceReasonsGlAccounts = await (from v in _context.VarianceReasonGlAccounts
                join a in _context.GlAccounts
                    on v.GlAccountId equals a.GlAccountId
                where v.OrganizationPartyId == request.CompanyId
                select new GetVarianceReasonGlAccountDto
                {
                    VarianceReasonId = v.VarianceReasonId,
                    VarianceReasonDescription = v.VarianceReason.Description,
                    GlAccountId = v.GlAccountId + " - " + a.AccountName,
                    GlAccountName = a.AccountName
                }).ToListAsync(cancellationToken);


            return Result<List<GetVarianceReasonGlAccountDto>>.Success(varianceReasonsGlAccounts!);
        }
    }
}