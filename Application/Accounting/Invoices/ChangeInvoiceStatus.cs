using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Accounting.Invoices;
using Application.Accounting.Services;
using MediatR;
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