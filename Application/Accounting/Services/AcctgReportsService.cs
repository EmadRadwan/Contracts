using Application.Accounting.OrganizationGlSettings;
using Application.Shipments.Reports;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Services;

public interface IAcctgReportsService
{
    Task<TrialBalanceContext> ComputeTrialBalance(string customTimePeriodId, string organizationPartyId);

    Task<TransactionTotalsViewModel> GetTransactionTotals(string organizationPartyId, DateTime? fromDate,
        DateTime? thruDate, string glFiscalTypeId = "ACTUAL", int? selectedMonth = null);

    Task<IncomeStatementViewModel> GenerateIncomeStatement(string organizationPartyId, DateTime? fromDate,
        DateTime? thruDate, string glFiscalTypeId, int? selectedMonth = null);

    Task<GlAccountTrialBalanceResult> GenerateGlAccountTrialBalance(string organizationPartyId,
        string glAccountId,
        string timePeriodId,
        string isPosted);

    Task<InventoryValuationContext> ComputeInventoryValuation(
        string? organizationPartyId,
        string? facilityId,
        string? productId,
        DateTime? thruDate);

    Task<CashFlowStatementViewModel> GenerateCashFlowStatement(
        string organizationPartyId,
        DateTime? fromDate,
        DateTime? thruDate,
        string glFiscalTypeId, int? selectedMonth = null);

    Task<BalanceSheetViewModel> GenerateBalanceSheet(
        string organizationPartyId,
        DateTime? thruDate,
        string glFiscalTypeId);

    Task<ComparativeBalanceSheetResult> GenerateComparativeBalanceSheet(
        string organizationPartyId,
        DateTime? period1ThruDate,
        string period1GlFiscalTypeId,
        DateTime? period2ThruDate,
        string period2GlFiscalTypeId);

    Task<LastClosedTimePeriodResult> FindLastClosedDate(
        string organizationPartyId,
        DateTime? findDate,
        string? periodTypeId);

    Task<List<string>> GetDescendantGlAccountClassIds(string glAccountClassId);
}

public class AcctgReportsService : IAcctgReportsService
{
    private readonly DataContext _context;
    private readonly IAcctgMiscService _acctgMiscService;
    private readonly IGeneralLedgerService _generalLedgerService;

    public AcctgReportsService(DataContext context, IAcctgMiscService acctgMiscService,
        IGeneralLedgerService generalLedgerService)
    {
        _context = context;
        _acctgMiscService = acctgMiscService;
        _generalLedgerService = generalLedgerService;
    }


    /// <summary>
    /// This C# method mirrors the OFBiz Groovy script logic for computing a trial balance
    /// for a single party (identified by organizationPartyId) and a specific CustomTimePeriod.
    /// 
    /// Comparison to OFBiz Script (excerpts shown below) and Explanation:
    /// ----------------------------------------------------------------------
    /// OFBiz Groovy Script Key Points:
    ///   1. Reads the customTimePeriodId from parameters, fetches CustomTimePeriod.
    ///   2. Gathers a list of GlAccountOrganization records (via exprList conditions):
    ///      - organizationPartyId in partyIds
    ///      - fromDate < customTimePeriod.thruDate
    ///      - (thruDate >= customTimePeriod.fromDate or thruDate == null)
    ///   3. Iterates over those GlAccount entries and calls a service named 
    ///      'computeGlAccountBalanceForTimePeriod'.
    ///   4. Accumulates postedDebits and postedCredits totals in the context.
    ///   5. Returns these totals plus the list of computed account balances.
    /// 
    /// C# Equivalent (this method):
    ///   1. Accepts customTimePeriodId and organizationPartyId instead of a list of parties.
    ///   2. Queries the CustomTimePeriod from the database. If not found, throws an exception.
    ///   3. Retrieves all GlAccountOrganization records for that organizationPartyId
    ///      where the fromDate is <= customTimePeriod.ThruDate 
    ///      and (thruDate >= customTimePeriod.FromDate or null).
    ///   4. For each matching GlAccount, calls a function 'ComputeGlAccountBalanceForTimePeriod'
    ///      (similar to the OFBiz service call).
    ///   5. Checks if posted debits/credits are non-zero, then accumulates them into a total
    ///      for the final TrialBalanceContext.
    ///   6. Returns a context object with the total posted debits, total posted credits, 
    ///      and a list of AccountBalance results.
    /// 
    /// Business/Accounting Explanation:
    ///   - Trial Balance is used to verify that total debits and total credits match 
    ///     for an organization over a specific period. 
    ///   - Each GL account is analyzed for its posted (final) transactions, 
    ///     determining the net effect of debits vs. credits. 
    ///   - Summing across all relevant accounts yields an overall debit/credit snapshot. 
    ///   - If the books are balanced, postedDebitsTotal should equal postedCreditsTotal 
    ///     in a well-formed set of accounting entries.
    /// </summary>
    /// <param name="customTimePeriodId">
    ///   The ID of the financial period (e.g., quarter, month, year).
    ///   Must match an existing record in the <c>CustomTimePeriods</c> table.
    /// </param>
    /// <param name="organizationPartyId">
    ///   The single party (organization) for which we want to compute the trial balance.
    ///   This replaces the broader 'partyIds' approach from OFBiz that handles multiple divisions.
    /// </param>
    /// <returns>
    ///   A <see cref="TrialBalanceContext"/> that contains:
    ///   <list type="bullet">
    ///     <item><description>PostedDebitsTotal: The sum of all posted debit amounts in the GL accounts.</description></item>
    ///     <item><description>PostedCreditsTotal: The sum of all posted credit amounts in the GL accounts.</description></item>
    ///     <item><description>AccountBalances: A list of per-account computed balances (opening, posted debits, posted credits, ending).</description></item>
    ///   </list>
    /// </returns>
    public async Task<TrialBalanceContext> ComputeTrialBalance(string customTimePeriodId, string organizationPartyId)
    {
        var context = new TrialBalanceContext();

        try
        {
            if (!string.IsNullOrEmpty(customTimePeriodId))
            {
                // 1. Fetch the CustomTimePeriod entity based on the provided customTimePeriodId
                var customTimePeriod = await _context.CustomTimePeriods
                    .FindAsync(customTimePeriodId);

                if (customTimePeriod == null)
                {
                    // If no record is found, throw an exception (mirroring OFBiz logic that checks the record).
                    throw new Exception("CustomTimePeriod not found.");
                }

                // 2. Retrieve the matching GL accounts for the specified organizationPartyId.
                //    Conditions (mirroring OFBiz):
                //       - organizationPartyId == input param
                //       - fromDate <= customTimePeriod.ThruDate
                //       - (thruDate >= customTimePeriod.FromDate || gao.ThruDate == null)
                var organizationGlAccounts = await _context.GlAccountOrganizations
                    .Include(gao => gao.GlAccount)
                    .Where(gao =>
                        gao.OrganizationPartyId == organizationPartyId &&
                        gao.FromDate <= customTimePeriod.ThruDate &&
                        (gao.ThruDate >= customTimePeriod.FromDate || gao.ThruDate == null))
                    .OrderBy(gao => gao.GlAccount.AccountCode)
                    .ToListAsync();

                var accountBalances = new List<AccountBalance>();
                decimal postedDebitsTotal = 0;
                decimal postedCreditsTotal = 0;

                // 3. For each GlAccountOrganization record, compute the account balance 
                //    using our equivalent of the 'computeGlAccountBalanceForTimePeriod' service.
                foreach (var organizationGlAccount in organizationGlAccounts)
                {
                    var accountBalance = await ComputeGlAccountBalanceForTimePeriod(
                        organizationGlAccount.OrganizationPartyId,
                        customTimePeriod.CustomTimePeriodId,
                        organizationGlAccount.GlAccountId
                    );

                    // Only accumulate if there are real posted debits or credits.
                    if (accountBalance.PostedDebits != 0 || accountBalance.PostedCredits != 0)
                    {
                        var balance = new AccountBalance
                        {
                            GlAccountId = organizationGlAccount.GlAccountId,
                            AccountCode = organizationGlAccount.GlAccount.AccountCode,
                            AccountName = organizationGlAccount.GlAccount.AccountName,
                            OpeningBalance = accountBalance.OpeningBalance,
                            PostedDebits = accountBalance.PostedDebits,
                            PostedCredits = accountBalance.PostedCredits,
                            EndingBalance = accountBalance.EndingBalance
                        };

                        // 4. Accumulate totals for the entire Trial Balance.
                        postedDebitsTotal += accountBalance.PostedDebits;
                        postedCreditsTotal += accountBalance.PostedCredits;

                        accountBalances.Add(balance);
                    }
                }

                // 5. Set totals and list of balances in the returned context,
                //    mimicking the OFBiz approach of storing these in "context".
                context.PostedDebitsTotal = postedDebitsTotal;
                context.PostedCreditsTotal = postedCreditsTotal;
                context.AccountBalances = accountBalances;
            }
        }
        catch (Exception ex)
        {
            // Log or handle the exception as needed.
            // In a real scenario, you'd likely have a logging framework.
            throw new Exception("An error occurred while computing the trial balance.", ex);
        }

        return context;
    }

    /// <summary>
    /// Calculates the opening balance, ending balance, total debits, and total credits for a given GL account 
    /// within a specified CustomTimePeriod. This mirrors the OFBiz simple-method named 
    /// "computeGlAccountBalanceForTimePeriod" from an accounting standpoint.
    /// </summary>
    /// <param name="organizationPartyId">
    /// The unique identifier for the organization for which we are computing the GL account balances. 
    /// In accounting, each organization (or legal entity) might have its own ledger.
    /// </param>
    /// <param name="customTimePeriodId">
    /// The ID of the CustomTimePeriod representing the financial reporting period (e.g., a fiscal month, quarter, or year).
    /// In accounting, this is the timeframe during which financial transactions are summarized.
    /// </param>
    /// <param name="glAccountId">
    /// The unique identifier of the GL (General Ledger) account for which balances and transactions are being computed.
    /// </param>
    /// <returns>
    /// A <see cref="GlAccountBalanceResult"/> object containing:
    ///   - <c>OpeningBalance</c>: The account's balance at the start of the reporting period.
    ///   - <c>EndingBalance</c>: The account's balance at the end of the reporting period.
    ///   - <c>PostedDebits</c>: The sum of all posted debits within the period.
    ///   - <c>PostedCredits</c>: The sum of all posted credits within the period.
    /// </returns>
    /// <remarks>
    /// From an accounting perspective:
    /// 1. "Debits" and "Credits" are the two fundamental sides of every financial transaction. 
    ///    - A "Debit" entry increases the balance of certain types of accounts (e.g., Assets, Expenses) 
    ///      and decreases others (e.g., Liabilities, Equity).
    ///    - A "Credit" entry does the opposite.
    /// 2. A "Debit account" (e.g., an Asset account) typically has a positive balance if its debits exceed its credits.
    ///    A "Credit account" (e.g., a Liability or Income account) typically shows a positive balance if its credits exceed its debits.
    /// 3. The difference between debits and credits up to the start (opening) and end (closing) of a time period 
    ///    determines the OpeningBalance and EndingBalance for that period.
    /// 4. The "IsPosted" flag ensures we only include transactions that are finalized and not provisional.
    /// 5. The "GlFiscalTypeId" of "ACTUAL" indicates we only consider real transactions (versus budgets or forecasts).
    /// 6. <c>TransactionDate &lt; customTimePeriod.FromDate</c> means transactions happening before the start of the period 
    ///    are counted towards the opening balance, while <c>TransactionDate &lt; customTimePeriod.ThruDate</c> 
    ///    means transactions within the period up to the end date.
    /// </remarks>
    public async Task<GlAccountBalanceResult> ComputeGlAccountBalanceForTimePeriod(
        string organizationPartyId,
        string customTimePeriodId,
        string glAccountId)
    {
        try
        {
            // 1. Retrieve the relevant CustomTimePeriod (the date range for our accounting period).
            var customTimePeriod = await _context.CustomTimePeriods
                .FindAsync(customTimePeriodId);

            // 2. Retrieve the GL Account entity, which holds metadata like account type (asset, liability, etc.).
            var glAccount = await _context.GlAccounts
                .FindAsync(glAccountId);

            // Safety checks to ensure both the period and the GL account exist in the database.
            if (customTimePeriod == null)
            {
                throw new Exception("CustomTimePeriod not found.");
            }

            if (glAccount == null)
            {
                throw new Exception("GlAccount not found.");
            }

            // --------------------------
            // COMPUTE DEBITS AND CREDITS
            // --------------------------

            // totalDebitsToOpeningDate: Sum of all debit entries for this GL account 
            // that happened before the start (FromDate) of our CustomTimePeriod.
            decimal totalDebitsToOpeningDate = (decimal)await (from ate in _context.AcctgTransEntries
                join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                where ate.OrganizationPartyId == organizationPartyId
                      && ate.GlAccountId == glAccountId
                      && act.IsPosted == "Y" // Only posted (finalized) transactions
                      && ate.DebitCreditFlag == "D" // Debits only
                      && act.GlFiscalTypeId == "ACTUAL" // Real (not budget) transactions
                      && act.TransactionDate < customTimePeriod.FromDate
                select ate.Amount).SumAsync();

            // totalDebitsToEndingDate: Sum of all debit entries for this GL account 
            // that happened before the ThruDate of the CustomTimePeriod (i.e., up to the end).
            decimal totalDebitsToEndingDate = (decimal)await (from ate in _context.AcctgTransEntries
                join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                where ate.OrganizationPartyId == organizationPartyId
                      && ate.GlAccountId == glAccountId
                      && act.IsPosted == "Y"
                      && ate.DebitCreditFlag == "D"
                      && act.GlFiscalTypeId == "ACTUAL"
                      && act.TransactionDate < customTimePeriod.ThruDate
                select ate.Amount).SumAsync();

            // totalCreditsToOpeningDate: Sum of all credit entries for this GL account
            // that happened before the start (FromDate) of our CustomTimePeriod.
            decimal totalCreditsToOpeningDate = (decimal)await (from ate in _context.AcctgTransEntries
                join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                where ate.OrganizationPartyId == organizationPartyId
                      && ate.GlAccountId == glAccountId
                      && act.IsPosted == "Y"
                      && ate.DebitCreditFlag == "C"
                      && act.GlFiscalTypeId == "ACTUAL"
                      && act.TransactionDate < customTimePeriod.FromDate
                select ate.Amount).SumAsync();

            // totalCreditsToEndingDate: Sum of all credit entries for this GL account
            // that happened before the ThruDate of our CustomTimePeriod (up to the end).
            decimal totalCreditsToEndingDate = (decimal)await (from ate in _context.AcctgTransEntries
                join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                where ate.OrganizationPartyId == organizationPartyId
                      && ate.GlAccountId == glAccountId
                      && act.IsPosted == "Y"
                      && ate.DebitCreditFlag == "C"
                      && act.GlFiscalTypeId == "ACTUAL"
                      && act.TransactionDate < customTimePeriod.ThruDate
                select ate.Amount).SumAsync();

            // ------------------------------------
            // CALCULATE THE PERIOD'S DEBITS/CREDITS
            // ------------------------------------

            // The difference between the debits at the end of the period and the debits at the start 
            // gives us the net debits posted during the CustomTimePeriod.
            decimal totalDebitsInTimePeriod = totalDebitsToEndingDate - totalDebitsToOpeningDate;
            // Same logic for credits in the time period.
            decimal totalCreditsInTimePeriod = totalCreditsToEndingDate - totalCreditsToOpeningDate;

            // --------------------------------
            // DETERMINE IF THIS IS A DEBIT ACCT
            // --------------------------------
            // From an accounting standpoint:
            //   - Debit accounts have positive balances when debits exceed credits (e.g., Assets, Expenses).
            //   - Credit accounts have positive balances when credits exceed debits (e.g., Liabilities, Income).
            bool isDebit = await _acctgMiscService.IsDebitAccount(glAccount.GlAccountId);

            // ---------------------------------------------
            // COMPUTE THE OPENING BALANCE AND ENDING BALANCE
            // ---------------------------------------------
            decimal openingBalance;
            decimal endingBalance;

            if (isDebit)
            {
                // For debit-based accounts, the balance is Debits - Credits.
                openingBalance = totalDebitsToOpeningDate - totalCreditsToOpeningDate;
                endingBalance = totalDebitsToEndingDate - totalCreditsToEndingDate;
            }
            else
            {
                // For credit-based accounts, the balance is Credits - Debits.
                openingBalance = totalCreditsToOpeningDate - totalDebitsToOpeningDate;
                endingBalance = totalCreditsToEndingDate - totalDebitsToEndingDate;
            }

            // ---------------------------------------------
            // RETURN THE RESULTS AS A STRONGLY-TYPED OBJECT
            // ---------------------------------------------
            return new GlAccountBalanceResult
            {
                OpeningBalance = openingBalance,
                PostedDebits = totalDebitsInTimePeriod,
                PostedCredits = totalCreditsInTimePeriod,
                EndingBalance = endingBalance
            };
        }
        catch (Exception ex)
        {
            // In a production environment, you might log the error with a logging framework
            Console.WriteLine($"Error in ComputeGlAccountBalanceForTimePeriod: {ex.Message}");
            throw; // rethrow to propagate the exception up the call stack
        }
    }

