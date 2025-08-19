namespace Application.Accounting.Services.Models;

public class GlAccountTransactionDetails
{
    public decimal OpeningBalance { get; set; }
    public decimal PostedDebits { get; set; }
    public decimal PostedCredits { get; set; }
    public decimal EndingBalance { get; set; }
    public string GlAccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public string GlAccountClassId { get; set; }
    public List<TransactionEntryDto> Transactions { get; set; }
}
