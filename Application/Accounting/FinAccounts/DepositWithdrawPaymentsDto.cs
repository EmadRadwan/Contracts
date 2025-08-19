namespace Application.Accounting.FinAccounts;

public class DepositWithdrawPaymentsDto
{
    public List<string>? PaymentIds { get; set; }
    public string FinAccountId { get; set; }
    public string GroupInOneTransaction { get; set; } // "Y" or null
    public string PaymentGroupTypeId { get; set; }
    public string PaymentGroupName { get; set; }
}
