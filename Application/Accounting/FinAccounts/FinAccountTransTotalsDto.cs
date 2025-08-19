using Domain;

namespace Application.Accounting.FinAccounts;

public class FinAccountTransTotalsDto
{
    public List<FinAccountTransListDto> FinAccountTransList { get; set; }
    public int SearchedNumberOfRecords { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal CreatedGrandTotal { get; set; }
    public long TotalCreatedTransactions { get; set; }
    public decimal ApprovedGrandTotal { get; set; }
    public long TotalApprovedTransactions { get; set; }
    public decimal CreatedApprovedGrandTotal { get; set; }
    public long TotalCreatedApprovedTransactions { get; set; }
    public decimal GlReconciliationApprovedGrandTotal { get; set; }
}