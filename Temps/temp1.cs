public async Task<IncomeStatementResult> PrepareIncomeStatement(
    string organizationPartyId,
    DateTime fromDate,
    DateTime thruDate,
    string glFiscalTypeId)
{
    try
    {
        if (string.IsNullOrEmpty(organizationPartyId))
            throw new ArgumentException("organizationPartyId is required.");
        if (string.IsNullOrEmpty(glFiscalTypeId))
            throw new ArgumentException("glFiscalTypeId is required.");

        var result = new IncomeStatementResult
        {
            TotalNetIncome = 0m,
            GlAccountTotalsMap = new GlAccountTotalsMap
            {
                Income = new List<GlAccountTotal>(),
                Expenses = new List<GlAccountTotal>()
            }
        };

        // Step 1: Get descendant classes (same as OFBiz)
        var expenseClasses = await GetDescendantGlAccountClassIds("EXPENSE");
        var revenueClasses = await GetDescendantGlAccountClassIds("REVENUE");
        var incomeClasses  = await GetDescendantGlAccountClassIds("INCOME");

        var partyIds = await GetAssociatedPartyIdsByRelationshipType(organizationPartyId, "GROUP_ROLLUP");
        if (!partyIds.Contains(organizationPartyId))
            partyIds.Add(organizationPartyId);

        // Step 2: Main query - Get all relevant posted transactions (matches OFBiz view entity logic)
        var rawEntries = await (
            from ate in _context.AcctgTransEntries
            join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
            join gla in _context.GlAccounts on ate.GlAccountId equals gla.GlAccountId
            where partyIds.Contains(ate.OrganizationPartyId)
                  && act.IsPosted == "Y"
                  && act.GlFiscalTypeId == glFiscalTypeId
                  && act.AcctgTransTypeId != "PERIOD_CLOSING"
                  && act.TransactionDate >= fromDate
                  && act.TransactionDate < thruDate
                  && (expenseClasses.Contains(gla.GlAccountClassId) ||
                      revenueClasses.Contains(gla.GlAccountClassId) ||
                      incomeClasses.Contains(gla.GlAccountClassId))
            orderby act.AcctgTransId, ate.AcctgTransEntrySeqId
            select new
            {
                ate.GlAccountId,
                ate.DebitCreditFlag,
                Amount = ate.Amount,
                gla.AccountCode,
                gla.AccountNameArabic,
                GlAccountClassId = gla.GlAccountClassId
            }).ToListAsync();

        // Step 3: Process each entry with proper sign logic (Most critical part - matches OFBiz exactly)
        decimal totalNetIncome = 0m;

        var expenseMap = new Dictionary<string, decimal>();   // glAccountId -> total
        var profitMap  = new Dictionary<string, decimal>();

        foreach (var row in rawEntries)
        {
            decimal amount = row.Amount;

            // Determine account nature (this replaces the OFBiz UtilAccounting calls)
            bool isExpense = expenseClasses.Contains(row.GlAccountClassId);
            bool isCreditAccount = revenueClasses.Contains(row.GlAccountClassId) || 
                                   incomeClasses.Contains(row.GlAccountClassId);
            bool isDebitAccount = !isCreditAccount && !isExpense;

            // === Sign Flipping Logic (Exact equivalent of OFBiz) ===
            // If Debit on Credit account OR Credit on Debit account → negate
            if ((row.DebitCreditFlag == "D" && isCreditAccount) ||
                (row.DebitCreditFlag == "C" && isDebitAccount))
            {
                amount = -amount;
            }

            // If it's an Expense account → negate again
            if (isExpense)
            {
                amount = -amount;
            }

            totalNetIncome += amount;

            // Add to correct map
            var targetMap = isExpense ? expenseMap : profitMap;

            if (!targetMap.ContainsKey(row.GlAccountId))
                targetMap[row.GlAccountId] = 0m;

            targetMap[row.GlAccountId] += amount;
        }

        // Step 4: Get Current Fiscal Period totals for each account (matches OFBiz second loop)
        var customTimePeriodStartDate = fromDate;   // We can improve this later if needed
        var customTimePeriodEndDate = thruDate;

        // Build Income list
        foreach (var kvp in profitMap)
        {
            var (debitTotal, creditTotal) = await GetAcctgTransEntriesAndTransTotal(
                organizationPartyId, kvp.Key, "Y", customTimePeriodStartDate, customTimePeriodEndDate);

            var totalOfCurrentFiscalPeriod = debitTotal - creditTotal;

            result.GlAccountTotalsMap.Income.Add(new GlAccountTotal
            {
                GlAccountId = kvp.Key,
                TotalAmount = kvp.Value,
                TotalOfCurrentFiscalPeriod = totalOfCurrentFiscalPeriod,
                AccountCode = rawEntries.FirstOrDefault(x => x.GlAccountId == kvp.Key)?.AccountCode ?? "",
                AccountName = rawEntries.FirstOrDefault(x => x.GlAccountId == kvp.Key)?.AccountNameArabic ?? ""
            });
        }

        // Build Expense list
        foreach (var kvp in expenseMap)
        {
            var (debitTotal, creditTotal) = await GetAcctgTransEntriesAndTransTotal(
                organizationPartyId, kvp.Key, "Y", customTimePeriodStartDate, customTimePeriodEndDate);

            var totalOfCurrentFiscalPeriod = debitTotal - creditTotal;

            result.GlAccountTotalsMap.Expenses.Add(new GlAccountTotal
            {
                GlAccountId = kvp.Key,
                TotalAmount = kvp.Value,
                TotalOfCurrentFiscalPeriod = totalOfCurrentFiscalPeriod,
                AccountCode = rawEntries.FirstOrDefault(x => x.GlAccountId == kvp.Key)?.AccountCode ?? "",
                AccountName = rawEntries.FirstOrDefault(x => x.GlAccountId == kvp.Key)?.AccountNameArabic ?? ""
            });
        }

        result.TotalNetIncome = totalNetIncome;

        return result;
    }
    catch (Exception ex)
    {
        throw new Exception("Error in PrepareIncomeStatement", ex);
    }
}