public class DuplicateAcctgTransHandler : IRequestHandler<DuplicateAcctgTrans.Command, Result<DuplicateAcctgTransResult>>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;

    public DuplicateAcctgTransHandler(DataContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
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
                return Result.Failure<DuplicateAcctgTransResult>("Original not found");

            var origEntries = await _context.AcctgTransEntry
                .Where(e => e.AcctgTransId == request.AcctgTransId)
                .ToListAsync(ct);

            // Map to the same DTOs used by CreateMulti...
            var createParams = new CreateMultiAcctgTransParams
            {
                AcctgTransTypeId     = origHeader.AcctgTransTypeId,
                TransactionDate      = DateTime.UtcNow.Date,           // ← or origHeader.TransactionDate
                OrganizationPartyId  = origHeader.OrganizationPartyId,
                HeaderDescription    = origHeader.Description,
                Description          = origHeader.Description,         // or first entry desc
                IsPosted             = "N",
                GlFiscalTypeId       = origHeader.GlFiscalTypeId,
                PartyId              = origHeader.PartyId,
            };

            var entries = origEntries.Select(e => new MultiAcctgTransEntryParams
            {
                DebitGlAccountId   = e.DebitCreditFlag == "D" ? e.GlAccountId : null,
                CreditGlAccountId  = e.DebitCreditFlag == "C" ? e.GlAccountId : null,
                Amount             = e.Amount,
                Description        = e.Description ?? "",
                DebitCreditFlag    = e.DebitCreditFlag,
            }).ToList();

            var createCommand = new CreateMultiAcctgTransWithEntries.Command
            {
                CreateMultiAcctgTransParams = createParams,
                Entries = entries
            };

            var createResult = await _mediator.Send(createCommand, ct);

            await tx.CommitAsync(ct);

            return Result<DuplicateAcctgTransResult>.Success(
                new DuplicateAcctgTransResult { NewAcctgTransId = createResult.AcctgTransId }
            );
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return Result.Failure<DuplicateAcctgTransResult>($"Duplicate failed: {ex.Message}");
        }
    }
}