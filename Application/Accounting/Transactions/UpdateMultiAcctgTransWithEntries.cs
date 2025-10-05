using Application.Accounting.Accounting;
using Application.Accounting.Services;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Transactions
{
    public class UpdateMultiAcctgTransWithEntries
    {
        public class Command : IRequest<Result<UpdateMultiAcctgTransResult>>
        {
            public string AcctgTransId { get; set; }
            public UpdateMultiAcctgTransParams UpdateMultiAcctgTransParams { get; set; }
            public List<MultiAcctgTransEntryParams> Entries { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AcctgTransId).NotEmpty().WithMessage("Transaction ID is required.");
                RuleFor(x => x.Entries).NotEmpty().WithMessage("At least one entry is required.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<UpdateMultiAcctgTransResult>>
        {
            private readonly DataContext _context;
            private readonly IAcctgTransService _acctgTransService;

            public Handler(DataContext context, IAcctgTransService acctgTransService)
            {
                _context = context;
                _acctgTransService = acctgTransService;
            }

            public async Task<Result<UpdateMultiAcctgTransResult>> Handle(Command request,
                CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var existingTrans =
                        await _context.AcctgTrans.FindAsync(new object[] { request.AcctgTransId }, cancellationToken);
                    if (existingTrans == null)
                    {
                        return Result<UpdateMultiAcctgTransResult>.Failure("Transaction not found.");
                    }

                    var acctgTrans = new AcctgTran
                    {
                        AcctgTransId = request.AcctgTransId,
                        AcctgTransTypeId = request.UpdateMultiAcctgTransParams.AcctgTransTypeId,
                        TransactionDate = request.UpdateMultiAcctgTransParams.TransactionDate,
                        IsPosted = request.UpdateMultiAcctgTransParams.IsPosted,
                        Description = request.UpdateMultiAcctgTransParams.HeaderDescription,
                        GlFiscalTypeId = request.UpdateMultiAcctgTransParams.GlFiscalTypeId,
                        PartyId = request.UpdateMultiAcctgTransParams.OrganizationPartyId,
                        LastUpdatedStamp = DateTime.UtcNow
                    };
                    var updateMessages = await _acctgTransService.UpdateAcctgTrans(acctgTrans);
                    if (updateMessages.Any())
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<UpdateMultiAcctgTransResult>.Failure(string.Join("; ", updateMessages));
                    }

                    // Purpose: Identify entries to delete by comparing with request
                    // Improvement: Ensures deleted entries are removed from the database
                    var existingEntries = await _context.AcctgTransEntries
                        .Where(e => e.AcctgTransId == request.AcctgTransId)
                        .ToListAsync(cancellationToken);
                    var requestEntrySeqIds = request.Entries
                        .Where(e => !string.IsNullOrEmpty(e.AcctgTransEntrySeqId))
                        .Select(e => e.AcctgTransEntrySeqId)
                        .ToList();
                    var entriesToDelete = existingEntries
                        .Where(e => !requestEntrySeqIds.Contains(e.AcctgTransEntrySeqId))
                        .ToList();

                    // Delete removed entries
                    _context.AcctgTransEntries.RemoveRange(entriesToDelete);

                    // REFACTOR: Process entries (update existing, create new)
                    // Purpose: Handle both updates and creations in a single loop
                    // Improvement: Reduces code duplication and improves maintainability
                    var stamp = DateTime.UtcNow;
                    var maxSeqId = existingEntries.Any()
                        ? existingEntries.Max(e => int.Parse(e.AcctgTransEntrySeqId))
                        : 0;

                    foreach (var entry in request.Entries)
                    {
                        if (!string.IsNullOrEmpty(entry.AcctgTransEntrySeqId))
                        {
                            // Update existing entry
                            var existingEntry = existingEntries.FirstOrDefault(e =>
                                e.AcctgTransEntrySeqId == entry.AcctgTransEntrySeqId);
                            if (existingEntry != null)
                            {
                                existingEntry.GlAccountId = entry.DebitGlAccountId ?? entry.CreditGlAccountId;
                                existingEntry.DebitCreditFlag = entry.DebitCreditFlag;
                                existingEntry.Amount = entry.Amount;
                                existingEntry.Description = entry.Description;
                                existingEntry.LastUpdatedStamp = stamp;
                                _context.AcctgTransEntries.Update(existingEntry);
                            }
                        }
                        else
                        {
                            // Create new entry
                            var entrySeqId = (++maxSeqId).ToString().PadLeft(3, '0');
                            var acctgTransEntry = new AcctgTransEntry
                            {
                                AcctgTransId = request.AcctgTransId,
                                AcctgTransEntrySeqId = entrySeqId,
                                GlAccountId = entry.DebitGlAccountId ?? entry.CreditGlAccountId,
                                DebitCreditFlag = entry.DebitCreditFlag,
                                AcctgTransEntryTypeId = "_NA_",
                                Amount = entry.Amount,
                                ReconcileStatusId = "AES_NOT_RECONCILED",
                                Description = entry.Description,
                                OrganizationPartyId = request.UpdateMultiAcctgTransParams.OrganizationPartyId,
                                CreatedStamp = stamp,
                                LastUpdatedStamp = stamp,
                            };
                            await _acctgTransService.CreateAcctgTransEntry(acctgTransEntry);
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    // REFACTOR: Return result with transaction ID
                    // Purpose: Provide feedback to the frontend
                    // Improvement: Maintains consistency with create operation
                    var result = new UpdateMultiAcctgTransResult { AcctgTransId = request.AcctgTransId };
                    return Result<UpdateMultiAcctgTransResult>.Success(result);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<UpdateMultiAcctgTransResult>.Failure($"Error updating transaction: {ex.Message}");
                }
            }
        }
    }

    public class UpdateMultiAcctgTransParams
    {
        public string AcctgTransId { get; set; }
        public string AcctgTransTypeId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string OrganizationPartyId { get; set; }
        public string HeaderDescription { get; set; }
        public string IsPosted { get; set; }
        public string GlFiscalTypeId { get; set; }
    }

    public class UpdateMultiAcctgTransResult
    {
        public string AcctgTransId { get; set; }
    }
}