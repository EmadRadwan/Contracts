using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;
using Application.Shipments.Invoices;

namespace Application.Accounting.Invoices
{
    public class ListInvoices
    {
        public class Query : IRequest<IQueryable<InvoiceRecord>>
        {
            public ODataQueryOptions<InvoiceRecord> Options { get; set; }
            public string Language { get; set; } // REFACTOR: Added Language property to support "en" or "ar" for bilingual descriptions
        }

        public class Handler : IRequestHandler<Query, IQueryable<InvoiceRecord>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<IQueryable<InvoiceRecord>> Handle(Query request, CancellationToken cancellationToken)
            {
                var baseQuery = from inv in _context.Invoices
                    join invt in _context.InvoiceTypes on inv.InvoiceTypeId equals invt.InvoiceTypeId
                    join fromParty in _context.Parties on inv.PartyIdFrom equals fromParty.PartyId
                    join toParty in _context.Parties on inv.PartyId equals toParty.PartyId
                    join sts in _context.StatusItems on inv.StatusId equals sts.StatusId
                    join bil in _context.BillingAccounts on inv.BillingAccountId equals bil.BillingAccountId into
                        billingGroup
                    from bil in billingGroup.DefaultIfEmpty()
                    select new InvoiceRecord
                    {
                        InvoiceId = inv.InvoiceId,
                        InvoiceTypeDescription = request.Language == "ar" ? invt.DescriptionArabic : invt.Description,
                        InvoiceDate = inv.InvoiceDate,
                        StatusId = inv.StatusId,
                        InvoiceTypeId = inv.InvoiceTypeId,
                        StatusDescription = request.Language == "ar" ? sts.DescriptionArabic : sts.Description,
                        Description = inv.Description,
                        DueDate = inv.DueDate,
                        PaidDate = inv.PaidDate,
                        PartyId = new InvoicePartyDto
                        {
                            FromPartyId = inv.PartyId,
                            FromPartyName = toParty.Description
                        },
                        ToPartyName = toParty.Description,
                        PartyIdFrom = new InvoicePartyDto
                        {
                            FromPartyId = inv.PartyIdFrom,
                            FromPartyName = fromParty.Description
                        },
                        FromPartyName = fromParty.Description,
                        BillingAccountId = inv.BillingAccountId,
                        BillingAccountName = bil.Description,
                        // Return zero for now
                        Total = 0,
                        OutstandingAmount = 0
                    };


                return baseQuery;
            }
        }
    }
}