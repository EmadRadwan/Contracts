using MediatR;
using Persistence;
using AutoMapper;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Accounting.Services;
using Application.Shipments.Reports;



using Application.Accounting.OrganizationGlSettings;

namespace Application.Shipments.OrganizationGlSettings
{
    public class GetGlAccountTrialBalanceReport
    {
        public class Query : IRequest<Result<GlAccountTrialBalanceResult>>
        {
            public string OrganizationPartyId { get; set; }
            public string? IsPosted { get; set; }
            public string TimePeriodId { get; set; }
            public string GlAccountId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<GlAccountTrialBalanceResult>>
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

            public async Task<Result<GlAccountTrialBalanceResult>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var context = await _acctgReportsService.GenerateGlAccountTrialBalance(request.OrganizationPartyId, request.GlAccountId, request.TimePeriodId, request.IsPosted);
                    return Result<GlAccountTrialBalanceResult>.Success(context);
                }
                catch (Exception ex)
                {
                    return Result<GlAccountTrialBalanceResult>.Failure(ex.Message);
                }
            }
        }
    }
}