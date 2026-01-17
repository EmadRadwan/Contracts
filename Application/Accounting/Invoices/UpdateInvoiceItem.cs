using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Application.Shipments.Invoices;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace Application.Accounting.Invoices
{
    public class UpdateInvoiceItem
    {
        public class Command : IRequest<ServiceResult>
        {
            public InvoiceItemParameters Parameters { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Parameters).NotNull();
                RuleFor(x => x.Parameters.InvoiceId).NotEmpty();
                RuleFor(x => x.Parameters.InvoiceItemSeqId).NotEmpty();
                // Add more specific rules if needed, for example:
                // RuleFor(x => x.Parameters.Amount).GreaterThan(0).When(x => x.Parameters.Amount.HasValue);
            }
        }

        public class Handler : IRequestHandler<Command, ServiceResult>
        {
            private readonly DataContext _context;
            private readonly IInvoiceHelperService _invoiceHelperService;

            public Handler(
                DataContext context,
                IInvoiceHelperService invoiceHelperService)
            {
                _context = context;
                _invoiceHelperService = invoiceHelperService;
            }

            public async Task<ServiceResult> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Call the UpdateInvoiceItem method from the helper service
                    var serviceResult = await _invoiceHelperService.UpdateInvoiceItem(request.Parameters);

                    if (serviceResult.IsError)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return serviceResult;
                    }

                    // Only save if there were actual changes (the service already decided whether to update)
                    // But we still call SaveChanges in case the helper marked the entity as modified
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    return serviceResult;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return new ServiceResult
                    {
                        IsError = true,
                        ErrorMessage = $"Error updating invoice item: {ex.Message}"
                    };
                }
            }
        }
    }
}