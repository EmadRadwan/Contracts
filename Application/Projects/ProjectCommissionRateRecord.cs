using System.ComponentModel.DataAnnotations;

namespace Application.Projects;

public class ProjectCommissionRateRecord
{
    [Key] public string ProjectCommissionRateId { get; set; } = null!;
    public string ProjectId { get; set; } = null!;
    public string? ProjectName { get; set; }
    public string SaleTypeId { get; set; } = null!;
    public decimal SalesRepPercent { get; set; }
    public decimal ManagerPercent { get; set; }
    public decimal? ExternalCompanyPercent { get; set; }
    public decimal? ExternalSalesRepPercent { get; set; }
    public decimal? ExternalManagerPercent { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
}
