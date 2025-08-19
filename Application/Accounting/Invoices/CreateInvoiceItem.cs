using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Application.Shipments.Invoices;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.Invoices
{
    public class CreateInvoiceItem
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
                // You can add additional validation rules here if needed,
                // e.g., RuleFor(x => x.Parameters.InvoiceId).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, ServiceResult>
        {
            private readonly DataContext _context;
            private readonly IInvoiceHelperService _invoiceHelperService;

            public Handler(DataContext context, IInvoiceHelperService invoiceHelperService)
            {
                _context = context;
                _invoiceHelperService = invoiceHelperService;
            }

            public async Task<ServiceResult> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the CreateInvoiceItem method from the helper service.
                    var serviceResult = await _invoiceHelperService.CreateInvoiceItem(request.Parameters);

                    if (serviceResult.IsError)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return serviceResult;
                    }

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
                        ErrorMessage = $"Error creating invoice item: {ex.Message}"
                    };
                }
            }
        }
    }
}