    /// <summary>
    /// Main entry method that mirrors the OFBiz logic:
    /// 1) If fromDate is not supplied, attempts to find last closed date. Otherwise uses fallback.
    /// 2) If thruDate is null, defaults to now.
    /// 3) Retrieves posted, unposted, and all totals, including opening balances.
    /// </summary>
    public async Task<TransactionTotalsViewModel> GetTransactionTotals(
        string organizationPartyId,
        DateTime? fromDate,
        DateTime? thruDate,
        string glFiscalTypeId = "ACTUAL",
        int? selectedMonth = null)
    {
        try
        {
            // Fetch associated parties; your existing logic
            var partyIds = await GetAssociatedPartyIdsByRelationshipType(organizationPartyId, "GROUP_ROLLUP");
            // Ensure the main org is included
            if (!partyIds.Contains(organizationPartyId))
            {
                partyIds.Add(organizationPartyId);
            }

            // Retrieve base currency (not essential to totals, but presumably used in your real code)
            var partyAcctgPreference = await _acctgMiscService.GetPartyAccountingPreferences(organizationPartyId);
            var currencyUomId = partyAcctgPreference?.BaseCurrencyUomId;

            var now = DateTime.Now;

            // If fromDate is not specified, attempt to find last closed date or fallback to (now - 1 month)
            if (!fromDate.HasValue)
            {
                var lastClosedDateResult = await FindLastClosedDate(organizationPartyId, null, null);
                if (lastClosedDateResult?.LastClosedDate != null)
                {
                    fromDate = lastClosedDateResult.LastClosedDate;
                }
                else
                {
                    fromDate = now.AddMonths(-1);
                }
            }

            // If user selected a month, override fromDate & thruDate
            if (selectedMonth.HasValue)
            {
                var selectedMonthDate = new DateTime(now.Year, selectedMonth.Value + 1, 1);
                fromDate = selectedMonthDate;
                thruDate = selectedMonthDate.AddMonths(1).AddDays(-1);
            }

            // If thruDate is still null, default to now
            if (!thruDate.HasValue)
            {
                thruDate = now;
            }

            // ------------------------------------------------
            // 1) Posted Totals
            // ------------------------------------------------
            var postedTotals = await GetTransactionTotalsByPostingStatus(
                organizationPartyId, partyIds,
                fromDate.Value, thruDate.Value,
                glFiscalTypeId, isPosted: true);

            // ------------------------------------------------
            // 2) Unposted Totals
            // ------------------------------------------------
            var unpostedTotals = await GetTransactionTotalsByPostingStatus(
                organizationPartyId, partyIds,
                fromDate.Value, thruDate.Value,
                glFiscalTypeId, isPosted: false);

            // ------------------------------------------------
            // 3) All Totals (Posted + Unposted)
            // ------------------------------------------------
            var allTotals = await GetAllTransactionTotals(
                organizationPartyId, partyIds,
                fromDate.Value, thruDate.Value,
                glFiscalTypeId);

            // Return aggregated view
            return new TransactionTotalsViewModel
            {
                FromDate = fromDate,
                ThruDate = thruDate,
                GlFiscalTypeId = glFiscalTypeId,
                OrganizationPartyId = organizationPartyId,
                PostedTransactionTotals = postedTotals,
                UnpostedTransactionTotals = unpostedTotals,
                AllTransactionTotals = allTotals
            };
        }
        catch (Exception ex)
        {
            // Handle or log as needed
            throw new Exception("Error in GetTransactionTotals", ex);
        }
    }

    //// <summary>
    /// Fetch associated partyIds for a given relationship type (e.g. "GROUP_ROLLUP").
    ///
    /// Business Explanation:
    /// Some organizations have multiple divisions or subsidiaries rolled up under them.
    /// We gather those child parties so that the income statement includes all related entities.
    /// </summary>
    private async Task<List<string>> GetAssociatedPartyIdsByRelationshipType(string partyIdFrom,
        string relationshipTypeId)
    {
        var partyIds = new List<string>();

        // Start by fetching direct children
        var initialParties = await _context.PartyRelationships
            .Where(pr => pr.PartyIdFrom == partyIdFrom && pr.PartyRelationshipTypeId == relationshipTypeId)
            .Select(pr => pr.PartyIdTo)
            .ToListAsync();

        // Use a queue for BFS or DFS
        var queue = new Queue<string>(initialParties);

        while (queue.Count > 0)
        {
            var currentPartyId = queue.Dequeue();
            if (!partyIds.Contains(currentPartyId))
            {
                partyIds.Add(currentPartyId);
                // Add next-level children
                var childParties = await _context.PartyRelationships
                    .Where(pr => pr.PartyIdFrom == currentPartyId && pr.PartyRelationshipTypeId == relationshipTypeId)
                    .Select(pr => pr.PartyIdTo)
                    .ToListAsync();
                foreach (var cp in childParties)
                {
                    if (!partyIds.Contains(cp))
                    {
                        queue.Enqueue(cp);
                    }
                }
            }
        }

        return partyIds.Distinct().ToList();
    }

    /// <summary>
    /// Recursively fetches child party IDs.
    /// </summary>
    private async Task FetchChildPartyIds(string partyIdFrom, string partyRelationshipTypeId, List<string> partyIds)
    {
        var childParties = await _context.PartyRelationships
            .Where(pr => pr.PartyIdFrom == partyIdFrom && pr.PartyRelationshipTypeId == partyRelationshipTypeId)
            .Select(pr => pr.PartyIdTo)
            .ToListAsync();

        foreach (var childPartyId in childParties)
        {
            if (!partyIds.Contains(childPartyId))
            {
                partyIds.Add(childPartyId);
                await FetchChildPartyIds(childPartyId, partyRelationshipTypeId, partyIds);
            }
        }
    }

