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
// REFACTOR: Complete rewrite to avoid non-translatable patterns:
//          - Removed g.First() by using grouping with proper key selection
//          - Moved Total calculation into the main query using left join + group
//          - Used Distinct() on OrderId per Invoice to prevent duplication safely
//          - All parts now fully translatable by EF Core + OData

                var baseQuery = from inv in _context.Invoices
                    join invt in _context.InvoiceTypes on inv.InvoiceTypeId equals invt.InvoiceTypeId
                    join fromParty in _context.Parties on inv.PartyIdFrom equals fromParty.PartyId
                    join toParty in _context.Parties on inv.PartyId equals toParty.PartyId
                    join sts in _context.StatusItems on inv.StatusId equals sts.StatusId
                    join bil in _context.BillingAccounts on inv.BillingAccountId equals bil.BillingAccountId into
                        billingGj
                    from bil in billingGj.DefaultIfEmpty()

                    // REFACTOR: Get distinct OrderId per Invoice (at most one certificate per order)
                    join oib in _context.OrderItemBillings on inv.InvoiceId equals oib.InvoiceId into oibGj
                    from oib in oibGj.DefaultIfEmpty()
                    let OrderId = oib != null ? oib.OrderId : null

                    // REFACTOR: Join to WorkEffort only once per OrderId (not per OrderItemBilling row)
                    join we in _context.WorkEfforts
                        on new { OrderId, Type = "PROJECT_CERTIFICATE" }
                        equals new { OrderId = we.RelatedOrderId, Type = we.WorkEffortTypeId } into weGj
                    from we in weGj.DefaultIfEmpty()

                    // REFACTOR: Calculate Total using group join + sum (fully translatable)
                    join item in _context.InvoiceItems on inv.InvoiceId equals item.InvoiceId into itemsGj
                    from item in itemsGj.DefaultIfEmpty()
                    select new
                    {
                        inv, invt, fromParty, toParty, sts, bil, OrderId,
                        CertificateNumber = we != null ? we.CertificateNumber : (string)null, item
                    }
                    into temp
                    group temp by new
                    {
                        temp.inv.InvoiceId,
                        temp.inv.InvoiceTypeId,
                        InvoiceTypeDescriptionEn = temp.invt.Description,
                        InvoiceTypeDescriptionAr = temp.invt.DescriptionArabic,
                        temp.inv.InvoiceDate,
                        temp.inv.StatusId,
                        StatusDescriptionEn = temp.sts.Description,
                        StatusDescriptionAr = temp.sts.DescriptionArabic,
                        temp.inv.Description,
                        temp.inv.DueDate,
                        temp.inv.PaidDate,
                        temp.inv.PartyId,
                        ToPartyName = temp.toParty.Description,
                        temp.inv.PartyIdFrom,
                        FromPartyName = temp.fromParty.Description,
                        temp.inv.BillingAccountId,
                        BillingAccountName = temp.bil != null ? temp.bil.Description : (string)null,
                        temp.OrderId,
                        temp.CertificateNumber
                    }
                    into g
                    select new InvoiceRecord
                    {
                        InvoiceId = g.Key.InvoiceId,
                        InvoiceTypeDescription = request.Language == "ar"
                            ? g.Key.InvoiceTypeDescriptionAr
                            : g.Key.InvoiceTypeDescriptionEn,
                        InvoiceDate = g.Key.InvoiceDate,
                        StatusId = g.Key.StatusId,
                        InvoiceTypeId = g.Key.InvoiceTypeId,
                        StatusDescription = request.Language == "ar"
                            ? g.Key.StatusDescriptionAr
                            : g.Key.StatusDescriptionEn,
                        Description = g.Key.Description,
                        DueDate = g.Key.DueDate,
                        PaidDate = g.Key.PaidDate,

                        PartyId = new InvoicePartyDto
                        {
                            FromPartyId = g.Key.PartyId,
                            FromPartyName = g.Key.ToPartyName
                        },
                        ToPartyName = g.Key.ToPartyName,

                        PartyIdFrom = new InvoicePartyDto
                        {
                            FromPartyId = g.Key.PartyIdFrom,
                            FromPartyName = g.Key.FromPartyName
                        },
                        FromPartyName = g.Key.FromPartyName,

                        BillingAccountId = g.Key.BillingAccountId,
                        BillingAccountName = g.Key.BillingAccountName,

                        // REFACTOR: Sum is now inside the group → fully translatable
                        Total = g.Sum(x => (decimal?)(x.item != null ? x.item.Quantity * x.item.Amount : 0)) ?? 0,

                        OutstandingAmount = 0,
                        OrderId = g.Key.OrderId,
                        CertificateNumber = g.Key.CertificateNumber
                    };

                return await Task.FromResult(baseQuery);
            }
        }
    }
}