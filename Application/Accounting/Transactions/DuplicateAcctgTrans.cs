using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Transactions
{
    public class DuplicateAcctgTrans
    {
        public class Command : IRequest<Result<DuplicateAcctgTransResult>>
        {
            public string AcctgTransId { get; set; } = string.Empty;
        }

        public class Handler : IRequestHandler<Command, Result<DuplicateAcctgTransResult>>
        {
            private readonly DataContext _context;
            private readonly IAcctgTransService _acctgTransService;


            public Handler(DataContext context, IAcctgTransService acctgTransService)
            {
                _context = context;
                _acctgTransService = acctgTransService;
            }

            public async Task<Result<DuplicateAcctgTransResult>> Handle(Command request, CancellationToken ct)
            {
                await using var tx = await _context.Database.BeginTransactionAsync(ct);
                try
                {
                    // Load original
                    var origHeader = await _context.AcctgTrans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.AcctgTransId == request.AcctgTransId, ct);

                    if (origHeader == null)
                        return Result<DuplicateAcctgTransResult>.Failure("Original not found");

                    var origEntries = await _context.AcctgTransEntries
                        .Where(e => e.AcctgTransId == request.AcctgTransId)
                        .ToListAsync(ct);

                    // Map to the same DTOs used by CreateMulti...
                    var acctgTransParams = new CreateAcctgTransParams
                    {
                        AcctgTransTypeId = origHeader.AcctgTransTypeId,
                        TransactionDate = DateTime.UtcNow.Date,
                        Description = origHeader.Description,
                        IsPosted = "N",
                        GlFiscalTypeId = origHeader.GlFiscalTypeId,
                        PartyId = origHeader.PartyId,
                    };
                    var acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);


                    var stamp = DateTime.UtcNow;
                    var maxSeqId = 0;
                    foreach (var entry in origEntries)
                    {
                        var entrySeqId = (++maxSeqId).ToString().PadLeft(3, '0');
                        var acctgTransEntry = new AcctgTransEntry
                        {
                            AcctgTransId = acctgTransId,
                            AcctgTransEntrySeqId = entrySeqId,
                            GlAccountId = entry.GlAccountId,
                            DebitCreditFlag = entry.DebitCreditFlag,
                            AcctgTransEntryTypeId = "_NA_",
                            Amount = entry.Amount,
                            ReconcileStatusId = "AES_NOT_RECONCILED",
                            Description = entry.Description, // Maintain entry-level description
                            OrganizationPartyId = entry.OrganizationPartyId,
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp,
                        };
                        await _acctgTransService.CreateAcctgTransEntry(acctgTransEntry);
                    }

                    await _context.SaveChangesAsync(ct);

                    await tx.CommitAsync(ct);

                    return Result<DuplicateAcctgTransResult>.Success(
                        new DuplicateAcctgTransResult { NewAcctgTransId = acctgTransId }
                    );
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync(ct);
                    return Result<DuplicateAcctgTransResult>.Failure($"Duplicate failed: {ex.Message}");
                }
            }
        }
    }

    public class DuplicateAcctgTransResult
    {
        public string NewAcctgTransId { get; set; } = string.Empty;
    }
}