using Application.Accounting.Services;
using Application.Shipments.Invoices;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.Invoices;

public class UpdateInvoice
{
    public class Command : IRequest<Result<InvoiceDto3>>
    {
        public InvoiceDto3 InvoiceDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.InvoiceDto).SetValidator(new CreateInvoiceValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<InvoiceDto3>>
    {
        private readonly DataContext _context;
        private readonly IInvoiceHelperService _invoiceHelperService;

        public Handler(DataContext context, IInvoiceHelperService invoiceHelperService)
        {
            _context = context;
            _invoiceHelperService = invoiceHelperService;
        }

        public async Task<Result<InvoiceDto3>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var createdInvoice = await _invoiceHelperService.UpdateInvoice(request.InvoiceDto);

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                
                return Result<InvoiceDto3>.Success(createdInvoice);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result<InvoiceDto3>.Failure("Error updating Invoice");
            }
        }
    }
}