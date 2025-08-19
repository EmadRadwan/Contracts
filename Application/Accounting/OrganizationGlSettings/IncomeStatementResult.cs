namespace Application.Accounting.OrganizationGlSettings;

public class IncomeStatementResult
{
    public decimal TotalNetIncome { get; set; }
    public GlAccountTotalsMap GlAccountTotalsMap { get; set; }
}