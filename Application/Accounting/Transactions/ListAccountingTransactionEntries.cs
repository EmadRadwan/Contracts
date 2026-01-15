using Application.Shipments.Transactions;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;
using FluentValidation;

namespace Application.Accounting.Transactions;

public class ListAccountingTransactionEntries
{
    public class Query : IRequest<IQueryable<AccountingTransactionEntryRecord>>
    {
        public ODataQueryOptions<AccountingTransactionEntryRecord> Options { get; set; }
        public string Language { get; set; }
        public string CompanyId { get; set; }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.Language).NotEmpty().WithMessage("Language is required");
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .When(x => x.CompanyId != null)
                .WithMessage("CompanyId cannot be empty if provided");
        }
    }

    public class Handler : IRequestHandler<Query, IQueryable<AccountingTransactionEntryRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<AccountingTransactionEntryRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var validator = new QueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var language = request.Language?.ToLower() == "ar" ? "ar" : "en";

            var query = (from te in _context.AcctgTransEntries
                join t in _context.AcctgTrans on te.AcctgTransId equals t.AcctgTransId into transJoin
                from trans in transJoin.DefaultIfEmpty()
                join tt in _context.AcctgTransTypes 
                    on trans.AcctgTransTypeId equals tt.AcctgTransTypeId into typeJoin
                from acctType in typeJoin.DefaultIfEmpty()
                join g in _context.GlAccounts on te.GlAccountId equals g.GlAccountId into glAccountJoin
                from glAccount in glAccountJoin.DefaultIfEmpty()
                join go in _context.GlAccountOrganizations on glAccount != null ? glAccount.GlAccountId : null equals
                    go.GlAccountId into glAccountOrgJoin
                from glAccountOrg in glAccountOrgJoin.DefaultIfEmpty()
                join p in _context.Parties on trans.PartyId equals p.PartyId into partyJoin
                from party in partyJoin.DefaultIfEmpty()
                join we in _context.WorkEfforts on trans.WorkEffortId equals we.WorkEffortId into workEffortJoin
                from workEffort in workEffortJoin.DefaultIfEmpty()
                join prod in _context.Products on te.ProductId equals prod.ProductId into productJoin
                from product in productJoin.DefaultIfEmpty()

                where request.CompanyId == null ||
                      (glAccountOrg != null && glAccountOrg.OrganizationPartyId == request.CompanyId)
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
                    ProductName = product.ProductName,
                    InvoiceId = trans.InvoiceId,
                    PaymentId = trans.PaymentId,
                    ShipmentId = trans.ShipmentId,
                    WorkEffortId = trans.WorkEffortId,
                    CertificateNumber = workEffort.CertificateNumber,
                    GlAccountTypeId = te.GlAccountTypeId,
                    GlAccountId = te.GlAccountId,
                    Amount = te.Amount,
                    DebitCreditFlag = te.DebitCreditFlag,
                    IsPosted = trans.IsPosted,
                    PostedDate = trans.PostedDate,
                    TransactionDate = trans.TransactionDate,
                    GlFiscalTypeId = trans.GlFiscalTypeId,
                    AcctgTransactionTypeDescription = acctType.Description,
                    CreatedStamp = te.CreatedStamp
                }).AsQueryable();

            return query;
        }
    }
}