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

            public string
                Language
            {
                get;
                set;
            } // REFACTOR: Added Language property to support "en" or "ar" for bilingual descriptions
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
                var language = request.Language ?? "en"; // fallback

                // 1. Pre-calculate invoice totals separately (no cartesian risk)
                var invoiceTotals = _context.InvoiceItems
                    .Where(ii => ii.InvoiceId != null)
                    .GroupBy(ii => ii.InvoiceId)
                    .Select(g => new
                    {
                        InvoiceId = g.Key,
                        Total = g.Sum(ii => ii.Quantity * ii.Amount)
                    });

                // 2. Main query – get one row per invoice with certificate info
                var query = from inv in _context.Invoices
                    join invt in _context.InvoiceTypes on inv.InvoiceTypeId equals invt.InvoiceTypeId
                    join fromParty in _context.Parties on inv.PartyIdFrom equals fromParty.PartyId
                    join toParty in _context.Parties on inv.PartyId equals toParty.PartyId
                    join sts in _context.StatusItems on inv.StatusId equals sts.StatusId
                    join bil in _context.BillingAccounts on inv.BillingAccountId equals bil.BillingAccountId into bilGj
                    from bil in bilGj.DefaultIfEmpty()

                    // Get OrderId + CertificateNumber (at most one per invoice)
                    // REFACTOR: Use FirstOrDefault instead of join explosion
                    let orderInfo = _context.OrderItemBillings
                        .Where(oib => oib.InvoiceId == inv.InvoiceId)
                        .Select(oib => new { oib.OrderId })
                        .FirstOrDefault()
                    let certificate = orderInfo != null
                        ? _context.WorkEfforts
                            .Where(we =>
                                we.RelatedOrderId == orderInfo.OrderId && we.WorkEffortTypeId == "PROJECT_CERTIFICATE")
                            .Select(we => we.CertificateNumber)
                            .FirstOrDefault()
                        : null

                    // Join pre-calculated total
                    join tot in invoiceTotals on inv.InvoiceId equals tot.InvoiceId into totGj
                    from tot in totGj.DefaultIfEmpty()
                    select new InvoiceRecord
                    {
                        InvoiceId = inv.InvoiceId,
                        InvoiceTypeId = inv.InvoiceTypeId,
                        InvoiceTypeDescription = language == "ar" ? invt.DescriptionArabic : invt.Description,

                        InvoiceDate = inv.InvoiceDate,
                        DueDate = inv.DueDate,
                        PaidDate = inv.PaidDate,

                        StatusId = inv.StatusId,
                        StatusDescription = language == "ar" ? sts.DescriptionArabic : sts.Description,

                        Description = inv.Description,

                        PartyIdFrom = new InvoicePartyDto
                        {
                            FromPartyId = inv.PartyIdFrom,
                            FromPartyName = fromParty.Description
                        },
                        FromPartyName = fromParty.Description,

                        PartyId = new InvoicePartyDto
                        {
                            FromPartyId = inv.PartyId,
                            FromPartyName = toParty.Description
                        },
                        ToPartyName = toParty.Description,

                        BillingAccountId = inv.BillingAccountId,
                        BillingAccountName = bil != null ? bil.Description : null,

                        Total = tot != null ? tot.Total : 0m,
                        OutstandingAmount = 0m, // calculate separately if needed

                        OrderId = orderInfo != null ? orderInfo.OrderId : null,
                        CertificateNumber = certificate
                    };

                return await Task.FromResult(query);
            }
        }
    }
}