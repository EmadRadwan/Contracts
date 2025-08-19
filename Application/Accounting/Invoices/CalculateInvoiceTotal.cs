using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Application.Accounting.Services;

namespace Application.Accounting.Invoices;

public class CalculateInvoiceTotal
{
    // REFACTOR: Removed incorrect IRequest<List<InvoiceTotalsDto>> implementation
    // Ensures Query matches the handler's return type to resolve MediatR registration error
    public class Query : IRequest<InvoiceTotalsDto>
    {
        public string InvoiceId { get; set; }
    }

    public class Handler : IRequestHandler<Query, InvoiceTotalsDto>
    {
        private readonly IInvoiceUtilityService _invoiceUtilityService;

        public Handler(IInvoiceUtilityService invoiceUtilityService)
        {
            _invoiceUtilityService = invoiceUtilityService;
        }

        public async Task<InvoiceTotalsDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var total = await _invoiceUtilityService.GetInvoiceTotal(request.InvoiceId, true);
            var outstanding = await _invoiceUtilityService.GetInvoiceNotApplied(request.InvoiceId);

            return new InvoiceTotalsDto
            {
                InvoiceId = request.InvoiceId,
                Total = total,
                OutstandingAmount = outstanding
            };
        }
    }

    public class InvoiceTotalsDto
    {
        public string InvoiceId { get; set; }
        public decimal Total { get; set; }
        public decimal OutstandingAmount { get; set; }
    }
}