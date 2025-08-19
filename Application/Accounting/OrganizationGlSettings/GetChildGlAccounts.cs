using MediatR;
using Persistence;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Shipments.GlobalGlSettings
{
    public class GetChildGlAccounts
    {
        public class Query : IRequest<Result<List<GlAccountDto>>>
        {
            public string ParentGlAccountId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<GlAccountDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<GlAccountDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var childAccounts = _context.GlAccounts
                        .Where(a => a.ParentGlAccountId == request.ParentGlAccountId)  // Select child accounts for the provided parentGlAccountId
                        .Select(account => new GlAccountDto
                        {
                            GlAccountId = account.GlAccountId,
                            GlAccountTypeId = account.GlAccountTypeId,
                            GlAccountTypeDescription = account.GlAccountType.Description,
                            GlAccountClassId = account.GlAccountClassId,
                            GlResourceTypeId = account.GlResourceTypeId,
                            GlResourceTypeDescription = account.GlAccountClass.Description,
                            ParentGlAccountId = account.ParentGlAccountId,
                            AccountCode = account.AccountCode,
                            AccountName = account.AccountName
                        })
                        .ToList();

                    return Result<List<GlAccountDto>>.Success(childAccounts);
                }
                catch (Exception ex)
                {
                    return Result<List<GlAccountDto>>.Failure(ex.Message);
                }
            }
        }
    }
}
