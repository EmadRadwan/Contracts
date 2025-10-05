namespace Application.Accounting.Transactions;

public class ValidateMultiAcctgTransResult
{
    public bool IsBalanced { get; set; }
    public List<string> Messages { get; set; } = new List<string>();
}