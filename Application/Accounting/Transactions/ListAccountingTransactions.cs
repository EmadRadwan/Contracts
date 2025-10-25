using API.Controllers.Accounting.Transactions;
using Application.Interfaces;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.Transactions;

public class ListAccountingTransactions
{
    public class Query : IRequest<IQueryable<AccountingTransactionRecord>>
    {
        public ODataQueryOptions<AccountingTransactionRecord> Options { get; set; }
        public string CompanyId { get; set; }
    }
    
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            // REFACTOR: Add validation for CompanyId
            // Ensures that a valid companyId is provided to prevent invalid queries.
            RuleFor(x => x.CompanyId).NotEmpty().WithMessage("CompanyId is required");
        }
    }

    
    public class Handler : IRequestHandler<Query, IQueryable<AccountingTransactionRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<AccountingTransactionRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var validator = new QueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }
            
            var query = (from transaction in _context.AcctgTrans
                         join transactionType in _context.AcctgTransTypes on transaction.AcctgTransTypeId equals transactionType.AcctgTransTypeId
                         join transEntry in _context.AcctgTransEntries on transaction.AcctgTransId equals transEntry.AcctgTransId
                         join glAccount in _context.GlAccounts on transEntry.GlAccountId equals glAccount.GlAccountId
                         join glAccountOrg in _context.GlAccountOrganizations on glAccount.GlAccountId equals glAccountOrg.GlAccountId
                         join certificate in _context.WorkEfforts on new { transaction.WorkEffortId, Type = "PROJECT_CERTIFICATE" } equals new { WorkEffortId = certificate.WorkEffortId, Type = certificate.WorkEffortTypeId } into certGroup
                         from certificate in certGroup.DefaultIfEmpty()
                         join project in _context.WorkEfforts on new { ProjectId = certificate != null ? certificate.ProjectId : transaction.WorkEffortId, Type = "PROJECT" } equals new { ProjectId = project.WorkEffortId, Type = project.WorkEffortTypeId } into projGroup
                         from project in projGroup.DefaultIfEmpty()
                         where glAccountOrg.OrganizationPartyId == request.CompanyId // Filter by companyId
                         select new AccountingTransactionRecord
                         {
                             AcctgTransId = transaction.AcctgTransId,
                             AcctgTransTypeId = transaction.AcctgTransTypeId,
                             AcctgTransTypeDescription = transactionType.Description,
                             PartyId = transaction.PartyId,
                             PaymentId = transaction.PaymentId,
                             TransactionDate = transaction.TransactionDate,
                             IsPosted = transaction.IsPosted,
                             PostedDate = transaction.PostedDate,
                             Description = transaction.Description,
                             InvoiceId = transaction.InvoiceId,
                             WorkEffortId = transaction.WorkEffortId,
                             ShipmentId = transaction.ShipmentId,
                             CertificateNumber = certificate != null ? certificate.CertificateNumber : null,
                             ProjectNumber = project != null ? project.WorkEffortId : null,
                             ProjectName = project != null ? project.ProjectName : null
                         }).Distinct().AsQueryable();

            return await Task.FromResult(query);
        }
    }
}