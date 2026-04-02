using Application.Shipments.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using FluentValidation;

namespace Application.Accounting.Transactions;

public class ListAccountingTransactionEntriesByDateRange
{
    public class Query : IRequest<AccountingTransactionEntriesResponse>
    {
        public string CompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Language { get; set; }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.CompanyId).NotEmpty();
            RuleFor(x => x.FromDate).NotEmpty();
            RuleFor(x => x.ToDate).NotEmpty();
            RuleFor(x => x.Language).NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Query, AccountingTransactionEntriesResponse>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<AccountingTransactionEntriesResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language?.ToLower() == "ar" ? "ar" : "en";

            var query = from te in _context.AcctgTransEntries
                        join t in _context.AcctgTrans on te.AcctgTransId equals t.AcctgTransId into transJoin
                        from trans in transJoin.DefaultIfEmpty()
                        join tt in _context.AcctgTransTypes
                            on trans.AcctgTransTypeId equals tt.AcctgTransTypeId into typeJoin
                        from acctType in typeJoin.DefaultIfEmpty()
                        join g in _context.GlAccounts on te.GlAccountId equals g.GlAccountId into glAccountJoin
                        from glAccount in glAccountJoin.DefaultIfEmpty()
                        join go in _context.GlAccountOrganizations on (glAccount != null ? glAccount.GlAccountId : null) equals go.GlAccountId into glAccountOrgJoin
                        from glAccountOrg in glAccountOrgJoin.DefaultIfEmpty()
                        join p in _context.Parties on trans.PartyId equals p.PartyId into partyJoin
                        from party in partyJoin.DefaultIfEmpty()
                        join we in _context.WorkEfforts on trans.WorkEffortId equals we.WorkEffortId into workEffortJoin
                        from workEffort in workEffortJoin.DefaultIfEmpty()
                        join prod in _context.Products on te.ProductId equals prod.ProductId into productJoin
                        from product in productJoin.DefaultIfEmpty()
                        where glAccountOrg.OrganizationPartyId == request.CompanyId &&
                              trans.TransactionDate >= request.FromDate &&
                              trans.TransactionDate <= request.ToDate
                        orderby trans.TransactionDate descending
                        select new AccountingTransactionEntryRecord
                        {
                            AcctgTransId = te.AcctgTransId,
                            AcctgTransEntrySeqId = te.AcctgTransEntrySeqId,
                            GlAccountName = glAccount != null
                                ? (language == "ar" ? glAccount.AccountNameArabic : glAccount.AccountName)
                                : null,
                            Description = te.Description,
                            PartyId = te.PartyId,
                            PartyName = party != null ? party.Description : null,
                            ProductId = te.ProductId,
                            ProductName = product != null ? product.ProductName : null,
                            InvoiceId = trans.InvoiceId,
                            PaymentId = trans.PaymentId,
                            ShipmentId = trans.ShipmentId,
                            WorkEffortId = trans.WorkEffortId,
                            CertificateNumber = workEffort != null ? workEffort.CertificateNumber : null,
                            GlAccountTypeId = te.GlAccountTypeId,
                            GlAccountId = te.GlAccountId,
                            Amount = te.Amount,
                            DebitCreditFlag = te.DebitCreditFlag,
                            IsPosted = trans.IsPosted,
                            PostedDate = trans.PostedDate,
                            TransactionDate = trans.TransactionDate,
                            GlFiscalTypeId = trans.GlFiscalTypeId,
                            AcctgTransactionTypeDescription = acctType != null ? acctType.Description : null,
                            CreatedStamp = te.CreatedStamp,
                            SalesRequestId = trans.SalesRequestId
                        };

            var data = await query.ToListAsync(cancellationToken);

            return new AccountingTransactionEntriesResponse
            {
                Data = data,
                Total = data.Count
            };
        }
    }
}

public class AccountingTransactionEntriesResponse
{
    public List<AccountingTransactionEntryRecord> Data { get; set; }
    public int Total { get; set; }
}