    /// <summary>
    /// Mirrors OFBiz findLastClosedDate. 
    /// Returns the last closed time period on or before findDate (if specified), 
    /// else earliest fromDate if none closed.
    /// </summary>
    public async Task<LastClosedTimePeriodResult> FindLastClosedDate(
        string organizationPartyId,
        DateTime? findDate,
        string? periodTypeId)
    {
        try
        {
            // Default findDate to now if not provided
            if (!findDate.HasValue)
            {
                findDate = DateTime.Now;
            }

            DateTime? lastClosedDate = null;
            CustomTimePeriod lastClosedTimePeriod = null;

            // Step 1: Attempt to find the most recent closed period before `findDate`
            var closedTimePeriodQuery = _context.CustomTimePeriods
                .Where(tp => tp.OrganizationPartyId == organizationPartyId
                             && tp.ThruDate <= findDate
                             && tp.IsClosed == "Y");

            if (!string.IsNullOrEmpty(periodTypeId))
            {
                closedTimePeriodQuery = closedTimePeriodQuery.Where(tp => tp.PeriodTypeId == periodTypeId);
            }

            var closedTimePeriod = await closedTimePeriodQuery
                .OrderByDescending(tp => tp.ThruDate)
                .FirstOrDefaultAsync();

            if (closedTimePeriod != null)
            {
                // Found a closed period
                lastClosedTimePeriod = new CustomTimePeriod
                {
                    CustomTimePeriodId = closedTimePeriod.CustomTimePeriodId,
                    PeriodTypeId = closedTimePeriod.PeriodTypeId,
                    IsClosed = closedTimePeriod.IsClosed,
                    FromDate = closedTimePeriod.FromDate,
                    ThruDate = closedTimePeriod.ThruDate
                };
                lastClosedDate = closedTimePeriod.ThruDate;
            }
            else
            {
                // Step 2: If no closed periods, find the earliest available period
                var timePeriodQuery = _context.CustomTimePeriods
                    .Where(tp => tp.OrganizationPartyId == organizationPartyId);

                if (!string.IsNullOrEmpty(periodTypeId))
                {
                    timePeriodQuery = timePeriodQuery.Where(tp => tp.PeriodTypeId == periodTypeId);
                }

                var earliestTimePeriod = await timePeriodQuery
                    .OrderBy(tp => tp.FromDate)
                    .FirstOrDefaultAsync();

                if (earliestTimePeriod != null)
                {
                    // Fallback to the earliest available period
                    lastClosedTimePeriod = new CustomTimePeriod
                    {
                        CustomTimePeriodId = earliestTimePeriod.CustomTimePeriodId,
                        PeriodTypeId = earliestTimePeriod.PeriodTypeId,
                        IsClosed = earliestTimePeriod.IsClosed,
                        FromDate = earliestTimePeriod.FromDate,
                        ThruDate = earliestTimePeriod.ThruDate
                    };
                    lastClosedDate = earliestTimePeriod.FromDate;
                }
            }

            // Step 3: Return results (even if no periods exist)
            return new LastClosedTimePeriodResult
            {
                LastClosedDate = lastClosedDate, // Could be null if no periods exist
                LastClosedTimePeriod = lastClosedTimePeriod // Could be null if no periods exist
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Error in FindLastClosedDate", ex);
        }
    }


    /// <summary>
    /// Gets either posted or unposted totals (mirroring OFBiz posted/unposted logic).
    /// Also fetches opening balances from last closed period and sums opening transactions 
    /// from [lastClosedDate, fromDate) excluding PERIOD_CLOSING. 
    /// </summary>
    private async Task<List<TransactionTotal>> GetTransactionTotalsByPostingStatus(
        string organizationPartyId,
        List<string> partyIds,
        DateTime fromDate,
        DateTime thruDate,
        string glFiscalTypeId,
        bool isPosted)
    {
        try
        {
            // 1) Find last closed period info to get opening balances
            var lastClosedDateResult = await FindLastClosedDate(organizationPartyId, fromDate, null);
            var lastClosedDate = lastClosedDateResult?.LastClosedDate;
            var lastClosedTimePeriod = lastClosedDateResult?.LastClosedTimePeriod;

            // 2) Query main transactions in [fromDate, thruDate], isPosted = Y/N, excluding PERIOD_CLOSING
            var isPostedFlag = isPosted ? "Y" : "N";
            var mainTransactions = await (
                from ate in _context.AcctgTransEntries
                join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                join gla in _context.GlAccounts on ate.GlAccountId equals gla.GlAccountId
                where partyIds.Contains(ate.OrganizationPartyId)
                      && act.IsPosted == isPostedFlag
                      && act.GlFiscalTypeId == glFiscalTypeId
                      && act.AcctgTransTypeId != "PERIOD_CLOSING" // exclude period-closing
                      && act.TransactionDate >= fromDate
                      && act.TransactionDate <= thruDate
                group new { ate, gla } by new
                {
                    ate.GlAccountId,
                    gla.AccountName,
                    gla.AccountCode,
                    ate.DebitCreditFlag
                }
                into g
                select new
                {
                    g.Key.GlAccountId,
                    g.Key.AccountName,
                    g.Key.AccountCode,
                    g.Key.DebitCreditFlag,
                    Amount = g.Sum(x => x.ate.Amount)
                }
            ).ToListAsync();

            // 3) Build a map from GlAccountId -> TransactionTotal
            var totalsMap = new Dictionary<string, TransactionTotal>();
            foreach (var trans in mainTransactions)
            {
                // Check if we already have an entry for this GL account
                if (!totalsMap.TryGetValue(trans.GlAccountId, out var accountTotal))
                {
                    // (A) Initialize a new TransactionTotal
                    accountTotal = new TransactionTotal
                    {
                        GlAccountId = trans.GlAccountId,
                        AccountCode = trans.AccountCode,
                        AccountName = trans.AccountName,
                        OpeningD = 0,
                        OpeningC = 0,
                        D = 0,
                        C = 0,
                        Balance = 0
                    };

                    // (B) If there's a lastClosedTimePeriod, retrieve prior ending balance & posted debits/credits
                    //     by manually joining GlAccount, GlAccountHistory, (and optionally GlAccountClass).
                    if (lastClosedTimePeriod != null)
                    {
                        var lastHistory = await (
                            from gla in _context.GlAccounts
                            join glah in _context.GlAccountHistories
                                on gla.GlAccountId equals glah.GlAccountId
                            join glac in _context.GlAccountClasses
                                on gla.GlAccountClassId equals glac.GlAccountClassId
                                into glacJoin
                            from glac in glacJoin.DefaultIfEmpty() // left-join if needed
                            where glah.OrganizationPartyId == organizationPartyId
                                  && glah.GlAccountId == trans.GlAccountId
                                  && glah.CustomTimePeriodId == lastClosedTimePeriod.CustomTimePeriodId
                            select new
                            {
                                glah.EndingBalance,
                                glah.PostedDebits,
                                glah.PostedCredits
                            }
                        ).FirstOrDefaultAsync();

                        if (lastHistory != null)
                        {
                            accountTotal.Balance = lastHistory.EndingBalance ?? 0;
                            accountTotal.OpeningD = lastHistory.PostedDebits ?? 0;
                            accountTotal.OpeningC = lastHistory.PostedCredits ?? 0;
                        }
                    }

                    // (C) Also add any transactions in [lastClosedDate, fromDate), 
                    //     isPosted = Y/N, glFiscalTypeId = same, excluding PERIOD_CLOSING
                    //     (only if lastClosedDate < fromDate)
                    if (lastClosedDate.HasValue && lastClosedDate < fromDate)
                    {
                        var openingTrans = await (
                            from ate in _context.AcctgTransEntries
                            join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                            where partyIds.Contains(ate.OrganizationPartyId)
                                  && act.IsPosted == isPostedFlag
                                  && act.GlFiscalTypeId == glFiscalTypeId
                                  && act.AcctgTransTypeId != "PERIOD_CLOSING"
                                  && act.TransactionDate >= lastClosedDate
                                  && act.TransactionDate < fromDate
                                  && ate.GlAccountId == trans.GlAccountId
                            group ate by ate.DebitCreditFlag
                            into g
                            select new
                            {
                                DebitCreditFlag = g.Key,
                                Amount = g.Sum(x => x.Amount)
                            }
                        ).ToListAsync();

                        foreach (var opTrans in openingTrans)
                        {
                            if (opTrans.DebitCreditFlag == "D")
                            {
                                accountTotal.OpeningD += opTrans.Amount;
                            }
                            else
                            {
                                accountTotal.OpeningC += opTrans.Amount;
                            }
                        }
                    }

                    // Finally store it in the dictionary
                    totalsMap[trans.GlAccountId] = accountTotal;
                }

                // (D) Add the main transaction amounts for [fromDate, thruDate]
                if (trans.DebitCreditFlag == "D")
                {
                    accountTotal.D += trans.Amount;
                }
                else
                {
                    accountTotal.C += trans.Amount;
                }
            }

            // 4) Convert dictionary to list
            var totalsList = totalsMap.Values.ToList();

            // 5) Compute grand total (debit & credit) across all mainTransactions
            var grandTotalDebit = mainTransactions
                .Where(t => t.DebitCreditFlag == "D")
                .Sum(t => t.Amount);

            var grandTotalCredit = mainTransactions
                .Where(t => t.DebitCreditFlag == "C")
                .Sum(t => t.Amount);

            // 6) Add a "summary row" for the overall totals
            totalsList.Add(new TransactionTotal
            {
                D = grandTotalDebit,
                C = grandTotalCredit
            });

            return totalsList;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error in GetTransactionTotalsByPostingStatus (isPosted={isPosted})", ex);
        }
    }


    /// <summary>
    /// Retrieves BOTH posted and unposted transactions in [fromDate, thruDate].
    /// Also merges prior closing balances, similar to the method above.
    /// </summary>
    private async Task<List<TransactionTotal>> GetAllTransactionTotals(
        string organizationPartyId,
        List<string> partyIds,
        DateTime fromDate,
        DateTime thruDate,
        string glFiscalTypeId)
    {
        try
        {
            // 1) Find last closed period info (for opening balances)
            var lastClosedDateResult = await FindLastClosedDate(organizationPartyId, fromDate, null);
            var lastClosedDate = lastClosedDateResult?.LastClosedDate;
            var lastClosedTimePeriod = lastClosedDateResult?.LastClosedTimePeriod;

            // 2) Query for all transactions (posted + unposted), excluding PERIOD_CLOSING, in [fromDate, thruDate]
            var allTransactions = await (
                from ate in _context.AcctgTransEntries
                join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                join gla in _context.GlAccounts on ate.GlAccountId equals gla.GlAccountId
                where partyIds.Contains(ate.OrganizationPartyId)
                      && act.GlFiscalTypeId == glFiscalTypeId
                      && act.AcctgTransTypeId != "PERIOD_CLOSING"
                      && act.TransactionDate >= fromDate
                      && act.TransactionDate <= thruDate
                group new { ate, gla } by new
                {
                    ate.GlAccountId,
                    gla.AccountName,
                    gla.AccountCode,
                    ate.DebitCreditFlag
                }
                into g
                select new
                {
                    g.Key.GlAccountId,
                    g.Key.AccountName,
                    g.Key.AccountCode,
                    g.Key.DebitCreditFlag,
                    Amount = g.Sum(x => x.ate.Amount)
                }
            ).ToListAsync();

            // 3) Build a map from GlAccountId -> TransactionTotal
            var totalsMap = new Dictionary<string, TransactionTotal>();

            foreach (var trans in allTransactions)
            {
                if (!totalsMap.TryGetValue(trans.GlAccountId, out var accountTotal))
                {
                    accountTotal = new TransactionTotal
                    {
                        GlAccountId = trans.GlAccountId,
                        AccountCode = trans.AccountCode,
                        AccountName = trans.AccountName,
                        OpeningD = 0,
                        OpeningC = 0,
                        D = 0,
                        C = 0,
                        Balance = 0
                    };

                    // --------------------------------------------------------
                    // (A) Retrieve prior balances from the underlying tables
                    //     that GlAccountAndHistory "view-entity" would join.
                    // --------------------------------------------------------
                    // This is the expanded join logic:
                    //   GlAccount (GLA) -> GlAccountHistory (GLAH) -> GlAccountClass (GLAC) [optional]
                    // Based on your <view-entity> definition:
                    //   <view-link entity-alias="GLA" rel-entity-alias="GLAH"> glAccountId = glAccountId </view-link>
                    //   <view-link entity-alias="GLA" rel-entity-alias="GLAC"> glAccountClassId = glAccountClassId </view-link>

                    if (lastClosedTimePeriod != null)
                    {
                        // Only attempt to retrieve history if we have a valid time period
                        var lastHistory = await (
                            from gla in _context.GlAccounts
                            join glah in _context.GlAccountHistories
                                on gla.GlAccountId equals glah.GlAccountId
                            join glac in _context.GlAccountClasses
                                on gla.GlAccountClassId equals glac.GlAccountClassId
                                into glacJoin
                            from glac in glacJoin.DefaultIfEmpty() // left-join if needed
                            where glah.OrganizationPartyId == organizationPartyId
                                  && glah.GlAccountId == trans.GlAccountId
                                  && glah.CustomTimePeriodId == lastClosedTimePeriod.CustomTimePeriodId
                            select new
                            {
                                glah.EndingBalance,
                                glah.PostedDebits,
                                glah.PostedCredits
                                // If you need anything else from GlAccountClass, pull it here
                            }
                        ).FirstOrDefaultAsync();

                        if (lastHistory != null)
                        {
                            accountTotal.Balance = lastHistory.EndingBalance ?? 0;
                            accountTotal.OpeningD = lastHistory.PostedDebits ?? 0;
                            accountTotal.OpeningC = lastHistory.PostedCredits ?? 0;
                        }
                    }

                    // --------------------------------------------------------
                    // (B) Add opening transactions from [lastClosedDate, fromDate),
                    //     excluding PERIOD_CLOSING. That replicates the script's logic
                    //     for partial “opening” activity after the period closed.
                    // --------------------------------------------------------
                    if (lastClosedDate.HasValue && lastClosedDate < fromDate)
                    {
                        var openingTrans = await (
                            from ate in _context.AcctgTransEntries
                            join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                            where partyIds.Contains(ate.OrganizationPartyId)
                                  && act.GlFiscalTypeId == glFiscalTypeId
                                  && act.AcctgTransTypeId != "PERIOD_CLOSING"
                                  && act.TransactionDate >= lastClosedDate
                                  && act.TransactionDate < fromDate
                                  && ate.GlAccountId == trans.GlAccountId
                            group ate by ate.DebitCreditFlag
                            into g
                            select new
                            {
                                DebitCreditFlag = g.Key,
                                Amount = g.Sum(x => x.Amount)
                            }
                        ).ToListAsync();

                        foreach (var opTrans in openingTrans)
                        {
                            if (opTrans.DebitCreditFlag == "D")
                            {
                                accountTotal.OpeningD += opTrans.Amount;
                            }
                            else
                            {
                                accountTotal.OpeningC += opTrans.Amount;
                            }
                        }
                    }

                    totalsMap[trans.GlAccountId] = accountTotal;
                }

                // (C) Add the main transaction amounts (the in-range [fromDate, thruDate] portion)
                if (trans.DebitCreditFlag == "D")
                {
                    accountTotal.D += trans.Amount;
                }
                else
                {
                    accountTotal.C += trans.Amount;
                }
            }

            // 4) Convert dictionary to list
            var totalsList = totalsMap.Values.ToList();

            // 5) Compute grand totals for all D/C in the date window
            var grandTotalDebit = allTransactions
                .Where(t => t.DebitCreditFlag == "D")
                .Sum(t => t.Amount);

            var grandTotalCredit = allTransactions
                .Where(t => t.DebitCreditFlag == "C")
                .Sum(t => t.Amount);

            // 6) Append a final "summary row" for overall D/C totals
            totalsList.Add(new TransactionTotal
            {
                D = grandTotalDebit,
                C = grandTotalCredit
            });

            return totalsList;
        }
        catch (Exception ex)
        {
            throw new Exception("Error in GetAllTransactionTotals", ex);
        }
    }

    /// <summary>
    /// Generates an Income Statement (similar to the OFBiz script).
    /// Includes the basic lines: net sales, gross margin, etc.
    /// 
    /// Business Logic (Accounting Explanation):
    /// 1) Revenue & Contra Revenue produce Net Sales.
    /// 2) Subtract COGS from Net Sales to get Gross Margin.
    /// 3) Subtract SGA Expenses from Gross Margin to get Income from Operations.
    /// 4) Add other income and subtract total expenses to get Net Income.
    /// </summary>
    public async Task<IncomeStatementViewModel> GenerateIncomeStatement(string organizationPartyId, DateTime? fromDate,
        DateTime? thruDate, string glFiscalTypeId, int? selectedMonth = null)
    {
        var now = DateTime.Now;

        try
        {
            if (!fromDate.HasValue || !thruDate.HasValue || string.IsNullOrEmpty(glFiscalTypeId))
            {
                throw new ArgumentException("Invalid parameters");
            }

            // If user selected a month, override fromDate & thruDate
            if (selectedMonth.HasValue)
            {
                var selectedMonthDate = new DateTime(now.Year, selectedMonth.Value + 1, 1);
                fromDate = selectedMonthDate;
                thruDate = selectedMonthDate.AddMonths(1).AddDays(-1);
            }

            if (!thruDate.HasValue)
            {
                thruDate = DateTime.Now;
            }

            organizationPartyId = organizationPartyId ?? throw new ArgumentNullException(nameof(organizationPartyId));

            // Setup the divisions for which the report is executed
            var partyIds = await GetAssociatedPartyIdsByRelationshipType(organizationPartyId, "GROUP_ROLLUP");
            partyIds.Add(organizationPartyId);

            // Step C: Retrieve descendant GL account classes for each root class:
            // e.g. REVENUE includes all sub-classes, etc.
            var revenueAccountClassIds = await GetDescendantGlAccountClassIds("REVENUE");
            var contraRevenueAccountClassIds = await GetDescendantGlAccountClassIds("CONTRA_REVENUE");
            var incomeAccountClassIds = await GetDescendantGlAccountClassIds("INCOME");
            var expenseAccountClassIds = await GetDescendantGlAccountClassIds("EXPENSE");
            var cogsExpenseAccountClassIds = await GetDescendantGlAccountClassIds("COGS_EXPENSE");
            var sgaExpenseAccountClassIds = await GetDescendantGlAccountClassIds("SGA_EXPENSE");
            var depreciationAccountClassIds = await GetDescendantGlAccountClassIds("DEPRECIATION");

            // Step D: For each major account class, calculate posted transactions and final balance
            // Explanation: We only want posted transactions (IsPosted = "Y"),
            // excluding PERIOD_CLOSING, between [fromDate, thruDate).

            // 1) Summation of all REVENUE accounts
            var revenueTotals = await CalculateAccountBalances(
                revenueAccountClassIds, partyIds, fromDate.Value, thruDate.Value, glFiscalTypeId,
                isRevenueOrIncome: true);

            // 2) Summation of all CONTRA REVENUE accounts
            var contraRevenueTotals = await CalculateAccountBalances(
                contraRevenueAccountClassIds, partyIds, fromDate.Value, thruDate.Value, glFiscalTypeId,
                isRevenueOrIncome: false);

            // 3) Summation of all EXPENSE accounts
            var expenseTotals = await CalculateAccountBalances(
                expenseAccountClassIds, partyIds, fromDate.Value, thruDate.Value, glFiscalTypeId,
                isRevenueOrIncome: false);

            // 4) Summation of all COGS (Cost of Goods Sold) EXPENSE accounts
            var cogsExpenseTotals = await CalculateAccountBalances(
                cogsExpenseAccountClassIds, partyIds, fromDate.Value, thruDate.Value, glFiscalTypeId,
                isRevenueOrIncome: false);

            // 5) Summation of all SGA (Selling, General & Administrative) EXPENSE accounts
            var sgaExpenseTotals = await CalculateAccountBalances(
                sgaExpenseAccountClassIds, partyIds, fromDate.Value, thruDate.Value, glFiscalTypeId,
                isRevenueOrIncome: false);

            // 6) Summation of all DEPRECIATION accounts (also typically expenses)
            var depreciationTotals = await CalculateAccountBalances(
                depreciationAccountClassIds, partyIds, fromDate.Value, thruDate.Value, glFiscalTypeId,
                isRevenueOrIncome: false);

            // 7) Summation of all INCOME accounts
            var incomeTotals = await CalculateAccountBalances(
                incomeAccountClassIds, partyIds, fromDate.Value, thruDate.Value, glFiscalTypeId,
                isRevenueOrIncome: true);


            // Step E: Compute final lines of the Income Statement

            // Net Sales = Revenue - Contra Revenue
            var netSales = revenueTotals.BalanceTotal - contraRevenueTotals.BalanceTotal;

            // Gross Margin = Net Sales - COGS
            var grossMargin = netSales - cogsExpenseTotals.BalanceTotal;

            // Income From Operations = Gross Margin - SGA Expenses
            var incomeFromOperations = grossMargin - sgaExpenseTotals.BalanceTotal;

            // Net Income = Net Sales + Income Accounts - Expense Accounts
            // Explanation: Another typical formula is (Revenue - COGS - Expense + Other Income).
            var netIncome = netSales + incomeTotals.BalanceTotal - expenseTotals.BalanceTotal;

            // Step F: Return a structured view model with all details
            return new IncomeStatementViewModel
            {
                // Detailed lists of each account's balance
                RevenueAccountBalances = revenueTotals.AccountBalanceList,
                ContraRevenueAccountBalances = contraRevenueTotals.AccountBalanceList,
                ExpenseAccountBalances = expenseTotals.AccountBalanceList,
                CogsExpenseAccountBalances = cogsExpenseTotals.AccountBalanceList,
                SgaExpenseAccountBalances = sgaExpenseTotals.AccountBalanceList,
                DepreciationAccountBalances = depreciationTotals.AccountBalanceList,
                IncomeAccountBalances = incomeTotals.AccountBalanceList,

                // Key line items in an income statement
                NetSales = netSales,
                GrossMargin = grossMargin,
                IncomeFromOperations = incomeFromOperations,
                NetIncome = netIncome,

                // Final numeric totals for each category
                RevenueBalanceTotal = revenueTotals.BalanceTotal,
                ContraRevenueBalanceTotal = contraRevenueTotals.BalanceTotal,
                ExpenseBalanceTotal = expenseTotals.BalanceTotal,
                CogsExpenseBalanceTotal = cogsExpenseTotals.BalanceTotal,
                SgaExpenseBalanceTotal = sgaExpenseTotals.BalanceTotal,
                DepreciationBalanceTotal = depreciationTotals.BalanceTotal,
                IncomeBalanceTotal = incomeTotals.BalanceTotal
            };
        }
        catch (Exception ex)
        {
            // If anything goes wrong (DB errors, invalid data, etc.), we catch here
            // You can log, rethrow, or handle as needed
            throw new Exception("Error generating Income Statement", ex);
        }
    }

    /// <summary>
    /// Recursive method to gather all descendant GlAccountClassIds
    /// starting from a given 'root' (e.g. "REVENUE").
    ///
    /// Business Explanation: 
    /// Certain GL account classes may have child classes.
    /// We want the full hierarchy so we can sum transactions for the entire group.
    /// </summary>
    public async Task<List<string>> GetDescendantGlAccountClassIds(string glAccountClassId)
    {
        var results = new List<string>();
        await CollectAccountClassChildren(glAccountClassId, results);
        return results;
    }

    private async Task CollectAccountClassChildren(string glAccountClassId, List<string> collected)
    {
        // Add the current class
        collected.Add(glAccountClassId);

        // Find any children whose parent is this class
        var children = await _context.GlAccountClasses
            .Where(gac => gac.ParentClassId == glAccountClassId)
            .ToListAsync();

        // For each child, recursively collect more
        foreach (var child in children)
        {
            await CollectAccountClassChildren(child.GlAccountClassId, collected);
        }
    }

    /// <summary>
    /// Generates a Cash Flow Statement focusing on cash-equivalent accounts:
    /// 1) Validates input parameters (dates, fiscal type).
    /// 2) Fetches party IDs (organization + subdivisions).
    /// 3) Finds last closed date to gather opening balances from that historical period.
    /// 4) Merges opening transactions in [lastClosedDate, fromDate) with those balances.
    /// 5) Fetches period transactions in [fromDate, thruDate).
    /// 6) Combines them to produce a final "closing" snapshot, 
    ///    and calculates "EndingCashBalance" = opening + period totals.
    /// </summary>
    public async Task<CashFlowStatementViewModel> GenerateCashFlowStatement(
        string organizationPartyId,
        DateTime? fromDate,
        DateTime? thruDate,
        string glFiscalTypeId, int? selectedMonth = null)
    {
        var now = DateTime.Now;

        try
        {
            // A) Parameter checks
            if (!fromDate.HasValue || !thruDate.HasValue)
            {
                throw new ArgumentException("Both fromDate and thruDate are required.");
            }

            // If user selected a month, override fromDate & thruDate
            if (selectedMonth.HasValue)
            {
                var selectedMonthDate = new DateTime(now.Year, selectedMonth.Value + 1, 1);
                fromDate = selectedMonthDate;
                thruDate = selectedMonthDate.AddMonths(1).AddDays(-1);
            }

            if (string.IsNullOrEmpty(glFiscalTypeId))
            {
                glFiscalTypeId = "ACTUAL";
            }

            // B) Gather all relevant party IDs (org + children)
            var partyIds = await GetAssociatedPartyIdsByRelationshipType(organizationPartyId, "GROUP_ROLLUP");
            if (!partyIds.Contains(organizationPartyId))
            {
                partyIds.Add(organizationPartyId);
            }

            // C) "CASH_EQUIVALENT" account classes
            var glAccountClassIds = await GetDescendantGlAccountClassIds("CASH_EQUIVALENT");

            // D) Find last closed date/period prior to 'fromDate'
            var lastClosedTimePeriodResult = await FindLastClosedDate(organizationPartyId, fromDate, null);
            if (lastClosedTimePeriodResult?.LastClosedDate == null)
            {
                // If no closed period found, we could return null or handle differently
                return null;
            }

            var periodClosingFromDate = lastClosedTimePeriodResult.LastClosedDate.Value;
            var lastClosedTimePeriod = lastClosedTimePeriodResult.LastClosedTimePeriod;

            // E) Retrieve the "opening" cash balances from that closed period
            //    This is the baseline before new transactions after lastClosedDate.
            var openingCashBalances = await GetOpeningCashBalances(
                lastClosedTimePeriod, // includes customTimePeriodId
                partyIds,
                glAccountClassIds
            );

            // F) Merge new transactions in [lastClosedDate, fromDate)
            //    This method returns a list of "TransactionTotal" objects
            //    with a normal "cash" logic (D => add, C => subtract).
            var openingBalanceResult = await GetCashTransactionsWithMerge(
                partyIds,
                glAccountClassIds,
                periodClosingFromDate,
                fromDate.Value,
                glFiscalTypeId,
                openingCashBalances);

            // G) Collect transactions for the statement period [fromDate, thruDate)
            var periodBalanceResult = await GetCashTransactionsWithMerge(
                partyIds,
                glAccountClassIds,
                fromDate.Value,
                thruDate.Value,
                glFiscalTypeId,
                null); // no initial balances here, just fresh transactions

            // H) Now combine opening + period to get a "closing" snapshot
            // Explanation: If an account appears in both sets, sum up D, C, then do (C - D).
            var combinedTransactions = openingBalanceResult
                .Concat(periodBalanceResult)
                .ToList();

            // We rely on a group-based aggregator that sums up D & C for each account
            var closingBalanceResult = CalculateClosingBalances(combinedTransactions);

            // I) The final "Ending Cash Balance" is simply the sum of 
            //    the total opening balance and the total period balance
            //    (assuming typical "debit" logic for cash).
            var endingCashBalanceTotal =
                SumBalance(openingBalanceResult) + SumBalance(periodBalanceResult);

            // J) Build a final view model (mirroring the OFBiz script)
            return new CashFlowStatementViewModel
            {
                // Opening portion
                OpeningCashBalanceList = openingBalanceResult,
                OpeningCashBalanceTotal = SumBalance(openingBalanceResult),

                // Period portion
                PeriodCashBalanceList = periodBalanceResult,
                PeriodCashBalanceTotal = SumBalance(periodBalanceResult),

                // Closing snapshot (all accounts combined)
                ClosingCashBalanceList = closingBalanceResult.AccountBalanceList,
                ClosingCashBalanceTotal = closingBalanceResult.BalanceTotal,

                // Final ending total = opening + period
                EndingCashBalanceTotal = endingCashBalanceTotal
            };
        }
        catch (Exception ex)
        {
            // If a DB or logic error occurs, we capture it here
            throw new Exception("Error in GenerateCashFlowStatement", ex);
        }
    }

    /// <summary>
    /// Summation of the "Balance" field across a list of TransactionTotal.
    /// This matches how your script tracks net effect for "cash" accounts.
    /// </summary>
    private decimal SumBalance(List<TransactionTotal> transactions)
    {
        return (decimal)transactions.Sum(t => t.Balance);
    }

    // <summary>
    /// Gathers posted transactions in [startDate, endDate),
    /// merges them with existing 'initialBalances' if provided,
    /// and returns a list of TransactionTotal with final Debit, Credit, and Balance.
    ///
    /// Business Explanation:
    /// - For "cash" accounts, a Debit typically increases the balance, 
    ///   while a Credit decreases it.
    /// - This is similar to the script logic where "C => subtract" and "D => add".
    /// </summary>
    private async Task<List<TransactionTotal>> GetCashTransactionsWithMerge(
        List<string> partyIds,
        List<string> glAccountClassIds,
        DateTime startDate,
        DateTime endDate,
        string glFiscalTypeId,
        List<TransactionTotal> initialBalances)
    {
        // 1) Convert initial list into a quick map, if needed
        var resultMap = new Dictionary<string, TransactionTotal>();
        if (initialBalances != null)
        {
            foreach (var acc in initialBalances)
            {
                resultMap[acc.GlAccountId] = new TransactionTotal
                {
                    GlAccountId = acc.GlAccountId,
                    AccountCode = acc.AccountCode,
                    AccountName = acc.AccountName,
                    D = acc.D,
                    C = acc.C,
                    Balance = acc.Balance
                };
            }
        }

        // 2) Fetch posted non-period-closing transactions within [startDate, endDate)
        var transactions = await (
            from ate in _context.AcctgTransEntries
            join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
            join gla in _context.GlAccounts on ate.GlAccountId equals gla.GlAccountId
            where partyIds.Contains(ate.OrganizationPartyId)
                  && glAccountClassIds.Contains(gla.GlAccountClassId)
                  && act.IsPosted == "Y"
                  && act.GlFiscalTypeId == glFiscalTypeId
                  && act.AcctgTransTypeId != "PERIOD_CLOSING"
                  && act.TransactionDate >= startDate
                  && act.TransactionDate < endDate
            select new
            {
                ate.GlAccountId,
                gla.AccountCode,
                gla.AccountName,
                ate.DebitCreditFlag,
                ate.Amount
            }
        ).ToListAsync();

        // 3) Merge these transactions into our map
        //    For a "cash" account, if Debit => +Balance, if Credit => -Balance
        foreach (var row in transactions)
        {
            if (!resultMap.TryGetValue(row.GlAccountId, out var accTotal))
            {
                accTotal = new TransactionTotal
                {
                    GlAccountId = row.GlAccountId,
                    AccountCode = row.AccountCode,
                    AccountName = row.AccountName,
                    D = 0,
                    C = 0,
                    Balance = 0
                };
                resultMap[row.GlAccountId] = accTotal;
            }

            if (row.DebitCreditFlag == "D")
            {
                accTotal.D += row.Amount;
                accTotal.Balance += row.Amount; // typical "cash" logic
            }
            else
            {
                accTotal.C += row.Amount;
                accTotal.Balance -= row.Amount;
            }
        }

        // 4) Return the final list, sorted by account code
        return resultMap.Values
            .OrderBy(x => x.AccountCode)
            .ToList();
    }

    /// <summary>
    /// Calculates account balances for a given list of GL account class IDs, 
    /// by summing all posted transactions in [fromDate, thruDate).
    /// 
    /// Business Explanation:
    /// For "credit" class accounts (Revenue/Income), we typically do (Credit - Debit) to get a positive balance.
    /// For "debit" class accounts (Expenses), we do (Debit - Credit).
    /// We store them all in a single structure, but we pass a boolean "isRevenueOrIncome" 
    /// so we know how to interpret the final sign of the balance.
    /// </summary>
    private async Task<AccountBalanceResult> CalculateAccountBalances(
        List<string> accountClassIds,
        List<string> partyIds,
        DateTime fromDate,
        DateTime thruDate,
        string glFiscalTypeId,
        bool isRevenueOrIncome)
    {
        try
        {
            // Query all posted, non-period-closing transactions for the given accounts and date range
            var transactions = await (
                from ate in _context.AcctgTransEntries
                join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                join gla in _context.GlAccounts on ate.GlAccountId equals gla.GlAccountId
                where accountClassIds.Contains(gla.GlAccountClassId)
                      && partyIds.Contains(ate.OrganizationPartyId)
                      && act.IsPosted == "Y"
                      && act.GlFiscalTypeId == glFiscalTypeId
                      && act.AcctgTransTypeId != "PERIOD_CLOSING"
                      && act.TransactionDate >= fromDate
                      && act.TransactionDate < thruDate
                group ate by new
                {
                    ate.GlAccountId,
                    gla.AccountName,
                    gla.AccountCode,
                    ate.DebitCreditFlag
                }
                into g
                select new
                {
                    g.Key.GlAccountId,
                    g.Key.AccountName,
                    g.Key.AccountCode,
                    g.Key.DebitCreditFlag,
                    Amount = g.Sum(x => x.Amount)
                }
            ).ToListAsync();

            // We'll build a map of GlAccountId -> TransactionTotal
            var transactionTotalsMap = new Dictionary<string, TransactionTotal>();

            foreach (var row in transactions)
            {
                // If we haven't stored this GlAccountId yet, create a new entry
                if (!transactionTotalsMap.TryGetValue(row.GlAccountId, out var accTotal))
                {
                    accTotal = new TransactionTotal
                    {
                        GlAccountId = row.GlAccountId,
                        AccountCode = row.AccountCode,
                        AccountName = row.AccountName,
                        D = 0,
                        C = 0,
                        Balance = 0
                    };
                    transactionTotalsMap[row.GlAccountId] = accTotal;
                }

                // Sum debits vs. credits
                if (row.DebitCreditFlag == "D")
                {
                    accTotal.D += row.Amount;
                }
                else
                {
                    accTotal.C += row.Amount;
                }
            }

            // Now finalize the balance for each account
            // For revenue/income (credit-based) accounts: Balance = C - D
            // For expense (debit-based) accounts: Balance = D - C
            foreach (var entry in transactionTotalsMap.Values)
            {
                if (isRevenueOrIncome)
                {
                    // Typically credit-based
                    entry.Balance = entry.C - entry.D;
                }
                else
                {
                    // Typically debit-based
                    entry.Balance = entry.D - entry.C;
                }
            }

            // We'll also compute an overall total for the entire category
            decimal totalDebits = (decimal)transactionTotalsMap.Values.Sum(x => x.D);
            decimal totalCredits = (decimal)transactionTotalsMap.Values.Sum(x => x.C);
            decimal balanceTotal;
            if (isRevenueOrIncome)
            {
                // For credit-based categories, net is totalCredits - totalDebits
                balanceTotal = totalCredits - totalDebits;
            }
            else
            {
                // For debit-based categories, net is totalDebits - totalCredits
                balanceTotal = totalDebits - totalCredits;
            }

            // Sort by account code for display
            var accountBalanceList = transactionTotalsMap.Values
                .OrderBy(x => x.AccountCode)
                .ToList();

            // Return the structured result
            return new AccountBalanceResult
            {
                AccountBalanceList = accountBalanceList,
                BalanceTotal = balanceTotal
            };
        }
        catch (Exception ex)
        {
            // If any exception occurs while calculating, handle/log as needed
            throw new Exception("Error in CalculateAccountBalances", ex);
        }
    }


    /// <summary>
    /// You also have this version that returns a List, instead of dictionary:
    /// Fetches GlAccountHistory for the lastClosedTimePeriod 
    /// and returns each account's postedDebits, postedCredits, endingBalance in a list.
    /// </summary>
    private async Task<List<TransactionTotal>> GetOpeningCashBalances(CustomTimePeriod lastClosedTimePeriod,
        List<string> partyIds, List<string> glAccountClassIds)
    {
        var openingCashBalances = new List<TransactionTotal>();

        if (lastClosedTimePeriod != null)
        {
            var customTimePeriodId = lastClosedTimePeriod.CustomTimePeriodId;

            var lastTimePeriodHistories = await (from gla in _context.GlAccounts
                join glah in _context.GlAccountHistories on gla.GlAccountId equals glah.GlAccountId
                join glac in _context.GlAccountClasses on gla.GlAccountClassId equals glac.GlAccountClassId
                where partyIds.Contains(glah.OrganizationPartyId)
                      && glAccountClassIds.Contains(gla.GlAccountClassId)
                      && glah.EndingBalance != 0
                      && glah.CustomTimePeriodId == customTimePeriodId
                select new
                {
                    gla.GlAccountId,
                    gla.AccountCode,
                    gla.AccountName,
                    glah.PostedDebits,
                    glah.PostedCredits,
                    glah.EndingBalance
                }).ToListAsync();

            foreach (var lastTimePeriodHistory in lastTimePeriodHistories)
            {
                var accountMap = new TransactionTotal
                {
                    GlAccountId = lastTimePeriodHistory.GlAccountId,
                    AccountCode = lastTimePeriodHistory.AccountCode,
                    AccountName = lastTimePeriodHistory.AccountName,
                    D = lastTimePeriodHistory.PostedDebits,
                    C = lastTimePeriodHistory.PostedCredits,
                    Balance = lastTimePeriodHistory.EndingBalance
                };
                openingCashBalances.Add(accountMap);
            }
        }

        return openingCashBalances;
    }

    /// Groups a list of TransactionTotals by (GlAccountId),
    /// sums up D, C, and does Balance = (C - D) for each group (typical "cash" is a debit account).
    /// If you want "Balance = D - C" instead, adjust accordingly.
    /// </summary>
    private AccountBalanceResult CalculateClosingBalances(List<TransactionTotal> combinedTransactions)
    {
        var accountBalanceList = combinedTransactions
            .GroupBy(t => new { t.GlAccountId, t.AccountCode, t.AccountName })
            .Select(g => new TransactionTotal
            {
                GlAccountId = g.Key.GlAccountId,
                AccountCode = g.Key.AccountCode,
                AccountName = g.Key.AccountName,
                D = g.Sum(t => t.D),
                C = g.Sum(t => t.C),
                // This variant does (C - D), consistent with "credit-based" approach
                Balance = g.Sum(t => t.C) - g.Sum(t => t.D)
            })
            .OrderBy(t => t.AccountCode)
            .ToList();

        var balanceTotal = accountBalanceList.Sum(t => t.Balance);

        return new AccountBalanceResult
        {
            AccountBalanceList = accountBalanceList,
            BalanceTotal = (decimal)balanceTotal
        };
    }


    // <summary>
    /// Computes the Inventory Valuation based on the InventoryItemDetailForSum view.
    /// </summary>
    /// <param name="organizationPartyId">The owning organization party ID.</param>
    /// <param name="facilityId">Optional facility ID filter.</param>
    /// <param name="productId">Optional product ID filter.</param>
    /// <param name="thruDate">The effective date by which inventory is valued (if applicable).</param>
    /// <returns>An InventoryValuationContext with aggregated inventory valuation data.</returns>
    public async Task<InventoryValuationContext> ComputeInventoryValuation(
        string? organizationPartyId,
        string? facilityId,
        string? productId,
        DateTime? thruDate)
    {
        try
        {
            var effectiveDate = thruDate ?? DateTime.UtcNow; // default effective date if not provided

            var query = _context.InventoryItemDetailForSumView.AsQueryable();

            // Apply time-based filter using the chosen effective date
            query = query.Where(x => x.EffectiveDate <= effectiveDate);

            // Filter out items with zero accounting quantity sum if that aligns with the original logic
            query = query.Where(x => x.AccountingQuantitySum != 0);

            // Filter by facility only if facilityId is provided
            if (!string.IsNullOrEmpty(facilityId))
            {
                query = query.Where(x => x.FacilityId == facilityId);
            }

            // Filter by product only if productId is provided
            if (!string.IsNullOrEmpty(productId))
            {
                query = query.Where(x => x.ProductId == productId);
            }
            
            var sql = query.ToQueryString();
            Console.WriteLine(sql);

            // Execute the query and fetch the results
            var results = await query.ToListAsync();

            // Map results into the InventoryValuationContext
            var context = new InventoryValuationContext();

            foreach (var item in results)
            {
                var value = item.AccountingQuantitySum * item.UnitCost;

                context.Items.Add(new InventoryValuationItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    QuantityUomDescription = item.QuantityUomDescription,
                    UnitCost = item.UnitCost,
                    CurrencyUomId = item.CurrencyUomId,
                    AccountingQuantitySum = item.AccountingQuantitySum,
                    QuantityOnHandSum = item.QuantityOnHandSum,
                    Value = value
                });
            }

            // Compute the total value of all items
            context.TotalValue = (decimal)context.Items.Sum(i => i.Value);

            return context;
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while computing inventory valuation: {ex.Message}", ex);
        }
    }

