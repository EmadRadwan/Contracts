namespace Application.Accounting.Accounting;

public class TrialBalanceResult
{
    public decimal DebitTotal { get; set; }
    public decimal CreditTotal { get; set; }
    public decimal DebitCreditDifference { get; set; }
}