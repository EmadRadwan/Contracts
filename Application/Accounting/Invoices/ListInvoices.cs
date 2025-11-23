using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;
using Application.Shipments.Invoices;
using Microsoft.EntityFrameworkCore;

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
                var language = request.Language ?? "en";

                // STEP 1: Get ALL invoice items for the relevant invoices (only what we need)
                var relevantInvoiceItems = await _context.InvoiceItems
                    .Where(ii => ii.InvoiceId != null)
                    .Select(ii => new
                    {
                        ii.InvoiceId,
                        ii.Quantity,
                        ii.Amount
                    })
                    .ToListAsync(cancellationToken);

                // STEP 2: In-memory grouping + correct per-line rounding (AwayFromZero)
                var invoiceTotalsDict = relevantInvoiceItems
                    .GroupBy(x => x.InvoiceId!)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var lineTotal = g.Sum(item =>
                                item.Quantity * Math.Round((decimal)item.Amount, 5, MidpointRounding.AwayFromZero)
                            );

                            // REFACTOR: Second rounding — this is what pushes 30999.99999 → 31000.00
                            return Math.Round((decimal)lineTotal, 2, MidpointRounding.AwayFromZero);
                        }
                    );
                
                // Optional: Log to prove it's working
                // foreach (var kv in invoiceTotalsDict)
                //     _logger.LogInformation("Invoice {Id} → Rounded Total: {Total}", kv.Key, kv.Value);

                // STEP 3: Main query — 100% translatable, no Math.Round, no client code
                var baseQuery = from inv in _context.Invoices
                    join invt in _context.InvoiceTypes on inv.InvoiceTypeId equals invt.InvoiceTypeId
                    join fromParty in _context.Parties on inv.PartyIdFrom equals fromParty.PartyId
                    join toParty in _context.Parties on inv.PartyId equals toParty.PartyId
                    join sts in _context.StatusItems on inv.StatusId equals sts.StatusId
                    join bil in _context.BillingAccounts on inv.BillingAccountId equals bil.BillingAccountId into
                        billingGj
                    from bil in billingGj.DefaultIfEmpty()

                    // Certificate handling
                    join oib in _context.OrderItemBillings on inv.InvoiceId equals oib.InvoiceId into oibGj
                    from oib in oibGj.DefaultIfEmpty()
                    let OrderId = oib != null ? oib.OrderId : null
                    join we in _context.WorkEfforts
                        on new { OrderId, Type = "PROJECT_CERTIFICATE" }
                        equals new { OrderId = we.RelatedOrderId, Type = we.WorkEffortTypeId } into weGj
                    from we in weGj.DefaultIfEmpty()
                    group new
                        {
                            inv, invt, fromParty, toParty, sts, bil,
                            OrderId,
                            CertificateNumber = we != null ? we.CertificateNumber : (string?)null
                        }
                        by new
                        {
                            inv.InvoiceId,
                            inv.InvoiceTypeId,
                            InvoiceTypeDescriptionEn = invt.Description,
                            InvoiceTypeDescriptionAr = invt.DescriptionArabic,
                            inv.InvoiceDate,
                            inv.DueDate,
                            inv.PaidDate,
                            inv.StatusId,
                            StatusDescriptionEn = sts.Description,
                            StatusDescriptionAr = sts.DescriptionArabic,
                            inv.Description,
                            inv.PartyId,
                            ToPartyName = toParty.Description,
                            inv.PartyIdFrom,
                            FromPartyName = fromParty.Description,
                            inv.BillingAccountId,
                            BillingAccountName = bil != null ? bil.Description : (string?)null,
                            OrderId,
                            CertificateNumber = we != null ? we.CertificateNumber : (string?)null
                        }
                    into g
                    select new InvoiceRecord
                    {
                        InvoiceId = g.Key.InvoiceId,
                        InvoiceTypeId = g.Key.InvoiceTypeId,
                        InvoiceTypeDescription = language == "ar"
                            ? g.Key.InvoiceTypeDescriptionAr
                            : g.Key.InvoiceTypeDescriptionEn,
                        InvoiceDate = g.Key.InvoiceDate,
                        DueDate = g.Key.DueDate,
                        PaidDate = g.Key.PaidDate,
                        StatusId = g.Key.StatusId,
                        StatusDescription = language == "ar" ? g.Key.StatusDescriptionAr : g.Key.StatusDescriptionEn,
                        Description = g.Key.Description,

                        PartyId = new InvoicePartyDto
                            { FromPartyId = g.Key.PartyId, FromPartyName = g.Key.ToPartyName },
                        ToPartyName = g.Key.ToPartyName,

                        PartyIdFrom = new InvoicePartyDto
                            { FromPartyId = g.Key.PartyIdFrom, FromPartyName = g.Key.FromPartyName },
                        FromPartyName = g.Key.FromPartyName,

                        BillingAccountId = g.Key.BillingAccountId,
                        BillingAccountName = g.Key.BillingAccountName,

                        Total = 0m, // will be replaced
                        OutstandingAmount = 0m,
                        OrderId = g.Key.OrderId,
                        CertificateNumber = g.Key.CertificateNumber
                    };

                // STEP 4: Final projection — apply correct accounting total
                return baseQuery
                    .AsEnumerable()
                    .Select(x =>
                    {
                        x.Total = invoiceTotalsDict.TryGetValue(x.InvoiceId, out var total) ? total : 0m;
                        return x;
                    })
                    .AsQueryable();
            }
        }
    }
}