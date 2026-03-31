public async Task<List<GlAccountBalance>> GetOpeningBalances(
    List<string> partyIds,
    List<string> glAccountClassIds,
    CustomTimePeriod lastClosedTimePeriod)
{
    if (lastClosedTimePeriod == null)
        return new List<GlAccountBalance>();

    var customTimePeriodId = lastClosedTimePeriod.CustomTimePeriodId;
    var periodFromDate = lastClosedTimePeriod.FromDate;

    // 1. First priority: Try to get opening balances from GlAccountHistory (as before)
    var rows = await (
        from glah in _context.GlAccountHistories
        join gla in _context.GlAccounts 
            on glah.GlAccountId equals gla.GlAccountId
        where partyIds.Contains(glah.OrganizationPartyId)
              && glAccountClassIds.Contains(gla.GlAccountClassId)
              && glah.EndingBalance != 0
              && glah.CustomTimePeriodId == customTimePeriodId
        select new
        {
            glah.GlAccountId,
            gla.AccountCode,
            gla.AccountName,
            glah.PostedDebits,
            glah.PostedCredits,
            glah.EndingBalance
        }
    ).ToListAsync();

    // If we found history records, return them
    if (rows.Any())
    {
        var resultList = new List<GlAccountBalance>();
        foreach (var r in rows)
        {
            var accountBalance = new GlAccountBalance
            {
                GlAccountId = r.GlAccountId,
                AccountCode = r.AccountCode,
                AccountName = r.AccountName,
                D = r.PostedDebits ?? 0,
                C = r.PostedCredits ?? 0,
                Balance = r.EndingBalance ?? 0
            };
            resultList.Add(accountBalance);
        }
        return resultList;
    }

    // 2. Fallback: No GlAccountHistory exists yet → Look for OPENING_BALANCE transactions
    //    before the start of this period (same logic used in Trial Balance)
    if (!periodFromDate.HasValue)
        return new List<GlAccountBalance>();

    var openingCutoff = periodFromDate.Value.Date.AddTicks(-1); // One day before period starts

    var openingTxQuery = 
        from ate in _context.AcctgTransEntries
        join act in _context.AcctgTrans 
            on ate.AcctgTransId equals act.AcctgTransId
        join gla in _context.GlAccounts 
            on ate.GlAccountId equals gla.GlAccountId
        where partyIds.Contains(ate.OrganizationPartyId)
              && glAccountClassIds.Contains(gla.GlAccountClassId)
              && act.IsPosted == "Y"
              && act.AcctgTransTypeId == "OPENING_BALANCE"
              && act.GlFiscalTypeId == "ACTUAL"                    // Usually "ACTUAL" for opening balances
              && act.TransactionDate <= openingCutoff
        group new { ate, gla } by new 
        { 
            ate.GlAccountId, 
            gla.AccountCode, 
            gla.AccountName 
        } into g
        select new
        {
            g.Key.GlAccountId,
            g.Key.AccountCode,
            g.Key.AccountName,
            TotalDebit = g.Sum(x => x.ate.DebitCreditFlag == "D" ? x.ate.Amount : 0),
            TotalCredit = g.Sum(x => x.ate.DebitCreditFlag == "C" ? x.ate.Amount : 0)
        };

    var openingTxRows = await openingTxQuery.ToListAsync();

    var resultFromOpening = new List<GlAccountBalance>();

    foreach (var row in openingTxRows)
    {
        // Determine if it's a debit or credit root account to calculate correct balance
        bool isDebitAccount = await _acctgMiscService.IsDebitAccount(row.GlAccountId); // Reuse your existing service

        decimal balance = isDebitAccount 
            ? (decimal)row.TotalDebit - (decimal)row.TotalCredit 
            : (decimal)row.TotalCredit - (decimal)row.TotalDebit;

        if (balance != 0)
        {
            resultFromOpening.Add(new GlAccountBalance
            {
                GlAccountId = row.GlAccountId,
                AccountCode = row.AccountCode,
                AccountName = row.AccountName,
                D = (decimal)row.TotalDebit,
                C = (decimal)row.TotalCredit,
                Balance = balance
            });
        }
    }

    return resultFromOpening.OrderBy(x => x.AccountCode).ToList();
}