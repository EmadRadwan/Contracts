using Application.Accounting.Services;
using Application.Shipments.Invoices;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.Invoices
{
    public class DeleteInvoiceItem
    {
        public class Command : IRequest<ServiceResult>
        {
            public string InvoiceId { get; set; } = string.Empty;
            public string InvoiceItemSeqId { get; set; } = string.Empty;
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.InvoiceId).NotEmpty().WithMessage("Invoice ID is required");
                RuleFor(x => x.InvoiceItemSeqId).NotEmpty().WithMessage("Invoice Item Sequence ID is required");
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

            public async Task<ServiceResult> Handle(Command request, CancellationToken ct)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(ct);

                try
                {
                    // ────────────────────────────────────────────────
                    // Option A: Using helper service (recommended)
                    var serviceResult = await _invoiceHelperService.DeleteInvoiceItem(
                        request.InvoiceId,
                        request.InvoiceItemSeqId);

                    if (serviceResult.IsError)
                    {
                        await transaction.RollbackAsync(ct);
                        return serviceResult;
                    }

                    await _context.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    return new ServiceResult
                    {
                        IsError = false,
                        ErrorMessage = null,
                        Data = null   // or new { Deleted = true } if frontend wants confirmation data
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(ct);

                    return new ServiceResult
                    {
                        IsError = true,
                        ErrorMessage = $"Failed to delete invoice item: {ex.Message}",
                        Data = null
                    };
                }
            }
        }
    }
}