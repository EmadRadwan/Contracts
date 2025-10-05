using API.Controllers.Accounting.Transactions;
using Application.Interfaces;
using AutoMapper;
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
            // REFACTOR: Modified query to include left joins with WorkEffort table twice:
            // 1. For certificates (WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE') to get CertificateNumber.
            // 2. For projects (WORK_EFFORT_TYPE_ID = 'PROJECT') to get ProjectNumber and ProjectName.
            // Left joins ensure records are returned even if WorkEffortId doesn't match a certificate or project.
            // The certificate's PROJECT_ID links to the project's WORK_EFFORT_ID for accurate project data.
            var query = (from transaction in _context.AcctgTrans
                         join transactionType in _context.AcctgTransTypes on transaction.AcctgTransTypeId equals transactionType.AcctgTransTypeId
                         join certificate in _context.WorkEfforts on new { transaction.WorkEffortId, Type = "PROJECT_CERTIFICATE" } equals new { WorkEffortId = certificate.WorkEffortId, Type = certificate.WorkEffortTypeId } into certGroup
                         from certificate in certGroup.DefaultIfEmpty()
                         join project in _context.WorkEfforts on new { ProjectId = certificate != null ? certificate.ProjectId : transaction.WorkEffortId, Type = "PROJECT" } equals new { ProjectId = project.WorkEffortId, Type = project.WorkEffortTypeId } into projGroup
                         from project in projGroup.DefaultIfEmpty()
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
                         }).AsQueryable();

            return await Task.FromResult(query);
        }
    }
}