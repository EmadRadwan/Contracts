using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Accounting.Invoices;
using Application.Accounting.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class ChangeInvoiceStatus
{
    public class Query : IRequest<InvoiceStatusDto>
    {
        public string InvoiceId { get; set; }
        public string StatusId { get; set; }
        public DateTime? StatusDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public bool ActualCurrency { get; set; }
    }

    public class Handler : IRequestHandler<Query, InvoiceStatusDto>
    {
        private readonly IInvoiceUtilityService _invoiceUtilityService;
        private readonly DataContext _context;


        public Handler(DataContext context, IInvoiceUtilityService invoiceUtilityService)
        {
            _invoiceUtilityService = invoiceUtilityService;
            _context = context;
        }

        public async Task<InvoiceStatusDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Invoke the SetInvoiceStatus method
                await _invoiceUtilityService.SetInvoiceStatus(
                    request.InvoiceId,
                    request.StatusId,
                    request.StatusDate,
                    request.PaidDate,
                    request.ActualCurrency
                );
                
                var affectedRecords = _context.ChangeTracker
                    .Entries()
                    .Where(e => e.State == EntityState.Added ||
                                e.State == EntityState.Modified ||
                                e.State == EntityState.Deleted)
                    .Select(e => new ChangeRecord
                    {
                        TableName = e.Entity.GetType().Name,
                        PKValues = string.Join(", ", e.Properties
                            .Where(p => p.Metadata.IsPrimaryKey())
                            .Select(p => $"{p.Metadata.Name}: {p.CurrentValue}")),
                        Operation = e.State.ToString(),
                        // REFACTOR: Added conditional check to include GlAccountId for AcctgTransEntries
                        // Purpose: Captures GlAccountId specifically for AcctgTransEntries entities
                        // Improvement: Enhances audit trail with critical financial data
                        AdditionalInfo = e.Entity.GetType().Name == "AcctgTransEntries" 
                            ? $"GlAccountId: {e.Property("GlAccountId")?.CurrentValue ?? "N/A"}"
                            : null
                    })
                    .ToList();
       
                foreach (var record in affectedRecords)
                {
                    Console.WriteLine(record);
                }

                
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                
                // Fetch the updated invoice status to return in the response
                var updatedStatus = await _invoiceUtilityService.GetInvoiceStatus(request.InvoiceId);


                return updatedStatus; // Return the updated status
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                // Log the error and throw
                throw new Exception("An error occurred while updating the invoice status.", ex);
            }
        }
    }
}