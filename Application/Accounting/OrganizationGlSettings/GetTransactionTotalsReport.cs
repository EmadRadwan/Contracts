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
    public class GetTransactionTotalsReport
    {
        public class Query : IRequest<Result<TransactionTotalsViewModel>>
        {
            public string OrganizationPartyId { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ThruDate { get; set; }
            public string? GlFiscalTypeId { get; set; }
            public int? SelectedMonth { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<TransactionTotalsViewModel>>
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

            public async Task<Result<TransactionTotalsViewModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var context = await _acctgReportsService.GetTransactionTotals(request.OrganizationPartyId, request.FromDate, request.ThruDate, request.GlFiscalTypeId, request.SelectedMonth);
                    return Result<TransactionTotalsViewModel>.Success(context);
                }
                catch (Exception ex)
                {
                    return Result<TransactionTotalsViewModel>.Failure(ex.Message);
                }
            }
        }
    }
}