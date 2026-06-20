namespace Application.Projects;

public class ProjectCommissionRateDto
{
    public string? ProjectCommissionRateId { get; set; }
    public string ProjectId { get; set; } = null!;
    public string SaleTypeId { get; set; } = null!;
    public decimal SalesRepPercent { get; set; }
    public decimal ManagerPercent { get; set; }
    public decimal? ExternalCompanyPercent { get; set; }
    public decimal? ExternalSalesRepPercent { get; set; }
    public decimal? ExternalManagerPercent { get; set; }
}
