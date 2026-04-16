namespace Application.Accounting.OrganizationGlSettings;

public class FinAccountTransationParamsDto
{
    public string FinAccountId { get; set; }
    public string? FinAccountTransTypeId { get; set; }
    public string? StatusId { get; set; }
    public string? GlReconciliationId { get; set; }
    public DateOnly? FromTransactionDate { get; set; }
    public DateOnly? ThruTransactionDate { get; set; }
    public DateOnly? FromEntryDate { get; set; }
    public DateOnly? ThruEntryDate { get; set; }
    public decimal? OpeningBalance { get; set; }
}