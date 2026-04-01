using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Domain;

namespace Application.Accounting.Invoices;

public class CheckExistingPayrollInvoices
{
    public class Query : IRequest<Result<bool>>
    {
        public DateTime InvoiceDate { get; set; }
        public string OrganizationPartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<bool>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(Query request, CancellationToken cancellationToken)
        {
            var monthStart = new DateTime(request.InvoiceDate.Year, request.InvoiceDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddTicks(-1);


            var existingInvoicesExist = await _context.Invoices
                .AnyAsync(i => i.InvoiceTypeId == "PAYROL_INVOICE"
                            && i.InvoiceDate >= monthStart
                            && i.InvoiceDate <= monthEnd, cancellationToken);

            return Result<bool>.Success(existingInvoicesExist);
        }
    }
}
