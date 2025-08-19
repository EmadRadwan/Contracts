using MediatR;
using Persistence;
using AutoMapper;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Accounting.Services;
using Application.Shipments.Reports;




namespace Application.Shipments.OrganizationGlSettings
{
    public class GetTrialBalanceReport
    {
        public class Query : IRequest<Result<TrialBalanceContext>>
        {
            public string CustomTimePeriodId { get; set; }
            public string OrganizationPartyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<TrialBalanceContext>>
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

            public async Task<Result<TrialBalanceContext>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var context = await _acctgReportsService.ComputeTrialBalance(request.CustomTimePeriodId, request.OrganizationPartyId);
                    return Result<TrialBalanceContext>.Success(context);
                }
                catch (Exception ex)
                {
                    return Result<TrialBalanceContext>.Failure(ex.Message);
                }
            }
        }
    }
}