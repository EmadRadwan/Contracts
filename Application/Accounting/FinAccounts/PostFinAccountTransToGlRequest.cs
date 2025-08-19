namespace Application.Accounting.FinAccounts;

public class PostFinAccountTransToGlRequest
{
    public string FinAccountTransId { get; set; }
    public string GlAccountId { get; set; }
}