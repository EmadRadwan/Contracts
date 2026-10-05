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
    public class Query : IRequest<Result<InvoiceStatusDto>>
    {
        public string InvoiceId { get; set; }
        public string StatusId { get; set; }
        public DateOnly? StatusDate { get; set; }
        public DateOnly? PaidDate { get; set; }
        public bool ActualCurrency { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<InvoiceStatusDto>>
    {
        private readonly IInvoiceUtilityService _invoiceUtilityService;
        private readonly IGlAccountOrganizationGuard _glAccountGuard;
        private readonly DataContext _context;


        public Handler(DataContext context, IInvoiceUtilityService invoiceUtilityService,
            IGlAccountOrganizationGuard glAccountGuard)
        {
            _invoiceUtilityService = invoiceUtilityService;
            _glAccountGuard = glAccountGuard;
            _context = context;
        }

        public async Task<Result<InvoiceStatusDto>> Handle(Query request, CancellationToken cancellationToken)
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

                // Turns the ACCTTXENT_GLACOG FK violation into a message the user can act on.
                await _glAccountGuard.EnsurePendingEntriesAssignedAsync(cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                
                // Fetch the updated invoice status to return in the response
                var updatedStatus = await _invoiceUtilityService.GetInvoiceStatus(request.InvoiceId);


                return Result<InvoiceStatusDto>.Success(updatedStatus);
            }
            catch (Exception ex) when (ex is GlAccountNotAssignedException or ClosedAccountingPeriodException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<InvoiceStatusDto>.Failure(ex.Message);
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