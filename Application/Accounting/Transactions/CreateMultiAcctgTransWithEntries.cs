using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.Transactions
{
    public class CreateMultiAcctgTransWithEntries
    {
        public class Command : IRequest<Result<CreateMultiAcctgTransResult>>
        {
            public CreateMultiAcctgTransParams CreateMultiAcctgTransParams { get; set; }
            public List<MultiAcctgTransEntryParams> Entries { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Entries).NotEmpty().WithMessage("At least one entry is required.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<CreateMultiAcctgTransResult>>
        {
            private readonly DataContext _context;
            private readonly IAcctgTransService _acctgTransService;

            public Handler(DataContext context, IAcctgTransService acctgTransService)
            {
                _context = context;
                _acctgTransService = acctgTransService;
            }

            public async Task<Result<CreateMultiAcctgTransResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                // REFACTOR: Begin transaction to ensure atomicity for header and entries
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // REFACTOR: Reuse existing service to create transaction header with HeaderDescription
                    // Purpose: Map HeaderDescription to transaction header
                    // Improvement: Supports new header-level description field
                    var acctgTransParams = new CreateAcctgTransParams
                    {
                        AcctgTransTypeId = request.CreateMultiAcctgTransParams.AcctgTransTypeId,
                        TransactionDate = request.CreateMultiAcctgTransParams.TransactionDate,
                        IsPosted = request.CreateMultiAcctgTransParams.IsPosted,
                        Description = request.CreateMultiAcctgTransParams.HeaderDescription, // REFACTOR: Use HeaderDescription
                        GlFiscalTypeId = request.CreateMultiAcctgTransParams.GlFiscalTypeId,
                    };
                    var acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

                    // REFACTOR: Create entries using existing service method
                    var stamp = DateTime.UtcNow;
                    var maxSeqId = 0;
                    foreach (var entry in request.Entries)
                    {
                        var entrySeqId = (++maxSeqId).ToString().PadLeft(3, '0');
                        var acctgTransEntry = new AcctgTransEntry
                        {
                            AcctgTransId = acctgTransId,
                            AcctgTransEntrySeqId = entrySeqId,
                            GlAccountId = entry.DebitGlAccountId ?? entry.CreditGlAccountId,
                            DebitCreditFlag = entry.DebitCreditFlag,
                            AcctgTransEntryTypeId = "_NA_",
                            Amount = entry.Amount,
                            ReconcileStatusId = "AES_NOT_RECONCILED",
                            Description = entry.Description, // Maintain entry-level description
                            OrganizationPartyId = request.CreateMultiAcctgTransParams.OrganizationPartyId,
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp,
                        };
                        await _acctgTransService.CreateAcctgTransEntry(acctgTransEntry);
                    }

                    // REFACTOR: Removed trial balance validation
                    // Purpose: Eliminate debit/credit balance check to allow unbalanced transactions
                    // Improvement: Simplifies logic, reduces computation
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    // REFACTOR: Return result without IsBalanced
                    var result = new CreateMultiAcctgTransResult { AcctgTransId = acctgTransId };
                    return Result<CreateMultiAcctgTransResult>.Success(result);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<CreateMultiAcctgTransResult>.Failure($"Error creating transaction: {ex.Message}");
                }
            }
        }
    }

    public class CreateMultiAcctgTransResult
    {
        public string AcctgTransId { get; set; }
    }
}