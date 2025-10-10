using Application.Interfaces;
using Application.Shipments.Transactions;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;
using FluentValidation;

namespace Application.Accounting.Transactions;

public class ListAccountingTransactionEntries
{
    public class Query : IRequest<IQueryable<AccountingTransactionEntryRecord>>
    {
        public ODataQueryOptions<AccountingTransactionEntryRecord> Options { get; set; }
        public string Language { get; set; }
    }

    // REFACTOR: Added QueryValidator to ensure Language is provided and valid.
    // Purpose: Aligns with validation pattern in provided example to prevent invalid queries.
    // Context: Ensures Language is not empty and follows the same validation approach as WorkEffortId and Language in the example.
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.Language).NotEmpty().WithMessage("Language is required");
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
            // REFACTOR: Added validation for the query to ensure Language is valid.
            // Purpose: Prevents invalid queries, consistent with the example's validation pattern.
            // Context: Matches the validation approach in the provided CertificateItemDto handler.
            var validator = new QueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            // REFACTOR: Normalized language to lowercase and default to "en" if null.
            // Purpose: Ensures consistent language handling as in the example where language is normalized to "en" or "ar".
            // Context: Aligns with the example's language handling logic for UomName.
            var language = request.Language?.ToLower() == "ar" ? "ar" : "en";

            // REFACTOR: Modified query to include AccountNameArabic and DescriptionArabic based on language.
            // Purpose: Conditionally selects Arabic or English names for GlAccountName and AcctgTransactionTypeDescription.
            // Context: Mirrors the example's logic for UomName where DescriptionArabic is used when language is "ar".
            var query = (from te in _context.AcctgTransEntries
                         join t in _context.AcctgTrans on te.AcctgTransId equals t.AcctgTransId into transJoin
                         from trans in transJoin.DefaultIfEmpty()
                         join a in _context.AcctgTransTypes on trans.AcctgTransTypeId equals a.AcctgTransTypeId into transTypeJoin
                         from transType in transTypeJoin.DefaultIfEmpty()
                         join g in _context.GlAccounts on te.GlAccountId equals g.GlAccountId into glAccountJoin
                         from glAccount in glAccountJoin.DefaultIfEmpty()
                         join p in _context.Parties on te.PartyId equals p.PartyId into partyJoin
                         from party in partyJoin.DefaultIfEmpty()
                         select new AccountingTransactionEntryRecord
                         {
                             AcctgTransId = te.AcctgTransId,
                             AcctgTransEntrySeqId = te.AcctgTransEntrySeqId,
                             // REFACTOR: Select AccountNameArabic if language is "ar", else AccountName.
                             // Purpose: Provides Arabic account name when requested, aligning with example's UomName logic.
                             GlAccountName = glAccount != null ? (language == "ar" ? glAccount.AccountNameArabic : glAccount.AccountName) : null,
                             // REFACTOR: Select DescriptionArabic if language is "ar", else Description.
                             // Purpose: Provides Arabic transaction type description when requested, consistent with example.
                             //AcctgTransactionTypeDescription = transType != null ? (language == "ar" ? transType.DescriptionArabic : transType.Description) : null,
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
                             GlFiscalTypeId = trans.GlFiscalTypeId
                         }).AsQueryable();

            return query;
        }
    }
}