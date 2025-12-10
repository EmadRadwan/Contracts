// Application/Accounting/Transactions/CreateInitialBalanceTrans.cs
using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.Transactions
{
    public class CreateInitialBalanceTrans
    {
        public class Command : IRequest<Result<CreateInitialBalanceTransResult>>
        {
            public CreateInitialBalanceTransParams CreateInitialBalanceTransParams { get; set; }
            public InitialBalanceEntryParams Entry { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Entry.GlAccountId).NotEmpty();
                RuleFor(x => x.Entry.Amount).GreaterThan(0);
            }
        }

        public class Handler : IRequestHandler<Command, Result<CreateInitialBalanceTransResult>>
        {
            private readonly DataContext _context;
            private readonly IAcctgTransService _acctgTransService;

            public Handler(DataContext context, IAcctgTransService acctgTransService)
            {
                _context = context;
                _acctgTransService = acctgTransService;
            }

            public async Task<Result<CreateInitialBalanceTransResult>> Handle(Command request, CancellationToken ct)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(ct);

                try
                {
                    var entry = request.Entry;
                    
                    // Create header
                    var headerParams = new CreateAcctgTransParams
                    {
                        AcctgTransTypeId = "OPENING_BALANCE",
                        TransactionDate = request.CreateInitialBalanceTransParams.TransactionDate,
                        Description = request.CreateInitialBalanceTransParams.HeaderDescription,
                        GlFiscalTypeId = "ACTUAL",
                        PartyId = entry.PartyId,
                        IsPosted = "Y",
                    };

                    var acctgTransId = await _acctgTransService.CreateAcctgTrans(headerParams);

                    // Create single entry
                    var acctgTransEntry = new AcctgTransEntry
                    {
                        AcctgTransId = acctgTransId,
                        AcctgTransEntrySeqId = "001",
                        GlAccountId = entry.GlAccountId,
                        DebitCreditFlag = entry.DebitCreditFlag,
                        Amount = entry.Amount,
                        PartyId = entry.PartyId,
                        Description = entry.Description,
                        OrganizationPartyId = request.CreateInitialBalanceTransParams.OrganizationPartyId,
                        AcctgTransEntryTypeId = "_NA_",
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow,
                    };

                    await _acctgTransService.CreateAcctgTransEntry(acctgTransEntry);
                    await _context.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    return Result<CreateInitialBalanceTransResult>
                        .Success(new CreateInitialBalanceTransResult { AcctgTransId = acctgTransId });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<CreateInitialBalanceTransResult>.Failure($"Error: {ex.Message}");
                }
            }
        }
    }

    public class CreateInitialBalanceTransParams
    {
        public string AcctgTransTypeId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string OrganizationPartyId { get; set; }
        public string HeaderDescription { get; set; }
        public string GlFiscalTypeId { get; set; }
        public string IsPosted { get; set; }
    }

    public class InitialBalanceEntryParams
    {
        public string GlAccountId { get; set; }
        public string PartyId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string DebitCreditFlag { get; set; } // "D" or "C"
    }

    public class CreateInitialBalanceTransResult
    {
        public string AcctgTransId { get; set; }
    }
}