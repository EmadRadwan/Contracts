foreach (var organizationGlAccount in organizationGlAccounts)
{
    // Remove debug skip if not needed
    // if (organizationGlAccount.GlAccountId != "111700") continue;

    var accountBalance = await ComputeGlAccountBalanceForTimePeriod(
        organizationGlAccount.OrganizationPartyId,
        customTimePeriod.CustomTimePeriodId,
        organizationGlAccount.GlAccountId
    );

    // REFACTOR: Include account if ending balance exists or there was activity
    // Purpose: Prevent omission of accounts with opening balance only
    if (accountBalance.EndingBalance != 0 || 
        accountBalance.PostedDebits != 0 || 
        accountBalance.PostedCredits != 0)
    {
        var balance = new AccountBalance
        {
            GlAccountId = organizationGlAccount.GlAccountId,
            AccountCode = organizationGlAccount.GlAccount.AccountCode,
            AccountName = organizationGlAccount.GlAccount.AccountNameArabic,
            OpeningBalance = accountBalance.OpeningBalance,
            PostedDebits = accountBalance.PostedDebits,
            PostedCredits = accountBalance.PostedCredits,
            EndingBalance = accountBalance.EndingBalance
        };

        postedDebitsTotal += accountBalance.PostedDebits;
        postedCreditsTotal += accountBalance.PostedCredits;
        accountBalances.Add(balance);
    }
}