using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.Transactions;

public class ListAccountingTransactionTypes
{
    public class Query : IRequest<Result<List<AcctgTransTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<AcctgTransTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<AcctgTransTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var allowedAcctgTransTypes = new[]
            {
                "_NA_", "AMORTIZATION", "CAPITALIZATION", "CREDIT_LINE", "CREDIT_MEMO",
                "CUST_RTN_INVOICE", "DEPRECIATION", "DISBURSEMENT", "EXTERNAL_ACCTG_TRANS",
                "INCOMING_PAYMENT", "INTERNAL_ACCTG_TRANS", "INVENTORY", "INVENTORY_RETURN",
                "ITEM_VARIANCE", "MANUFACTURING", "NOTE", "OBLIGATION_ACCTG_TRA", "OTHER_INTERNAL",
                "OTHER_OBLIGATION", "OUTGOING_PAYMENT", "PAYMENT_ACCTG_TRANS", "PAYMENT_APPL",
                "PERIOD_CLOSING", "PURCHASE_INVOICE", "RECEIPT", "SALES", "SALES_INVOICE",
                "SALES_SHIPMENT", "SHIPMENT_RECEIPT", "TAX_DUE"
            };
            var query = _context.AcctgTransTypes
                .Where(z => allowedAcctgTransTypes.Contains(z.AcctgTransTypeId))
                .OrderBy(x => x.Description)
                .Select(x => new AcctgTransTypeDto
                {
                    AcctgTransTypeId = x.AcctgTransTypeId,
                    ParentTypeId = x.ParentTypeId,
                    Description = x.Description
                })
                .AsQueryable();


            var acctgTransTypes = await query
                .ToListAsync(cancellationToken);

            return Result<List<AcctgTransTypeDto>>.Success(acctgTransTypes);
        }
    }
}