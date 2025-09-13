using Application.Catalog.Products;

namespace Application.Projects;

public class CertificateItemDto
{
    public string? WorkEffortId { get; set; } // Maps to WorkEffort.WorkEffortId
    public string? WorkEffortParentId { get; set; } // Maps to WorkEffort.WorkEffortParentId
    public string? ProductId { get; set; } // Maps to WorkEffort.ProductId
    public ProductLovDto? ProductIdObject { get; set; } // Maps to WorkEffort.ProductId
    public UomLovDto? QuantityUomObject { get; set; } 
    public string? QuantityUom { get; set; } 
    public decimal? AchievementPercent { get; set; }
    public string? Description { get; set; } 
    public string? UomDescription { get; set; } 
    public string? ProductName { get; set; }
    public decimal Quantity { get; set; } 
    public decimal UnitPrice { get; set; } 
    public decimal TotalAmount { get; set; } 
    public decimal? Discount { get; set; } 
    public decimal? Deductions { get; set; } 
    public decimal? Insurance { get; set; }
    public decimal? TransportationExpenses { get; set; }
    public decimal? Gratuities { get; set; }
    public decimal? Rate { get; set; }
    public decimal? DiscountAmount { get; set; }  // Maps to discount
    public decimal? InsuranceAmount { get; set; } 
    public decimal? CompletionPercentage { get; set; }
    public string? Notes { get; set; }
    public DateTime? ProcurementDate { get; set; }
    public string? FacilityId { get; set; }
    public string FacilityName { get; set; }
    public bool IsDeleted { get; set; } 
}