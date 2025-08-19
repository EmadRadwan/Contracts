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
    public class GenerateComparativeBalanceSheet
    {
        public class Query : IRequest<Result<ComparativeBalanceSheetResult>>
        {
            public string OrganizationPartyId { get; set; }
            public DateTime? Period1ThruDate { get; set; }
            public DateTime? Period2ThruDate { get; set; }
            public string Period1GlFiscalTypeId { get; set; }
            public string Period2GlFiscalTypeId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ComparativeBalanceSheetResult>>
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

            public async Task<Result<ComparativeBalanceSheetResult>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var context = await _acctgReportsService.GenerateComparativeBalanceSheet(request.OrganizationPartyId, request.Period1ThruDate, request.Period1GlFiscalTypeId, request.Period2ThruDate, request.Period2GlFiscalTypeId);
                    return Result<ComparativeBalanceSheetResult>.Success(context);
                }
                catch (Exception ex)
                {
                    return Result<ComparativeBalanceSheetResult>.Failure(ex.Message);
                }
            }
        }
    }
}