    // <summary>
    /// Translates the OFBiz "prepareIncomeStatement" logic into C# using 
    /// the underlying DB tables (GlAccountOrganization, AcctgTransEntry, AcctgTrans) 
    /// instead of the single view "GlAccOrgAndAcctgTransAndEntry."
    /// 
    /// 1) Identify expense/revenue/income classes
    /// 2) Gather posted transactions in [fromDate, thruDate)
    /// 3) Flip amounts for debit/credit mismatch + expense
    /// 4) Accumulate net income & separate expense vs. profit
    /// 5) For each account, compute "fiscal period" totals
    /// 6) Return the final result with "TotalNetIncome" and a "GlAccountTotalsMap"
    /// </summary>
    public async Task<IncomeStatementResult> PrepareIncomeStatement(
        string organizationPartyId,
        DateTime fromDate,
        DateTime thruDate,
        string glFiscalTypeId)
    {
        try
        {
            // ----------------------------------------------------
            // (A) Validation checks
            // ----------------------------------------------------
            if (string.IsNullOrEmpty(organizationPartyId))
            {
                throw new ArgumentException("organizationPartyId cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(glFiscalTypeId))
            {
                throw new ArgumentException("glFiscalTypeId is required (e.g. 'ACTUAL').");
            }

            // Prepare the result container
            var result = new IncomeStatementResult
            {
                TotalNetIncome = 0m,
                GlAccountTotalsMap = new GlAccountTotalsMap
                {
                    Income = new List<GlAccountTotal>(),
                    Expenses = new List<GlAccountTotal>()
                }
            };

            // (B) Fetch the descendant GL account classes for EXPENSE, REVENUE, INCOME
            var expenseClasses = await GetDescendantGlAccountClassIds("EXPENSE");
            var revenueClasses = await GetDescendantGlAccountClassIds("REVENUE");
            var incomeClasses = await GetDescendantGlAccountClassIds("INCOME");

            // (C) Setup the party IDs (org + children)
            var partyIds = await GetAssociatedPartyIdsByRelationshipType(organizationPartyId, "GROUP_ROLLUP");
            if (!partyIds.Contains(organizationPartyId))
            {
                partyIds.Add(organizationPartyId);
            }

            // (D) Build the query that replicates GlAccOrgAndAcctgTransAndEntry:
            //     Join GlAccountOrganization (GAO), AcctgTransEntry (ATE), AcctgTrans (ATR).
            //     Group by GAO.glAccountId, ATE.debitCreditFlag, ATR.isPosted, ATR.transactionDate, etc.
            //     We'll do a simpler approach: we won't group in EF first, 
            //     but we WILL filter exactly how the script does 
            //     and sum the amounts or transform them after we gather the rows.
            //
            // Conditions from minilang:
            //    - organizationPartyId in (partyIds)
            //    - isPosted == Y
            //    - glFiscalTypeId == input
            //    - transactionDate >= fromDate, < thruDate
            //    - acctgTransTypeId != PERIOD_CLOSING
            //    - glAccountClassId in (expense, revenue, income)
            // We can get glAccountClassId from "GlAccount" table, 
            // so let's just join that as well.

            // Build a single EF query with multiple joins:

            var validClasses = expenseClasses.Concat(revenueClasses).Concat(incomeClasses).Distinct().ToList();

            var query = from gao in _context.GlAccountOrganizations
                join ate in _context.AcctgTransEntries
                    on new { gao.GlAccountId, gao.OrganizationPartyId }
                    equals new { ate.GlAccountId, ate.OrganizationPartyId }
                join atr in _context.AcctgTrans
                    on ate.AcctgTransId equals atr.AcctgTransId
                join gla in _context.GlAccounts
                    on ate.GlAccountId equals gla.GlAccountId
                where partyIds.Contains(ate.OrganizationPartyId)
                      && atr.IsPosted == "Y" // because "Y" => true
                      && atr.GlFiscalTypeId == glFiscalTypeId
                      && atr.AcctgTransTypeId != "PERIOD_CLOSING"
                      && atr.TransactionDate >= fromDate
                      && atr.TransactionDate < thruDate
                      && validClasses.Contains(gla.GlAccountClassId)
                // We'll order by AcctgTransId, AcctgTransEntrySeqId if you want
                // .OrderBy(...)
                select new
                {
                    ate.GlAccountId,
                    ate.DebitCreditFlag,
                    Amount = ate.Amount,
                    // We'll need to figure out if the account is "credit" or "debit" 
                    // or "expense" based on its class. 
                    // We'll do that after we have the 'gla.GlAccountClassId' 
                    GlAccountClassId = gla.GlAccountClassId,
                    gla.AccountCode,
                    gla.AccountName,
                    atr.TransactionDate
                };

            // Execute the query
            var rawEntries = await query.ToListAsync();

            // (E) We'll interpret each row, flipping amounts if needed:
            //  - If "debitCreditFlag == D" but account is "credit-based" => flip sign
            //  - If "debitCreditFlag == C" but account is "debit-based" => flip sign
            //  - If it's an "expense" account => flip sign again
            // Then we'll sum up net income and separate them into "expense" vs. "profit" sets.

            decimal totalNetIncome = 0m;
            var expenseTotals = new List<GlAccountTotal>();
            var profitTotals = new List<GlAccountTotal>();

            foreach (var row in rawEntries)
            {
                // 1) Determine if the account is credit, debit, or expense
                bool isExpense = expenseClasses.Contains(row.GlAccountClassId);
                bool isCredit = (revenueClasses.Contains(row.GlAccountClassId) ||
                                 incomeClasses.Contains(row.GlAccountClassId));
                bool isDebit = !isCredit && !isExpense;
                // (Note: if it's not revenue/income or expense, we treat it as debit-based by default,
                //  but your classification might differ.)

                // 2) Flip sign if (D + credit) or (C + debit)
                decimal adjustedAmount = (decimal)row.Amount;
                if ((row.DebitCreditFlag == "D" && isCredit)
                    || (row.DebitCreditFlag == "C" && isDebit))
                {
                    adjustedAmount = -adjustedAmount;
                }

                // 3) If expense, flip sign again
                if (isExpense)
                {
                    adjustedAmount = -adjustedAmount;
                }

                // 4) Add to net income
                totalNetIncome += adjustedAmount;

                // 5) Put in expense or profit list
                var targetList = isExpense ? expenseTotals : profitTotals;

                // See if we already have an entry for this glAccountId
                var existing = targetList.FirstOrDefault(e => e.GlAccountId == row.GlAccountId);
                if (existing == null)
                {
                    existing = new GlAccountTotal
                    {
                        GlAccountId = row.GlAccountId,
                        TotalAmount = 0m,
                        TotalOfCurrentFiscalPeriod = 0m,
                        AccountCode = row.AccountCode,
                        AccountName = row.AccountName
                    };
                    targetList.Add(existing);
                }

                existing.TotalAmount += adjustedAmount;
            }

            // (F) For each GL account in expenseTotals and profitTotals,
            //     call "getAcctgTransEntriesAndTransTotal" to get the 
            //     current fiscal period (debitTotal - creditTotal).
            // The script sets customTimePeriodStartDate = customTimePeriod.fromDate, 
            // but let's assume it's 'fromDate' or something similar 
            // (depending on your actual "findCustomTimePeriods" logic).
            var customTimePeriodStartDate = fromDate; // or a real time period start date
            var customTimePeriodEndDate = thruDate;

            // Example usage inside your "PrepareIncomeStatement" logic (or similar):
            foreach (var acct in expenseTotals)
            {
                // Pass isPosted = "Y" if we only want posted, 
                // or isPosted = null if you want to ignore that filter.
                var (debitTotal, creditTotal) = await GetAcctgTransEntriesAndTransTotal(
                    acct.GlAccountId,
                    organizationPartyId,
                    "Y",
                    customTimePeriodStartDate,
                    customTimePeriodEndDate
                );
                acct.TotalOfCurrentFiscalPeriod = debitTotal - creditTotal;
            }

            foreach (var acct in profitTotals)
            {
                var (debitTotal, creditTotal) = await GetAcctgTransEntriesAndTransTotal(
                    acct.GlAccountId,
                    organizationPartyId,
                    /* isPosted */ "Y", // for example
                    customTimePeriodStartDate,
                    customTimePeriodEndDate
                );
                acct.TotalOfCurrentFiscalPeriod = debitTotal - creditTotal;
            }

            // (G) Construct final result
            result.TotalNetIncome = totalNetIncome;
            result.GlAccountTotalsMap.Income = profitTotals;
            result.GlAccountTotalsMap.Expenses = expenseTotals;

            return result;
        }
        catch (Exception ex)
        {
            // If anything fails (DB access, logic error, etc.), we handle or rethrow
            throw new Exception("Error in PrepareIncomeStatement (underlying tables approach)", ex);
        }
    }

    /// <summary>
    /// Translates the OFBiz "getAcctgTransEntriesAndTransTotal" minilang service:
    /// 1) Identify the org + child parties (GROUP_ROLLUP).
    /// 2) Filter "AcctgTransAndEntries" for those parties, a specific GL account, 
    ///    optional posted status, and [customTimePeriodStartDate, customTimePeriodEndDate].
    /// 3) Sum debits vs. credits. 
    /// 4) Return the final totals + difference + the list of transactions.
    /// 
    /// Business Explanation:
    /// Often used to see how much debit and credit activity 
    /// took place in a given date range for a specific account,
    /// supporting further ledger or statement calculations.
    /// </summary>
    public async Task<AcctgTransEntriesAndTransTotalResult> GetAcctgTransEntriesAndTransTotal(
        string organizationPartyId,
        string glAccountId,
        string isPosted, // e.g. "Y" or "N" or null/empty
        DateTime customTimePeriodStartDate,
        DateTime customTimePeriodEndDate)
    {
        // Prepare the result object
        var result = new AcctgTransEntriesAndTransTotalResult
        {
            AcctgTransAndEntries = new List<AcctgTransAndEntryDto>(),
            DebitTotal = 0m,
            CreditTotal = 0m,
            DebitCreditDifference = 0m
        };

        try
        {
            // ---------------------------------------------------------
            // (A) Gather the party IDs for GROUP_ROLLUP
            // ---------------------------------------------------------
            var partyIds = await GetAssociatedPartyIdsByRelationshipType(organizationPartyId, "GROUP_ROLLUP");
            // Add the main org if not already present
            if (!partyIds.Contains(organizationPartyId))
            {
                partyIds.Add(organizationPartyId);
            }

            // ---------------------------------------------------------
            // (B) Build EF query to find AcctgTrans + AcctgTransEntry 
            //     matching the script's conditions:
            //       - partyIds
            //       - glAccountId
            //       - isPosted (optional)
            //       - transactionDate in [start, end)
            // ---------------------------------------------------------
            // We'll replicate "AcctgTransAndEntries" by joining 
            // AcctgTransEntries -> AcctgTrans. 
            // Then we filter as the script does.
            var query = from ate in _context.AcctgTransEntries
                join act in _context.AcctgTrans
                    on ate.AcctgTransId equals act.AcctgTransId
                where partyIds.Contains(ate.OrganizationPartyId)
                      && ate.GlAccountId == glAccountId
                      && act.TransactionDate >= customTimePeriodStartDate
                      && act.TransactionDate < customTimePeriodEndDate
                select new
                {
                    ate.OrganizationPartyId,
                    ate.GlAccountId,
                    ate.DebitCreditFlag,
                    ate.Amount,
                    act.AcctgTransId,
                    act.TransactionDate,
                    act.GlFiscalTypeId,
                    act.IsPosted
                };

            // If isPosted is not empty, filter on that
            if (!string.IsNullOrEmpty(isPosted))
            {
                // Convert the boolean postedFlag to "Y" or "N"
                bool postedFlag = (isPosted == "Y");
                var postedFlagString = postedFlag ? "Y" : "N";

                query = query.Where(x => x.IsPosted == postedFlagString);
            }

            // Execute the query
            var rawEntries = await query.ToListAsync();

            // ---------------------------------------------------------
            // (C) Iterate over each row, summing debit vs. credit
            // ---------------------------------------------------------
            decimal debitTotal = 0m;
            decimal creditTotal = 0m;

            // We'll also store them in result.AcntgTransAndEntries
            foreach (var row in rawEntries)
            {
                // Convert to DTO for returning to caller if needed
                var dto = new AcctgTransAndEntryDto
                {
                    OrganizationPartyId = row.OrganizationPartyId,
                    GlAccountId = row.GlAccountId,
                    DebitCreditFlag = row.DebitCreditFlag,
                    Amount = (decimal)row.Amount,
                    AcctgTransId = row.AcctgTransId,
                    TransactionDate = (DateTime)row.TransactionDate,
                    GlFiscalTypeId = row.GlFiscalTypeId,
                    IsPosted = row.IsPosted
                };
                result.AcctgTransAndEntries.Add(dto);

                // Tally Debits vs. Credits
                if (row.DebitCreditFlag == "D")
                {
                    debitTotal += (decimal)row.Amount;
                }
                else if (row.DebitCreditFlag == "C")
                {
                    creditTotal += (decimal)row.Amount;
                }
            }

            // ---------------------------------------------------------
            // (D) Round or scale if needed (the script does decimal-scale, rounding-mode)
            //     For demonstration, we just keep them as is. 
            //     Or use built-in .Round(...) if you prefer:
            // ---------------------------------------------------------
            var finalDebitTotal = decimal.Round(debitTotal, 2); // example rounding
            var finalCreditTotal = decimal.Round(creditTotal, 2); // example rounding

            // (E) debitCreditDifference = debitTotal - creditTotal
            var difference = finalDebitTotal - finalCreditTotal;

            // ---------------------------------------------------------
            // (F) Populate final result object
            // ---------------------------------------------------------
            result.DebitTotal = finalDebitTotal;
            result.CreditTotal = finalCreditTotal;
            result.DebitCreditDifference = difference;

            return result;
        }
        catch (Exception ex)
        {
            // If any DB error or logic error happens, rethrow or handle
            throw new Exception("Error in GetAcctgTransEntriesAndTransTotal", ex);
        }
    }

    /// <summary>
    /// Generates the "Balance Sheet" using logic that parallels your Groovy script.
    /// 
    /// Business (Accounting) Explanation:
    /// 1) A "Balance Sheet" shows the organization's financial position at a given date.
    /// 2) We gather "Assets", "Liabilities", and "Equity" from the last closed period ("opening balances").
    /// 3) Then we add new transactions (Debits/Credits) since that period up to 'thruDate' 
    ///    to get final net amounts.
    /// 4) We treat "Assets" (including "Current Assets" and "Long-Term Assets") as "debit-based" accounts:
    ///    final balance = (Debits - Credits).
    /// 5) We treat "Liabilities" and "Equity" as "credit-based": final balance = (Credits - Debits).
    /// 6) Finally, we add net income to the "Retained Earnings" account (from an Income Statement) 
    ///    to show updated equity.
    /// </summary>
    public async Task<BalanceSheetViewModel> GenerateBalanceSheet(
        string organizationPartyId,
        DateTime? thruDate,
        string glFiscalTypeId)
    {
        try
        {
            // 1) Validate input
            if (string.IsNullOrEmpty(glFiscalTypeId))
            {
                throw new ArgumentException("glFiscalTypeId is required.");
            }

            if (!thruDate.HasValue)
            {
                // The script sets thruDate = nowTimestamp() if missing
                thruDate = DateTime.Now;
            }

            // 2) Gather party IDs (org + child divisions)
            var partyIds = await GetAssociatedPartyIdsByRelationshipType(organizationPartyId, "GROUP_ROLLUP");
            if (!partyIds.Contains(organizationPartyId))
            {
                partyIds.Add(organizationPartyId);
            }

            // 3) Find the last closed period on or before 'thruDate', 
            //    so we can get fromDate + the customTimePeriod.
            var lastClosedDateResult = await FindLastClosedDate(organizationPartyId, thruDate.Value, null);
            if (lastClosedDateResult?.LastClosedDate == null)
            {
                // The script returns if there's no fromDate
                return null;
            }

            DateTime fromDate = lastClosedDateResult.LastClosedDate.Value;
            var lastClosedTimePeriod = lastClosedDateResult.LastClosedTimePeriod;

            // 4) Gather the GL class IDs for each category 
            //    (like "ASSET", "CURRENT_ASSET", "CONTRA_ASSET", "LIABILITY", "EQUITY", etc.)
            var assetAccountClassIds = await GetDescendantGlAccountClassIds("ASSET");
            var contraAssetAccountClassIds = await GetDescendantGlAccountClassIds("CONTRA_ASSET");
            var currentAssetAccountClassIds = await GetDescendantGlAccountClassIds("CURRENT_ASSET");
            var longtermAssetAccountClassIds = await GetDescendantGlAccountClassIds("LONGTERM_ASSET");
            var liabilityAccountClassIds = await GetDescendantGlAccountClassIds("LIABILITY");
            var currentLiabilityAccountClassIds = await GetDescendantGlAccountClassIds("CURRENT_LIABILITY");
            var equityAccountClassIds = await GetDescendantGlAccountClassIds("EQUITY");

            var accumDepreciationAccountClassIds = await GetDescendantGlAccountClassIds("ACCUM_DEPRECIATION");
            var accumAmortizationAccountClassIds = await GetDescendantGlAccountClassIds("ACCUM_AMORTIZATION");

            // 5) Get the "opening balances" from GlAccountHistory for each category
            //    i.e. amounts from the last closed period.
            var assetOpening = await GetOpeningBalances(partyIds, assetAccountClassIds, lastClosedTimePeriod);
            var contraAssetOpening =
                await GetOpeningBalances(partyIds, contraAssetAccountClassIds, lastClosedTimePeriod);
            var currentAssetOpening =
                await GetOpeningBalances(partyIds, currentAssetAccountClassIds, lastClosedTimePeriod);
            var longtermAssetOpening =
                await GetOpeningBalances(partyIds, longtermAssetAccountClassIds, lastClosedTimePeriod);
            var liabilityOpening = await GetOpeningBalances(partyIds, liabilityAccountClassIds, lastClosedTimePeriod);
            var currentLiabilityOpening =
                await GetOpeningBalances(partyIds, currentLiabilityAccountClassIds, lastClosedTimePeriod);
            var equityOpening = await GetOpeningBalances(partyIds, equityAccountClassIds, lastClosedTimePeriod);

            var accumDepreciationOpening =
                await GetOpeningBalances(partyIds, accumDepreciationAccountClassIds, lastClosedTimePeriod);
            var accumAmortizationOpening =
                await GetOpeningBalances(partyIds, accumAmortizationAccountClassIds, lastClosedTimePeriod);

            // 6) Build a base EF query for posted transactions in [fromDate, thruDate)
            //    excluding PERIOD_CLOSING, glFiscalTypeId = input, isPosted = true.
            //    We'll do a single query in a helper so we can filter out each class set.
            // Now the result type is IQueryable<AcctgTransEntryJoin> instead of an anonymous type
            var baseQuery = _context.AcctgTransEntries
                .Join(_context.AcctgTrans,
                    ate => ate.AcctgTransId,
                    act => act.AcctgTransId,
                    (ate, act) => new AcctgTransEntryJoin
                    {
                        Ate = ate,
                        Act = act
                    }
                )
                .Where(x =>
                    partyIds.Contains(x.Ate.OrganizationPartyId) &&
                    x.Act.IsPosted == "Y" &&
                    x.Act.GlFiscalTypeId == glFiscalTypeId &&
                    x.Act.AcctgTransTypeId != "PERIOD_CLOSING" &&
                    x.Act.TransactionDate >= fromDate &&
                    x.Act.TransactionDate < thruDate.Value
                );


            // 7) Calculate final balances for each major category,
            //    merging the opening list + new transactions.
            //    For assets => isDebitBased = true, so final = D - C
            //    For liabilities/equity => isDebitBased = false => final = C - D

            // ASSETS
            var assets = await CalculateCategoryBalances(
                baseQuery, assetAccountClassIds, assetOpening, isDebitBased: true);
            var assetBalanceTotal = assets.Sum(a => a.Balance);

            // CURRENT ASSETS
            var currentAssets = await CalculateCategoryBalances(
                baseQuery, currentAssetAccountClassIds, currentAssetOpening, isDebitBased: true);
            var currentAssetBalanceTotal = currentAssets.Sum(a => a.Balance);

            // LONGTERM ASSETS
            var longtermAssets = await CalculateCategoryBalances(
                baseQuery, longtermAssetAccountClassIds, longtermAssetOpening, isDebitBased: true);
            var longtermAssetBalanceTotal = longtermAssets.Sum(a => a.Balance);

            // CONTRA ASSETS (Typically credit-based)
            var contraAssets = await CalculateCategoryBalances(
                baseQuery, contraAssetAccountClassIds, contraAssetOpening, isDebitBased: false);
            var contraAssetBalanceTotal = contraAssets.Sum(a => a.Balance);

            // --- NEW: ACCUMULATED DEPRECIATION (credit-based) ---
            var accumDepreciation = await CalculateCategoryBalances(
                baseQuery, accumDepreciationAccountClassIds, accumDepreciationOpening, isDebitBased: false);
            var accumDepreciationBalanceTotal = accumDepreciation.Sum(a => a.Balance);

            // --- NEW: ACCUMULATED AMORTIZATION (credit-based) ---
            var accumAmortization = await CalculateCategoryBalances(
                baseQuery, accumAmortizationAccountClassIds, accumAmortizationOpening, isDebitBased: false);
            var accumAmortizationBalanceTotal = accumAmortization.Sum(a => a.Balance);

            // LIABILITIES
            var liabilities = await CalculateCategoryBalances(
                baseQuery, liabilityAccountClassIds, liabilityOpening, isDebitBased: false);
            var liabilityBalanceTotal = liabilities.Sum(a => a.Balance);

            // CURRENT LIABILITIES
            var currentLiabilities = await CalculateCategoryBalances(
                baseQuery, currentLiabilityAccountClassIds, currentLiabilityOpening, isDebitBased: false);
            var currentLiabilityBalanceTotal = currentLiabilities.Sum(a => a.Balance);

            // EQUITY
            var equities = await CalculateCategoryBalances(
                baseQuery, equityAccountClassIds, equityOpening, isDebitBased: false);

            // 8) Insert "Retained Earnings" from the net income (prepareIncomeStatement call).
            //    i.e. we add a new "GlAccountBalance" row for that account with netIncome as the balance.
            var netIncomeResult = await PrepareIncomeStatement(
                organizationPartyId, fromDate, thruDate.Value, glFiscalTypeId);

            var retainedEarningsAccount = await GetRetainedEarningsAccount(organizationPartyId);
            if (retainedEarningsAccount != null)
            {
                equities.Add(new GlAccountBalance
                {
                    GlAccountId = retainedEarningsAccount.GlAccountId,
                    AccountCode = retainedEarningsAccount.AccountCode,
                    AccountName = retainedEarningsAccount.AccountName,
                    Balance = netIncomeResult.TotalNetIncome
                });
            }

            var equityBalanceTotal = equities.Sum(e => e.Balance);

            // 9) Liabilities + Equity combined
            var liabilityEquityBalanceTotal = liabilityBalanceTotal + equityBalanceTotal;

            // 10) Build final view model
            var model = new BalanceSheetViewModel
            {
                // Lists
                AssetAccountBalanceList = assets
                    .Concat(contraAssets)
                    .Concat(accumDepreciation)
                    .Concat(accumAmortization)
                    .ToList(),
                LiabilityAccountBalanceList = liabilities.Concat(currentLiabilities).ToList(),
                EquityAccountBalanceList = equities,

                // Totals
                AssetBalanceTotal = assetBalanceTotal,
                LiabilityBalanceTotal = liabilityBalanceTotal,
                EquityBalanceTotal = equityBalanceTotal,
                LiabilityEquityBalanceTotal = liabilityEquityBalanceTotal,

                // Sub-totals
                CurrentAssetBalanceTotal = currentAssetBalanceTotal,
                LongtermAssetBalanceTotal = longtermAssetBalanceTotal,
                ContraAssetBalanceTotal = contraAssetBalanceTotal,
                AccumDepreciationBalanceTotal = accumDepreciationBalanceTotal,
                AccumAmortizationBalanceTotal = accumAmortizationBalanceTotal,

                // Example "BalanceTotalList" lines
                BalanceTotalList = new List<BalanceLineItem>
                {
                    new BalanceLineItem
                    {
                        TotalName = "Accounting Current Assets",
                        Balance = currentAssetBalanceTotal
                    },
                    new BalanceLineItem
                    {
                        TotalName = "Accounting Long Term Assets",
                        Balance = longtermAssetBalanceTotal
                    },
                    // Combine all contra accounts in your total or list them separately:
                    new BalanceLineItem
                    {
                        TotalName = "Accounting Total Contra Assets",
                        Balance = contraAssetBalanceTotal
                    },
                    new BalanceLineItem
                    {
                        TotalName = "Accounting Total Accumulated Depreciation",
                        Balance = accumDepreciationBalanceTotal
                    },
                    new BalanceLineItem
                    {
                        TotalName = "Accounting Total Accumulated Amortization",
                        Balance = accumAmortizationBalanceTotal
                    },
                    new BalanceLineItem
                    {
                        TotalName = "Accounting Total Assets",
                        // If you want to show net assets after subtracting contra accounts:
                        // (Here we sum them directly, so "contra" accounts reduce totals.)
                        Balance = assetBalanceTotal + currentAssetBalanceTotal + longtermAssetBalanceTotal
                                  + contraAssetBalanceTotal
                                  + accumDepreciationBalanceTotal
                                  + accumAmortizationBalanceTotal
                    },
                    new BalanceLineItem
                    {
                        TotalName = "Accounting Current Liabilities",
                        Balance = currentLiabilityBalanceTotal
                    },
                    new BalanceLineItem
                    {
                        TotalName = "Accounting Total Liabilities",
                        Balance = liabilityBalanceTotal
                    },
                    new BalanceLineItem
                    {
                        TotalName = "Accounting Total Equities",
                        Balance = equityBalanceTotal
                    },
                    new BalanceLineItem
                    {
                        TotalName = "Accounting Total Liabilities And Equities",
                        Balance = liabilityEquityBalanceTotal
                    }
                }
            };

            // Return the final "Balance Sheet" output
            return model;
        }
        catch (Exception ex)
        {
            // If anything goes wrong (DB error, missing data), we handle or rethrow:
            throw new Exception("Error generating Balance Sheet", ex);
        }
    }

    /// <summary>
    /// Sums new transactions in [fromDate, thruDate) for a given set of glAccountClassIds,
    /// merges them with the "opening" list, and finalizes the net balance for each account.
    /// 
    /// If isDebitBased = true => finalBalance = D - C,
    /// else => finalBalance = C - D.
    /// 
    /// Returns a new list of GlAccountBalance objects in ascending order by AccountCode.
    /// </summary>
    private async Task<List<GlAccountBalance>> CalculateCategoryBalances(
        IQueryable<AcctgTransEntryJoin> baseQuery, // Now strongly-typed
        List<string> glAccountClassIds,
        List<GlAccountBalance> openingBalances,
        bool isDebitBased)
    {
        // 1) Filter further by glAccountClassId via join to GlAccounts
        //    Because 'baseQuery' is typed, we can do x.Ate.* and x.Act.* safely.
        var query = baseQuery
            .Join(_context.GlAccounts,
                x => x.Ate.GlAccountId,
                gla => gla.GlAccountId,
                (x, gla) => new { x, gla })
            .Where(z => glAccountClassIds.Contains(z.gla.GlAccountClassId))
            .Select(z => new
            {
                z.x.Ate.GlAccountId, // from typed property x.Ate
                z.gla.AccountCode,
                z.gla.AccountName,
                z.x.Ate.DebitCreditFlag,
                z.x.Ate.Amount
            });

        // 2) Execute to get new transactions in memory
        var rows = await query.ToListAsync();

        // 3) Clone "openingBalances" into a new list so we can merge
        var resultList = new List<GlAccountBalance>();
        foreach (var ob in openingBalances)
        {
            resultList.Add(new GlAccountBalance
            {
                GlAccountId = ob.GlAccountId,
                AccountCode = ob.AccountCode,
                AccountName = ob.AccountName,
                D = ob.D,
                C = ob.C,
                Balance = ob.Balance
            });
        }

        // 4) Merge the new transactions:
        //    If the glAccountId doesn't exist in resultList, create it.
        foreach (var row in rows)
        {
            var existing = resultList.FirstOrDefault(a => a.GlAccountId == row.GlAccountId);
            if (existing == null)
            {
                existing = new GlAccountBalance
                {
                    GlAccountId = row.GlAccountId,
                    AccountCode = row.AccountCode,
                    AccountName = row.AccountName,
                    D = 0,
                    C = 0,
                    Balance = 0
                };
                resultList.Add(existing);
            }

            // Tally the new amounts
            if (row.DebitCreditFlag == "D")
            {
                existing.D += (decimal)row.Amount;
            }
            else
            {
                existing.C += (decimal)row.Amount;
            }
        }

        // 5) Compute final "Balance"
        //    For debit-based accounts => final = D - C
        //    For credit-based => final = C - D
        foreach (var acct in resultList)
        {
            if (isDebitBased)
            {
                acct.Balance = acct.D - acct.C;
            }
            else
            {
                acct.Balance = acct.C - acct.D;
            }
        }

        // 6) Sort by AccountCode for consistency
        return resultList.OrderBy(a => a.AccountCode).ToList();
    }

    /// <summary>
    /// Fetches a list of GlAccountBalance from GlAccountHistory
    /// for a given customTimePeriod (the last closed).
    /// 
    /// Business Explanation:
    /// The "opening balances" come from the official ended period. 
    /// We put them in a list so we can later merge new transactions.
    /// 
    /// We only store: 
    ///   - D => postedDebits
    ///   - C => postedCredits
    ///   - Balance => endingBalance
    /// </summary>
    private async Task<List<GlAccountBalance>> GetOpeningBalances(
        List<string> partyIds,
        List<string> glAccountClassIds,
        CustomTimePeriod lastClosedTimePeriod)
    {
        if (lastClosedTimePeriod == null) return new List<GlAccountBalance>();

        var customTimePeriodId = lastClosedTimePeriod.CustomTimePeriodId;

        // Query glAccountHistory + glAccount 
        var rows = await (
            from glah in _context.GlAccountHistories
            join gla in _context.GlAccounts on glah.GlAccountId equals gla.GlAccountId
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

        // Convert each row into a GlAccountBalance
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

    /// <summary>
    /// Looks up the "Retained Earnings" glAccount from "GlAccountTypeDefault"
    /// for the given org if it exists. 
    /// The script does: from("GlAccountTypeDefault").where("glAccountTypeId", "RETAINED_EARNINGS", "organizationPartyId", ...). 
    /// Then getRelatedOne("GlAccount").
    /// </summary>
    private async Task<GlAccount> GetRetainedEarningsAccount(string organizationPartyId)
    {
        // Example EF query
        var glAccountTypeDefault = await _context.GlAccountTypeDefaults
            .Include(x => x.GlAccount)
            .Where(x => x.GlAccountTypeId == "RETAINED_EARNINGS"
                        && x.OrganizationPartyId == organizationPartyId)
            .FirstOrDefaultAsync();

        return glAccountTypeDefault?.GlAccount;
    }

    /// <summary>
    /// Translates the "getAcctgTransEntriesAndTransTotal" minilang service into C# with EF.
    /// 
    /// Business Explanation:
    /// 1) We gather the organization's partyIds by GROUP_ROLLUP.
    /// 2) We filter rows in [customTimePeriodStartDate, customTimePeriodEndDate) for the given glAccountId,
    ///    and optionally isPosted if provided.
    /// 3) We sum amounts where debitCreditFlag == 'D' into debitTotal; if 'C', into creditTotal.
    /// 4) We compute difference = debitTotal - creditTotal, and return everything to the caller.
    /// </summary>
    public async Task<AcctgTransEntriesAndTransTotalResult> GetAcctgTransEntriesAndTransTotal(
        DateTime? customTimePeriodStartDate,
        DateTime? customTimePeriodEndDate,
        string organizationPartyId,
        string isPosted, // optional
        string glAccountId)
    {
        // Prepare the final result object
        var result = new AcctgTransEntriesAndTransTotalResult
        {
            AcctgTransAndEntries = new List<AcctgTransAndEntryDto>(),
            DebitTotal = 0m,
            CreditTotal = 0m,
            DebitCreditDifference = 0m
        };

        try
        {
            // ---------------------------------------------
            // (A) Gather partyIds (org + child) by GROUP_ROLLUP
            // ---------------------------------------------
            var partyIds = await GetAssociatedPartyIdsByRelationshipType(
                organizationPartyId,
                "GROUP_ROLLUP");

            // Add the main org if missing
            if (!partyIds.Contains(organizationPartyId))
            {
                partyIds.Add(organizationPartyId);
            }

            // ---------------------------------------------
            // (B) Build EF query that mirrors "AcctgTransAndEntries"
            // In OFBiz, this entity combines fields from AcctgTrans + AcctgTransEntry
            // We'll do a join in EF and store it in a query.
            // 
            // The script does:
            //   - organizationPartyId in partyIds
            //   - glAccountId == glAccountId
            //   - isPosted == isPosted (ignore if empty)
            //   - transactionDate >= customTimePeriodStartDate
            //   - transactionDate < customTimePeriodEndDate
            // ---------------------------------------------
            var query = from ate in _context.AcctgTransEntries
                join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                where partyIds.Contains(ate.OrganizationPartyId)
                      && ate.GlAccountId == glAccountId
                select new AcctgTransAndEntryDto
                {
                    OrganizationPartyId = ate.OrganizationPartyId,
                    GlAccountId = ate.GlAccountId,
                    DebitCreditFlag = ate.DebitCreditFlag,
                    Amount = (decimal)ate.Amount,
                    TransactionDate = (DateTime)act.TransactionDate,
                    IsPosted = act.IsPosted, // if storing "Y"/"N"
                    AcctgTransId = act.AcctgTransId,
                    GlFiscalTypeId = act.GlFiscalTypeId
                };

            // (B.1) Filter date range
            if (customTimePeriodStartDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate >= customTimePeriodStartDate.Value);
            }

            if (customTimePeriodEndDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate < customTimePeriodEndDate.Value);
            }

            // (B.2) If isPosted is not empty, filter on that
            // The OFBiz script does: <condition-expr field-name="isPosted" operator="equals" from-field="parameters.isPosted" ignore-if-empty="true"/>
            // We'll do the same by ignoring if isPosted is null/empty
            if (!string.IsNullOrEmpty(isPosted))
            {
                // Usually 'Y' or 'N'. If your DB uses bool, you'd convert.
                query = query.Where(x => x.IsPosted == isPosted);
            }

            // (B.3) Now execute the query
            var acctgTransEntries = await query.ToListAsync();

            // ---------------------------------------------
            // (C) Sum debitTotal / creditTotal 
            // ---------------------------------------------
            decimal debitTotal = 0m;
            decimal creditTotal = 0m;

            // We also store each row in the final result
            foreach (var entry in acctgTransEntries)
            {
                // Check the debitCreditFlag
                if (entry.DebitCreditFlag == "D")
                {
                    debitTotal += entry.Amount;
                }
                else if (entry.DebitCreditFlag == "C")
                {
                    creditTotal += entry.Amount;
                }

                // Add to the result's list if you want to return them
                result.AcctgTransAndEntries.Add(entry);
            }

            // ---------------------------------------------
            // (D) Compute difference = debitTotal - creditTotal
            //     Round or scale if needed. We'll skip that step here
            // ---------------------------------------------
            var debitCreditDifference = debitTotal - creditTotal;

            // ---------------------------------------------
            // (E) Put them in the result and return
            // ---------------------------------------------
            result.DebitTotal = debitTotal;
            result.CreditTotal = creditTotal;
            result.DebitCreditDifference = debitCreditDifference;

            return result;
        }
        catch (Exception ex)
        {
            // If anything goes wrong, rethrow or handle
            throw new Exception("Error in GetAcctgTransEntriesAndTransTotal", ex);
        }
    }

    /// <summary>
    /// Translates the OFBiz "getPreviousTimePeriod" service:
    /// 1) Loads "currentTimePeriod" by customTimePeriodId.
    /// 2) Decrements periodNum by 1.
    /// 3) If periodNum >= 0, queries "CustomTimePeriod" 
    ///    with same organizationPartyId, periodTypeId, new periodNum.
    /// 4) Returns the first match as "previousTimePeriod," else null.
    /// </summary>
    public async Task<PreviousTimePeriodResult> GetPreviousTimePeriod(string customTimePeriodId)
    {
        try
        {
            // (A) Load the specified "currentTimePeriod"
            var currentTimePeriod = await _context.CustomTimePeriods
                .Where(ctp => ctp.CustomTimePeriodId == customTimePeriodId)
                .FirstOrDefaultAsync();

            if (currentTimePeriod == null)
            {
                // If not found, we do not have a valid "previousTimePeriod"
                return new PreviousTimePeriodResult
                {
                    PreviousTimePeriod = null
                };
            }

            // (B) Decrement the periodNum
            int periodNum = (int)(currentTimePeriod.PeriodNum - 1);

            // (C) If periodNum > -1, search for that new period
            if (periodNum > -1)
            {
                // (D) Query matching "CustomTimePeriod" 
                // by same org, same type, new periodNum, 
                // e.g. "use-cache=true" in EF is roughly second-level caching, 
                // but we'll rely on normal EF caching or your own approach if needed.
                var previousTimePeriod = await _context.CustomTimePeriods
                    .Where(ctp =>
                        ctp.OrganizationPartyId == currentTimePeriod.OrganizationPartyId &&
                        ctp.PeriodTypeId == currentTimePeriod.PeriodTypeId &&
                        ctp.PeriodNum == periodNum
                    )
                    .OrderBy(ctp => ctp.CustomTimePeriodId) // mimic "first-from-list" (some stable ordering)
                    .FirstOrDefaultAsync();

                // Return it (can be null if none found)
                return new PreviousTimePeriodResult
                {
                    PreviousTimePeriod = previousTimePeriod
                };
            }
            else
            {
                // If periodNum <= -1, there's no "previous" year
                return new PreviousTimePeriodResult
                {
                    PreviousTimePeriod = null
                };
            }
        }
        catch (Exception ex)
        {
            // Log or handle as appropriate
            throw new Exception("Error in GetPreviousTimePeriod", ex);
        }
    }

    /// <summary>
    /// Translates the original Groovy script into typed C# parameters:
    ///  - organizationPartyId: the main org
    ///  - glAccountId: optional
    ///  - timePeriodId: optional
    ///  - isPosted: "Y", "N", "ALL", or null
    ///  - userLogin: if needed in sub-services
    ///  - timeZone, locale: for date calculations
    /// 
    /// Returns a TrialBalanceResult that holds the final "context" data.
    /// </summary>
    public async Task<GlAccountTrialBalanceResult> GenerateGlAccountTrialBalance(
        string organizationPartyId,
        string glAccountId,
        string timePeriodId,
        string isPosted)
    {
        // Prepare final result
        var result = new GlAccountTrialBalanceResult();
        if (string.IsNullOrEmpty(organizationPartyId))
        {
            // If no org found, exit
            return result;
        }

        // 1) We only want "FISCAL_YEAR" periods
        var onlyIncludePeriodTypeIdList = new List<string> { "FISCAL_YEAR" };

        // findCustomTimePeriods -> fetch first
        // NOTE: We pass 'null' for excludeNoOrganizationPeriods
        var customTimePeriodList = await _generalLedgerService.FindCustomTimePeriods(
            DateTime.Now,
            organizationPartyId,
            null, // excludeNoOrganizationPeriods => pass null if not needed
            onlyIncludePeriodTypeIdList
        );
        if (customTimePeriodList.Any())
        {
            // store the first ID
            result.TimePeriodId = customTimePeriodList.First().CustomTimePeriodId;
        }

        // For decimal scale/rounding (hardcoded or from config)
        int decimals = 2;
        MidpointRounding rounding = MidpointRounding.AwayFromZero;

        // 2) If glAccountId is given, load & check if it's a debit
        GlAccount glAccount = null;
        if (!string.IsNullOrEmpty(glAccountId))
        {
            glAccount = await _context.GlAccounts
                .Where(g => g.GlAccountId == glAccountId)
                .FirstOrDefaultAsync();
            if (glAccount != null)
            {
                bool isDebit = await _acctgMiscService.IsDebitAccount(glAccount.GlAccountTypeId);
                result.IsDebitAccount = isDebit;
                result.GlAccount = glAccount;
            }
        }

        decimal openingBalance = 0m;
        CustomTimePeriod currentTimePeriod = null;

        // 3) If timePeriodId is present, load that period & compute opening
        if (!string.IsNullOrEmpty(timePeriodId))
        {
            // load currentTimePeriod
            currentTimePeriod = await _context.CustomTimePeriods
                .Where(ctp => ctp.CustomTimePeriodId == timePeriodId)
                .FirstOrDefaultAsync();

            // getPreviousTimePeriod
            var prevResult = await GetPreviousTimePeriod(timePeriodId);
            var previousTimePeriod = prevResult.PreviousTimePeriod;
            if (previousTimePeriod != null && !string.IsNullOrEmpty(glAccountId))
            {
                // load GlAccountHistory
                var glAccountHistory = await _context.GlAccountHistories
                    .Where(h =>
                        h.CustomTimePeriodId == previousTimePeriod.CustomTimePeriodId &&
                        h.GlAccountId == glAccountId &&
                        h.OrganizationPartyId == organizationPartyId)
                    .FirstOrDefaultAsync();

                if (glAccountHistory != null && glAccountHistory.EndingBalance.HasValue)
                {
                    openingBalance = glAccountHistory.EndingBalance.Value;
                }
            }
        }

        result.OpeningBalance = openingBalance;
        result.CurrentTimePeriod = currentTimePeriod;

        // 4) If currentTimePeriod is set, do month by month
        if (currentTimePeriod != null && currentTimePeriod.FromDate.HasValue)
        {
            // We'll gather the final list of monthly data
            // NOTE: List of AcctgTransEntriesAndTransTotalResult
            var glAcctgTrialBalanceList = new List<AcctgTransEntriesAndTransTotalResult>();

            // Start from the fromDate
            var fromDate = currentTimePeriod.FromDate.Value;
            var customTimePeriodStartDate = MonthStart(fromDate);
            var customTimePeriodEndDate = MonthEnd(fromDate);

            var endOfPeriod = currentTimePeriod.ThruDate ?? DateTime.MaxValue;

            decimal totalOfYearToDateDebit = 0m;
            decimal totalOfYearToDateCredit = 0m;
            decimal balanceOfTheAcctgForYear = openingBalance;

            // We'll loop while customTimePeriodEndDate <= .ThruDate
            while (customTimePeriodEndDate <= endOfPeriod)
            {
                // If "ALL".Equals(isPosted) => isPosted = ""
                if ("ALL".Equals(isPosted, StringComparison.OrdinalIgnoreCase))
                {
                    isPosted = "";
                }

                // call getAcctgTransEntriesAndTransTotal
                // which returns AcctgTransEntriesAndTransTotalResult
                var monthlyResult = await GetAcctgTransEntriesAndTransTotal(
                    customTimePeriodStartDate,
                    customTimePeriodEndDate,
                    organizationPartyId,
                    isPosted,
                    glAccountId);

                // sum year-to-date
                totalOfYearToDateDebit += monthlyResult.DebitTotal;
                monthlyResult.TotalOfYearToDateDebit =
                    decimal.Round(totalOfYearToDateDebit, decimals, rounding);

                totalOfYearToDateCredit += monthlyResult.CreditTotal;
                monthlyResult.TotalOfYearToDateCredit =
                    decimal.Round(totalOfYearToDateCredit, decimals, rounding);

                // if isDebitAccount => balance = monthlyResult.DebitCreditDifference
                // else => balance = -1 * difference
                decimal thisBalance;
                if (result.IsDebitAccount == true)
                {
                    thisBalance = monthlyResult.DebitCreditDifference;
                }
                else
                {
                    thisBalance = -1 * monthlyResult.DebitCreditDifference;
                }

                monthlyResult.Balance = thisBalance;

                // add to running YTD
                balanceOfTheAcctgForYear += thisBalance;
                monthlyResult.BalanceOfTheAcctgForYear =
                    decimal.Round(balanceOfTheAcctgForYear, decimals, rounding);

                glAcctgTrialBalanceList.Add(monthlyResult);

                // next month
                customTimePeriodStartDate = customTimePeriodStartDate.AddMonths(1);
                customTimePeriodEndDate = MonthEnd(customTimePeriodStartDate);
            }

            result.GlAcctgTrialBalanceList = glAcctgTrialBalanceList;
        }

        return result;
    }

    /// <summary>
    /// Finds the start of the given month (00:00 on day 1).
    /// Equivalent to "UtilDateTime.getMonthStart" in your script.
    /// </summary>
    private DateTime MonthStart(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
    }

    /// <summary>
    /// Finds the end of the given month (23:59:59 on the last day).
    /// Equivalent to "UtilDateTime.getMonthEnd".
    /// </summary>
    private DateTime MonthEnd(DateTime dateTime)
    {
        // Go to 1st of next month, subtract a day
        var firstNextMonth = new DateTime(dateTime.Year, dateTime.Month, 1, 23, 59, 59, dateTime.Kind)
            .AddMonths(1);
        return firstNextMonth.AddDays(-1);
    }

    public ComparativeBalanceSheetResult MergeComparativeBalanceSheet(
        List<AccountBalanceForComparative> assetAccountBalanceList1,
        List<AccountBalanceForComparative> assetAccountBalanceList2,
        List<AccountBalanceForComparative> liabilityAccountBalanceList1,
        List<AccountBalanceForComparative> liabilityAccountBalanceList2,
        List<AccountBalanceForComparative> equityAccountBalanceList1,
        List<AccountBalanceForComparative> equityAccountBalanceList2,
        List<AccountBalanceForComparative> balanceTotalList1,
        List<AccountBalanceForComparative> balanceTotalList2)
    {
        // 1) Merge "asset" account balances
        var mergedAssets = MergeTwoPeriods(
            assetAccountBalanceList1, assetAccountBalanceList2,
            a => a.GlAccountId,
            (key, item, isPeriod1) => new ComparativeAccountBalance
            {
                // In your original code, you used .Balance and .TotalName
                // Now it’s valid, because 'item' is an AccountBalanceForComparative
                GlAccountId = item.GlAccountId,
                AccountCode = item.AccountCode,
                AccountName = item.AccountName,
                Balance1 = isPeriod1 ? item.Balance : 0,
                Balance2 = isPeriod1 ? 0 : item.Balance
            },
            (existing, item) =>
            {
                existing.Balance2 = item.Balance;
                return existing;
            },
            c => c.AccountCode
        );

        // 2) Merge "liability" account balances
        var mergedLiabilities = MergeTwoPeriods(
            liabilityAccountBalanceList1, liabilityAccountBalanceList2,
            a => a.GlAccountId,
            (key, item, isPeriod1) => new ComparativeAccountBalance
            {
                GlAccountId = item.GlAccountId,
                AccountCode = item.AccountCode,
                AccountName = item.AccountName,
                Balance1 = isPeriod1 ? item.Balance : 0,
                Balance2 = isPeriod1 ? 0 : item.Balance
            },
            (existing, item) =>
            {
                existing.Balance2 = item.Balance;
                return existing;
            },
            c => c.AccountCode
        );

        // 3) Merge "equity" account balances
        var mergedEquities = MergeTwoPeriods(
            equityAccountBalanceList1, equityAccountBalanceList2,
            a => a.GlAccountId,
            (key, item, isPeriod1) => new ComparativeAccountBalance
            {
                GlAccountId = item.GlAccountId,
                AccountCode = item.AccountCode,
                AccountName = item.AccountName,
                Balance1 = isPeriod1 ? item.Balance : 0,
                Balance2 = isPeriod1 ? 0 : item.Balance
            },
            (existing, item) =>
            {
                existing.Balance2 = item.Balance;
                return existing;
            },
            c => c.AccountCode
        );

        // 4) Merge "totals" (using "TotalName" as key in your original code)
        var mergedTotals = MergeTwoPeriods(
            balanceTotalList1, balanceTotalList2,
            t => t.TotalName,
            (key, item, isPeriod1) => new ComparativeBalanceTotal
            {
                TotalName = item.TotalName,
                Balance1 = isPeriod1 ? item.Balance : 0,
                Balance2 = isPeriod1 ? 0 : item.Balance
            },
            (existing, item) =>
            {
                existing.Balance2 = item.Balance;
                return existing;
            },
            t => t.TotalName
        );

        // Return the final merged data
        return new ComparativeBalanceSheetResult
        {
            AssetAccountBalanceList = mergedAssets,
            LiabilityAccountBalanceList = mergedLiabilities,
            EquityAccountBalanceList = mergedEquities,
            BalanceTotalList = mergedTotals
        };
    }


    /// <summary>
    /// Merges two lists (representing two periods of data) into a single comparative list.
    /// 
    /// Parameters:
    ///   list1, list2: the two lists to merge
    ///   keySelector: identifies the "primary key" (e.g. GlAccountId or TotalName)
    ///   createComparative: given (key, item, isPeriod1) => builds the initial comparative object
    ///   combine: merges an existing comparative object with a second item from the other list
    ///   orderSelector: how to sort the final list (e.g., by accountCode).
    /// </summary>
    private List<U> MergeTwoPeriods<T, U, K>(
        List<T> list1,
        List<T> list2,
        Func<T, K> keySelector,
        Func<K, T, bool, U> createComparative,
        Func<U, T, U> combine,
        Func<U, object> orderSelector)
    {
        var map = new Dictionary<K, U>();

        // 1) Populate from list1
        foreach (var item in list1)
        {
            var key = keySelector(item);
            // Create a new comparative object with Period1 data
            var comparative = createComparative(key, item, true);
            map[key] = comparative;
        }

        // 2) Merge from list2
        foreach (var item in list2)
        {
            var key = keySelector(item);
            if (!map.TryGetValue(key, out U existingComparative))
            {
                // Create new comparative with Period2 data
                var newComparative = createComparative(key, item, false);
                map[key] = newComparative;
            }
            else
            {
                // Combine existing object with Period2 item
                map[key] = combine(existingComparative, item);
            }
        }

        // 3) Sort & return
        var sorted = map.Values.OrderBy(orderSelector).ToList();
        return sorted;
    }

    public async Task<ComparativeBalanceSheetResult> GenerateComparativeBalanceSheet(
        string organizationPartyId,
        DateTime? period1ThruDate,
        string period1GlFiscalTypeId,
        DateTime? period2ThruDate,
        string period2GlFiscalTypeId)
    {
        // ---------------------------------------------------------------------
        // 1) Generate SINGLE-PERIOD for "Period1"
        // ---------------------------------------------------------------------
        var period1View = await GenerateBalanceSheet(
            organizationPartyId,
            period1ThruDate,
            period1GlFiscalTypeId
        );
        if (period1View == null)
        {
            // If single-run logic returned null (no closed period?), handle that
            throw new Exception("Could not generate Period1 balance sheet (no last closed period?).");
        }

        // ---------------------------------------------------------------------
        // 2) Generate SINGLE-PERIOD for "Period2"
        // ---------------------------------------------------------------------
        var period2View = await GenerateBalanceSheet(
            organizationPartyId,
            period2ThruDate,
            period2GlFiscalTypeId
        );
        if (period2View == null)
        {
            throw new Exception("Could not generate Period2 balance sheet (no last closed period?).");
        }

        // ---------------------------------------------------------------------
        // 3) Merge "assets", "liabilities", "equity" + "totals" into comparative
        // ---------------------------------------------------------------------
        //    We'll define small helper methods below to do the merging so that:
        //    - For each matching GlAccountId, we put period1's value in Balance1
        //      and period2's value in Balance2.
        //    - For "totals," we do the same but keyed by "TotalName."

        var mergedAssets = MergeAccountBalances(
            period1View.AssetAccountBalanceList,
            period2View.AssetAccountBalanceList
        );
        var mergedLiabilities = MergeAccountBalances(
            period1View.LiabilityAccountBalanceList,
            period2View.LiabilityAccountBalanceList
        );
        var mergedEquities = MergeAccountBalances(
            period1View.EquityAccountBalanceList,
            period2View.EquityAccountBalanceList
        );
        var mergedTotals = MergeBalanceTotals(
            period1View.BalanceTotalList,
            period2View.BalanceTotalList
        );

        // ---------------------------------------------------------------------
        // 4) Build final "ComparativeBalanceSheetResult" and return
        // ---------------------------------------------------------------------
        return new ComparativeBalanceSheetResult
        {
            AssetAccountBalanceList = mergedAssets,
            LiabilityAccountBalanceList = mergedLiabilities,
            EquityAccountBalanceList = mergedEquities,
            BalanceTotalList = mergedTotals
        };
    }

    /// <summary>
    /// Merges two sets of GlAccountBalance (Period1, Period2) into 
    /// a list of ComparativeAccountBalance with Balance1/Balance2.
    /// We assume 'GlAccountId' is the unique key.
    /// </summary>
    private List<ComparativeAccountBalance> MergeAccountBalances(
        List<GlAccountBalance> period1List,
        List<GlAccountBalance> period2List)
    {
        // Dictionary keyed by GlAccountId
        var map = new Dictionary<string, ComparativeAccountBalance>();

        // 1) Load period1
        foreach (var acct in period1List)
        {
            if (!map.ContainsKey(acct.GlAccountId))
            {
                map[acct.GlAccountId] = new ComparativeAccountBalance
                {
                    GlAccountId = acct.GlAccountId,
                    AccountCode = acct.AccountCode,
                    AccountName = acct.AccountName,
                    Balance1 = acct.Balance,
                    Balance2 = 0m
                };
            }
        }

        // 2) Merge period2
        foreach (var acct in period2List)
        {
            if (!map.TryGetValue(acct.GlAccountId, out var existing))
            {
                // No entry from period1 => create new, with Balance1=0
                existing = new ComparativeAccountBalance
                {
                    GlAccountId = acct.GlAccountId,
                    AccountCode = acct.AccountCode,
                    AccountName = acct.AccountName,
                    Balance1 = 0m,
                    Balance2 = acct.Balance
                };
                map[acct.GlAccountId] = existing;
            }
            else
            {
                // Update existing entry with period2's balance
                existing.Balance2 = acct.Balance;
            }
        }

        // 3) Sort by AccountCode (optional) for consistent display
        return map.Values.OrderBy(x => x.AccountCode).ToList();
    }

    /// <summary>
    /// Merges two sets of BalanceLineItem (Period1, Period2) into 
    /// a list of ComparativeBalanceTotal with Balance1/Balance2.
    /// We assume 'TotalName' is the unique key.
    /// </summary>
    private List<ComparativeBalanceTotal> MergeBalanceTotals(
        List<BalanceLineItem> period1Totals,
        List<BalanceLineItem> period2Totals)
    {
        // Dictionary keyed by TotalName
        var map = new Dictionary<string, ComparativeBalanceTotal>();

        // 1) Load period1
        foreach (var tot in period1Totals)
        {
            if (!map.ContainsKey(tot.TotalName))
            {
                map[tot.TotalName] = new ComparativeBalanceTotal
                {
                    TotalName = tot.TotalName,
                    Balance1 = tot.Balance,
                    Balance2 = 0m
                };
            }
        }

        // 2) Merge period2
        foreach (var tot in period2Totals)
        {
            if (!map.TryGetValue(tot.TotalName, out var existing))
            {
                existing = new ComparativeBalanceTotal
                {
                    TotalName = tot.TotalName,
                    Balance1 = 0m,
                    Balance2 = tot.Balance
                };
                map[tot.TotalName] = existing;
            }
            else
            {
                existing.Balance2 = tot.Balance;
            }
        }

        // 3) Sort by TotalName if desired
        return map.Values.OrderBy(x => x.TotalName).ToList();
    }
}