using Application.Interfaces;
using Application.Shipments.Transactions;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;
using FluentValidation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Accounting.Transactions;

public class ListAccountingTransactionEntries
{
    public class Query : IRequest<IQueryable<AccountingTransactionEntryRecord>>
    {
        public ODataQueryOptions<AccountingTransactionEntryRecord> Options { get; set; }
        public string Language { get; set; }
        // REFACTOR: Add CompanyId to the query model
        // Allows the frontend to pass companyId for filtering transaction entries by organization.
        public string CompanyId { get; set; }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.Language).NotEmpty().WithMessage("Language is required");
            // REFACTOR: Make CompanyId validation optional
            // Only validates CompanyId if provided, allowing queries without companyId for superusers.
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

        public async Task<IQueryable<AccountingTransactionEntryRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Log the received CompanyId for debugging
            Console.WriteLine($"Received CompanyId in handler: {request.CompanyId}");

            var validator = new QueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var language = request.Language?.ToLower() == "ar" ? "ar" : "en";

            // REFACTOR: Add join with GlAccountOrganization and filter by CompanyId
            // Filters transaction entries based on OrganizationPartyId, using LEFT JOINs to include
            // entries without associated GlAccountOrganization records if CompanyId is null.
            var query = (from te in _context.AcctgTransEntries
                         join t in _context.AcctgTrans on te.AcctgTransId equals t.AcctgTransId into transJoin
                         from trans in transJoin.DefaultIfEmpty()
                         join a in _context.AcctgTransTypes on trans.AcctgTransTypeId equals a.AcctgTransTypeId into transTypeJoin
                         from transType in transTypeJoin.DefaultIfEmpty()
                         join g in _context.GlAccounts on te.GlAccountId equals g.GlAccountId into glAccountJoin
                         from glAccount in glAccountJoin.DefaultIfEmpty()
                         join go in _context.GlAccountOrganizations on glAccount != null ? glAccount.GlAccountId : null equals go.GlAccountId into glAccountOrgJoin
                         from glAccountOrg in glAccountOrgJoin.DefaultIfEmpty()
                         join p in _context.Parties on te.PartyId equals p.PartyId into partyJoin
                         from party in partyJoin.DefaultIfEmpty()
                         where request.CompanyId == null || (glAccountOrg != null && glAccountOrg.OrganizationPartyId == request.CompanyId)
                         select new AccountingTransactionEntryRecord
                         {
                             AcctgTransId = te.AcctgTransId,
                             AcctgTransEntrySeqId = te.AcctgTransEntrySeqId,
                             GlAccountName = glAccount != null ? (language == "ar" ? glAccount.AccountNameArabic : glAccount.AccountName) : null,
                             Description = te.Description,
                             PartyId = te.PartyId,
                             PartyName = party != null ? party.Description : null,
                             ProductId = te.ProductId,
                             InvoiceId = trans.InvoiceId,
                             PaymentId = trans.PaymentId,
                             ShipmentId = trans.ShipmentId,
                             WorkEffortId = trans.WorkEffortId,
                             GlAccountTypeId = te.GlAccountTypeId,
                             GlAccountId = te.GlAccountId,
                             Amount = te.Amount,
                             DebitCreditFlag = te.DebitCreditFlag,
                             IsPosted = trans.IsPosted,
                             PostedDate = trans.PostedDate,
                             TransactionDate = trans.TransactionDate,
                             GlFiscalTypeId = trans.GlFiscalTypeId,
                             AcctgTransactionTypeDescription = transType != null ? transType.Description : null
                         }).AsQueryable();

            return await Task.FromResult(query);
        }
    }
}