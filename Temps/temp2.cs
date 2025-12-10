if (transferAmount > 0)
{
    financialSummary.TotalToBeReceived = Math.Round(transferAmount, 2, MidpointRounding.AwayFromZero);
    financialSummary.TotalToBePaid     = 0m;
}
else if (transferAmount < 0)
{
    financialSummary.TotalToBePaid     = Math.Round(-transferAmount, 2, MidpointRounding.AwayFromZero);
    financialSummary.TotalToBeReceived = 0m;
}
else
{
    financialSummary.TotalToBeReceived = 0m;
    financialSummary.TotalToBePaid     = 0m;
}