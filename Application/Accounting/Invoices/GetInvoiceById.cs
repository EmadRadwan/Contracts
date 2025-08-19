using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Invoices
{
    public class GetInvoiceById
    {
        public class Query : IRequest<Result<InvoiceDto4>>
        {
            public string InvoiceId { get; set; }
            public string Language { get; set; } // REFACTOR: Added Language property to support "en" or "ar"
        }

        public class Handler : IRequestHandler<Query, Result<InvoiceDto4>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<InvoiceDto4>> Handle(Query request, CancellationToken cancellationToken)
            {
                var invoice = await (from inv in _context.Invoices
                    join invt in _context.InvoiceTypes on inv.InvoiceTypeId equals invt.InvoiceTypeId
                    join fromParty in _context.Parties on inv.PartyIdFrom equals fromParty.PartyId
                    join toParty in _context.Parties on inv.PartyId equals toParty.PartyId
                    join sts in _context.StatusItems on inv.StatusId equals sts.StatusId
                    join bil in _context.BillingAccounts on inv.BillingAccountId equals bil.BillingAccountId into
                        billingGroup
                    from bil in billingGroup.DefaultIfEmpty()
                    where inv.InvoiceId == request.InvoiceId
                    select new InvoiceDto4
                    {
                        InvoiceId = inv.InvoiceId,
                        InvoiceTypeDescription = request.Language == "ar" ? invt.DescriptionArabic : invt.Description,

                        InvoiceDate = (DateTime)inv.InvoiceDate,
                        StatusId = inv.StatusId,
                        InvoiceTypeId = inv.InvoiceTypeId,
                        Status = inv.StatusId,
                        StatusDescription = request.Language == "ar" ? sts.DescriptionArabic : sts.Description,
                        Description = inv.Description,
                        DueDate = inv.DueDate,
                        PaidDate = inv.PaidDate,
                        PartyId = new InvoicePartyDto4
                        {
                            FromPartyId = inv.PartyId,
                            FromPartyName = toParty.Description
                        },
                        ToPartyName = toParty.Description,
                        PartyIdFrom = new InvoicePartyDto4
                        {
                            FromPartyId = inv.PartyIdFrom,
                            FromPartyName = fromParty.Description
                        },
                        FromPartyName = fromParty.Description,
                        BillingAccountId = inv.BillingAccountId,
                        BillingAccountName = bil != null ? bil.Description : null,
                        Total = 0m,
                        OutstandingAmount = 0m
                    }).FirstOrDefaultAsync(cancellationToken);

                if (invoice == null)
                {
                    return Result<InvoiceDto4>.Failure($"Invoice with ID {request.InvoiceId} not found.");
                }

                return Result<InvoiceDto4>.Success(invoice);
            }
        }
    }
}