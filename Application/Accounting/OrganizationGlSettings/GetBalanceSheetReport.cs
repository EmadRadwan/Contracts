using MediatR;
using Application.Accounting.Services;

namespace Application.Accounting.OrganizationGlSettings
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
            private readonly IAcctgReportsService _acctgReportsService;

            public Handler(IAcctgReportsService acctgReportsService)
            {
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