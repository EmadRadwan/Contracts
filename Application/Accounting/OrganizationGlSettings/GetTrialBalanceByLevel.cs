using MediatR;
using Persistence;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using Application.Accounting.Services;
using Application.Shipments.Reports;

namespace Application.Shipments.OrganizationGlSettings
{
    // Additive sibling of GetTrialBalanceReport.cs: returns the same trial balance rolled up
    // by Chart of Accounts hierarchy level instead of a flat leaf-account list.
    public class GetTrialBalanceByLevel
    {
        public class Query : IRequest<Result<TrialBalanceByLevelContext>>
        {
            public string CustomTimePeriodId { get; set; }
            public string OrganizationPartyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<TrialBalanceByLevelContext>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IAcctgReportsService _acctgReportsService;

            public Handler(DataContext context, IMapper mapper, IAcctgReportsService acctgReportsService)
            {
                _mapper = mapper;
                _context = context;
                _acctgReportsService = acctgReportsService;
            }

            public async Task<Result<TrialBalanceByLevelContext>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var context = await _acctgReportsService.ComputeTrialBalanceByLevel(request.CustomTimePeriodId, request.OrganizationPartyId);
                    return Result<TrialBalanceByLevelContext>.Success(context);
                }
                catch (Exception ex)
                {
                    return Result<TrialBalanceByLevelContext>.Failure(ex.Message);
                }
            }
        }
    }
}
