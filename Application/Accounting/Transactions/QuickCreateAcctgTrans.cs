using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.Transactions;

public class QuickCreateAcctgTrans
{
    public class Command : IRequest<Result<CreateAcctgTransAndEntriesResult>>
    {
        public CreateQuickAcctgTransAndEntriesParams CreateQuickAcctgTransAndEntriesParams { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.CreateQuickAcctgTransAndEntriesParams).SetValidator(new CreateAcctgTransValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<CreateAcctgTransAndEntriesResult>>
    {
        private readonly DataContext _context;
        private readonly IGeneralLedgerService _generalLedgerService;

        public Handler(DataContext context, IGeneralLedgerService generalLedgerService)
        {
            _context = context;
            _generalLedgerService = generalLedgerService;
        }

        public async Task<Result<CreateAcctgTransAndEntriesResult>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var acctgTransId =
                    await _generalLedgerService.QuickCreateAcctgTransAndEntries(
                        request.CreateQuickAcctgTransAndEntriesParams);

                
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);


                var transactionToReturn = new CreateAcctgTransAndEntriesResult
                {
                    AcctgTransEntries = new List<AcctgTransEntry>(),
                    AcctgTransId = acctgTransId,
                    GlFiscalTypeId = request.CreateQuickAcctgTransAndEntriesParams.GlFiscalTypeId,
                    AcctgTransTypeId = request.CreateQuickAcctgTransAndEntriesParams.AcctgTransTypeId,
                    InvoiceId = request.CreateQuickAcctgTransAndEntriesParams.InvoiceId,
                    PaymentId = request.CreateQuickAcctgTransAndEntriesParams.PaymentId,
                    PartyId = request.CreateQuickAcctgTransAndEntriesParams.PartyId,
                    ShipmentId = request.CreateQuickAcctgTransAndEntriesParams.ShipmentId,
                    RoleTypeId = request.CreateQuickAcctgTransAndEntriesParams.RoleTypeId,
                    TransactionDate = request.CreateQuickAcctgTransAndEntriesParams.TransactionDate
                };
                return Result<CreateAcctgTransAndEntriesResult>.Success(transactionToReturn);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result<CreateAcctgTransAndEntriesResult>.Failure("Error creating Transaction");
            }
        }
    }
}