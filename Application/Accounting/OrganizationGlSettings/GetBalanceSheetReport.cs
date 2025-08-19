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
    public class GetBalanceSheetReport
    {
        public class Query : IRequest<Result<BalanceSheetViewModel>>
        {
            public string OrganizationPartyId { get; set; }
            public DateTime? ThruDate { get; set; }
            public string GlFiscalTypeId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<BalanceSheetViewModel>>
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

            public async Task<Result<BalanceSheetViewModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var context = await _acctgReportsService.GenerateBalanceSheet(request.OrganizationPartyId, request.ThruDate, request.GlFiscalTypeId);
                    return Result<BalanceSheetViewModel>.Success(context);
                }
                catch (Exception ex)
                {
                    return Result<BalanceSheetViewModel>.Failure(ex.Message);
                }
            }
        }
    }
}