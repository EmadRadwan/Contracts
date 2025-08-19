using MediatR;
using Application.Accounting.Services;
using Application.Shipments.Invoices;


namespace Application.Accounting.Invoices
{
    public class ListNotAppliedInvoices
    {
        public class Query : IRequest<Result<List<InvoiceMap>>>
        {
            public string PaymentId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<InvoiceMap>>>
        {
            private readonly IInvoiceService _invoiceService;

            public Handler(IInvoiceService invoiceService)
            {
                _invoiceService = invoiceService;
            }

            public async Task<Result<List<InvoiceMap>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Call the ListNotAppliedInvoices method from the IInvoiceService
                var context = await _invoiceService.ListNotAppliedInvoices(request.PaymentId);

                var nonAppliedInvoices = new List<InvoiceMap>();

                if (context.Invoices != null && context.Invoices.Any())
                {
                    foreach (var invoice in context.Invoices)
                    {
                        nonAppliedInvoices.Add(invoice);
                    }
                }

                if (context.InvoicesOtherCurrency != null && context.InvoicesOtherCurrency.Any())
                {
                    foreach (var invoice in context.InvoicesOtherCurrency)
                    {
                        nonAppliedInvoices.Add(invoice);
                    }
                }

                return Result<List<InvoiceMap>>.Success(nonAppliedInvoices);
            }
        }
    }
}
