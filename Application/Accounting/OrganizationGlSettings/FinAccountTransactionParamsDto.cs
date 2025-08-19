namespace Application.Accounting.OrganizationGlSettings;

public class FinAccountTransationParamsDto
{
    public string FinAccountId { get; set; }
    public string? FinAccountTransTypeId { get; set; }
    public string? StatusId { get; set; }
    public string? GlReconciliationId { get; set; }
    public DateTime? FromTransactionDate { get; set; }
    public DateTime? ThruTransactionDate { get; set; }
    public DateTime? FromEntryDate { get; set; }
    public DateTime? ThruEntryDate { get; set; }
    public decimal? OpeningBalance { get; set; }
}