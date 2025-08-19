namespace Application.Shipments.Reports;

public class TransactionTotal
{
    public string GlAccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public decimal? OpeningD { get; set; }
    public decimal? OpeningC { get; set; }
    public decimal? D { get; set; }
    public decimal? C { get; set; }
    public decimal? Balance { get; set; }
}