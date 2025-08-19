using Application.Accounting.Services;



using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.Invoices;

public class CreateInvoice
{
    public class Command : IRequest<Result<InvoiceDto2>>
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

    public class Handler : IRequestHandler<Command, Result<InvoiceDto2>>
    {
        private readonly DataContext _context;
        private readonly IInvoiceHelperService _invoiceHelperService;

        public Handler(DataContext context, IInvoiceHelperService invoiceHelperService)
        {
            _context = context;
            _invoiceHelperService = invoiceHelperService;
        }

        public async Task<Result<InvoiceDto2>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var createdInvoice = await _invoiceHelperService.CreateInvoice(request.InvoiceDto);

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                
                var invoiceQuery = await (from inv in _context.Invoices
                    join pty in _context.Parties on inv.PartyIdFrom equals pty.PartyId
                    join ptyto in _context.Parties on inv.PartyId equals ptyto.PartyId
                    join invt in _context.InvoiceTypes on inv.InvoiceTypeId equals invt.InvoiceTypeId
                    join sts in _context.StatusItems on inv.StatusId equals sts.StatusId
                    join uom in _context.Uoms on inv.CurrencyUomId equals uom.UomId
                    where inv.InvoiceId == createdInvoice.InvoiceId 
                    select new
                    {
                        Invoice = inv,
                        PartyFrom = pty,
                        PartyTo = ptyto,
                        InvoiceType = invt,
                        Status = sts,
                        CurrencyUom = uom.UomId,
                    }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                InvoiceDto2 invoiceToReturn;
                
                // map the invoice to return from both createdInvoice and invoiceQuery
                invoiceToReturn = new InvoiceDto2
                {
                    InvoiceId = createdInvoice.InvoiceId,
                    InvoiceTypeId = createdInvoice.InvoiceTypeId,
                    InvoiceTypeDescription = invoiceQuery.InvoiceType.Description,
                    PartyIdFrom = new InvoicePartyDto
                    {
                        FromPartyId = invoiceQuery!.PartyFrom.PartyId,
                        FromPartyName = invoiceQuery!.PartyFrom.Description
                    },
                    FromPartyName = invoiceQuery?.PartyFrom.Description,
                    PartyId = new InvoicePartyDto
                    {
                        FromPartyId = invoiceQuery!.PartyTo.PartyId,
                        FromPartyName = invoiceQuery!.PartyTo.Description
                    },
                    ToPartyName = invoiceQuery?.PartyTo.Description,
                    RoleTypeId = createdInvoice.RoleTypeId,
                    StatusId = createdInvoice.StatusId,
                    StatusDescription = invoiceQuery!.Status.Description,
                    BillingAccountId = createdInvoice.BillingAccountId,
                    ContactMechId = createdInvoice.ContactMechId,
                    InvoiceDate = createdInvoice.InvoiceDate,
                    DueDate = createdInvoice.DueDate,
                    PaidDate = createdInvoice.PaidDate,
                    InvoiceMessage = createdInvoice.InvoiceMessage,
                    ReferenceNumber = createdInvoice.ReferenceNumber,
                    Description = createdInvoice.Description,
                    CurrencyUomId = createdInvoice.CurrencyUomId,
                    CurrencyUomName = createdInvoice.CurrencyUomName,
                    Total = createdInvoice.Total,
                    OutstandingAmount = createdInvoice.OutstandingAmount,
                    AllowSubmit = true,
                };
                
                
                return Result<InvoiceDto2>.Success(invoiceToReturn);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result<InvoiceDto2>.Failure("Error creating Invoice");
            }
        }
    }
}