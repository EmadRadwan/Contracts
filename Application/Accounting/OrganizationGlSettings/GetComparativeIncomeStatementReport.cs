using Application.Accounting.Reports;
using MediatR;
using Persistence;
using AutoMapper;
using Application.Accounting.Services;

namespace Application.Accounting.OrganizationGlSettings
{
    public class GetComparativeIncomeStatementReport
    {
        public class Query : IRequest<Result<ComparativeIncomeStatementViewModel>>
        {
            public string OrganizationPartyId { get; set; }
            public DateTime? FromDate1 { get; set; }
            public DateTime? ThruDate1 { get; set; }
            public string GlFiscalTypeId1 { get; set; }
            public int? SelectedMonth1 { get; set; }
            
            public DateTime? FromDate2 { get; set; }
            public DateTime? ThruDate2 { get; set; }
            public string GlFiscalTypeId2 { get; set; }
            public int? SelectedMonth2 { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ComparativeIncomeStatementViewModel>>
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

            public async Task<Result<ComparativeIncomeStatementViewModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var context = await _acctgReportsService.GenerateComparativeIncomeStatement(
                        request.OrganizationPartyId, 
                        request.FromDate1, request.ThruDate1, 
                        request.FromDate2, request.ThruDate2, 
                        request.GlFiscalTypeId1, request.GlFiscalTypeId2,
                        request.SelectedMonth1, request.SelectedMonth2);
                    
                    return Result<ComparativeIncomeStatementViewModel>.Success(context);
                }
                catch (Exception ex)
                {
                    return Result<ComparativeIncomeStatementViewModel>.Failure(ex.Message);
                }
            }
        }
    }
}
