public async Task<IQueryable<AccountingTransactionRecord>> Handle(Query request, CancellationToken cancellationToken)
{
    // validation (unchanged)
    var validator = new QueryValidator();
    var validationResult = await validator.ValidateAsync(request, cancellationToken);
    if (!validationResult.IsValid)
    {
        throw new ValidationException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
    }

    // ────────────────────────────────────────────────────────────────
    // Aggregated amounts per AcctgTransId (using AMOUNT + DEBIT_CREDIT_FLAG)
    // ────────────────────────────────────────────────────────────────
    var entrySummaries = 
        from entry in _context.AcctgTransEntries
        group entry by entry.AcctgTransId into g
        select new
        {
            AcctgTransId = g.Key,
            TotalDebit   = g.Where(e => e.DebitCreditFlag == "D").Sum(e => e.Amount ?? 0m),
            TotalCredit  = g.Where(e => e.DebitCreditFlag == "C").Sum(e => e.Amount ?? 0m),
            NetAmount    = g.Sum(e => 
                e.DebitCreditFlag == "D" ?  (e.Amount ?? 0m) :
                e.DebitCreditFlag == "C" ? -(e.Amount ?? 0m) : 0m
            )
        };

    // ────────────────────────────────────────────────────────────────
    // Main query – one row per transaction + left join to summary
    // ────────────────────────────────────────────────────────────────
    var query = 
        from trans in _context.AcctgTrans

        join transType in _context.AcctgTransTypes 
            on trans.AcctgTransTypeId equals transType.AcctgTransTypeId

        // Left join → entry summary (always one row per trans or null)
        join summary in entrySummaries 
            on trans.AcctgTransId equals summary.AcctgTransId into sumGroup
        from summary in sumGroup.DefaultIfEmpty()

        // Minimal join chain just to filter by company (using entry-level ORGANIZATION_PARTY_ID)
        join entry in _context.AcctgTransEntries 
            on trans.AcctgTransId equals entry.AcctgTransId into entryGroup
        from entry in entryGroup.DefaultIfEmpty()   // ← left join

        // Company filter: include transaction if it has NO entries OR at least one entry belongs to the company
        where entry == null || entry.OrganizationPartyId == request.CompanyId

        // Optional joins (your original ones – unchanged)
        join cert in _context.WorkEfforts 
            on new { WorkEffortId = trans.WorkEffortId, Type = "PROJECT_CERTIFICATE" } 
            equals new { cert.WorkEffortId, Type = cert.WorkEffortTypeId } into certGroup
        from certificate in certGroup.DefaultIfEmpty()

        join proj in _context.WorkEfforts 
            on new 
            { 
                ProjectId = certificate != null ? certificate.ProjectId : trans.WorkEffortId, 
                Type = "PROJECT" 
            } 
            equals new { proj.WorkEffortId, Type = proj.WorkEffortTypeId } into projGroup
        from project in projGroup.DefaultIfEmpty()

        join party in _context.Parties 
            on trans.PartyId equals party.PartyId into partyGroup
        from p in partyGroup.DefaultIfEmpty()

        select new AccountingTransactionRecord
        {
            AcctgTransId              = trans.AcctgTransId,
            AcctgTransTypeId          = trans.AcctgTransTypeId,
            AcctgTransTypeDescription = transType.Description,
            PartyId                   = trans.PartyId,
            PartyName                 = p != null ? p.Description : null,
            PaymentId                 = trans.PaymentId,
            TransactionDate           = trans.TransactionDate,
            IsPosted                  = trans.IsPosted,
            PostedDate                = trans.PostedDate,
            Description               = trans.Description,
            InvoiceId                 = trans.InvoiceId,
            WorkEffortId              = trans.WorkEffortId,
            ShipmentId                = trans.ShipmentId,
            CertificateNumber         = certificate?.CertificateNumber,
            ProjectNumber             = project?.WorkEffortId,
            ProjectName               = project?.ProjectName,
            SalesRequestId            = trans.SalesRequestId,
            CreatedStamp              = trans.CreatedStamp,

            // ───────────── New fields based on real structure ─────────────
            DebitTotal   = summary != null ? summary.TotalDebit   : 0m,
            CreditTotal  = summary != null ? summary.TotalCredit  : 0m,
            NetAmount    = summary != null ? summary.NetAmount    : 0m,

            // Alternative popular option (one column + sign):
            // TransactionAmount = summary != null ? summary.NetAmount : 0m,
        };

    return query.AsQueryable();
}