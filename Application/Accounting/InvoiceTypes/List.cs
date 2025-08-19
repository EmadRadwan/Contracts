using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.InvoiceTypes;

public class List
{
    public class Query : IRequest<Result<List<InvoiceTypeDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<InvoiceTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<InvoiceTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Update allowed invoice types to match specified requirements
            // Purpose: Restricts results to CUST_RTN_INVOICE, PURC_RTN_INVOICE, PURCHASE_INVOICE, SALES_INVOICE
            // Context: Aligns with OFBiz EditPaymentApplications screen's focus on specific invoice types for payment applications
            var allowedInvoiceTypes = new[]
            {
                "CUST_RTN_INVOICE",
                "PURC_RTN_INVOICE",
                "PURCHASE_INVOICE",
                "SALES_INVOICE"
            };

            // REFACTOR: Add filter for allowed invoice types
            // Purpose: Ensures only specified invoice types are returned, improving relevance and performance
            var query = _context.InvoiceTypes
                .Where(x => x.ParentTypeId != null && allowedInvoiceTypes.Contains(x.InvoiceTypeId))
                .OrderBy(x => x.Description)
                .Select(x => new InvoiceTypeDto
                {
                    InvoiceTypeId = x.InvoiceTypeId,
                    ParentTypeId = x.ParentTypeId,
                    Description = x.Description
                })
                .AsQueryable();

            var invoiceTypes = await query
                .ToListAsync(cancellationToken);

            return Result<List<InvoiceTypeDto>>.Success(invoiceTypes);
        }
    }
}