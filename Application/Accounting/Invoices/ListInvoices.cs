using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;
using Domain;


namespace Application.Accounting.Invoices
{
    public class ListInvoices
    {
        public class Query : IRequest<IQueryable<InvoiceView>>
        {
            public ODataQueryOptions<InvoiceView> Options { get; set; } = null!;
        }

        public class Handler : IRequestHandler<Query, IQueryable<InvoiceView>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public Task<IQueryable<InvoiceView>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Directly return the DbSet mapped to the view
                var query = _context.InvoiceRecords.AsQueryable();

                return Task.FromResult(query);
            }
        }
    }
}