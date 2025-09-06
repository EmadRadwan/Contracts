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
    public string? Description { get; set; } // Maps to WorkEffort.Description
    public string? UomDescription { get; set; } // Maps to WorkEffort.Description
    public string? ProductName { get; set; } // Maps to WorkEffort.Description
    public decimal Quantity { get; set; } // Maps to WorkEffort.Quantity
    public decimal UnitPrice { get; set; } // Maps to WorkEffort.Rate
    public decimal TotalAmount { get; set; } // Maps to WorkEffort.TotalAmount
    public decimal? Discount { get; set; } // Maps to WorkEffort.Discount
    public decimal? Deductions { get; set; } // Maps to WorkEffort.Discount
    public decimal? Insurance { get; set; } // Maps to WorkEffort.Insurance
    public decimal? CompletionPercentage { get; set; } // Maps to WorkEffort.CompletionPercentage
    public string? Notes { get; set; } // Maps to WorkEffort.Notes
    public DateTime? ProcurementDate { get; set; } // Maps to WorkEffort.ProcurementDate (ISO string)
    public string? FacilityId { get; set; } // Maps to WorkEffort.FacilityId
    public bool IsContractorPurchased { get; set; } // Maps to WorkEffort.IsContractorPurchased
    public bool IsDeleted { get; set; } // Maps to WorkEffort.IsContractorPurchased
}