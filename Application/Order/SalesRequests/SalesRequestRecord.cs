using System.ComponentModel.DataAnnotations;

namespace Application.Order.SalesRequests;

public class SalesRequestRecord
{
    [Key]
    public string SalesRequestId { get; set; } = null!;
    public string ApartmentId { get; set; } = null!;
    public string ApartmentName { get; set; } = null!;
    public string ProductTypeDescription { get; set; } = null!;
    public string FromPartyId { get; set; } = null!;
    public string FromPartyName { get; set; } = null!;

    public decimal? ApartmentPricePerM2 { get; set; }
    public decimal? GardenPricePerM2 { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? Discount { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Comments { get; set; }
    public decimal? AdvancePayment { get; set; }
    public int? NumberOfInstallments { get; set; }
    public DateTime? DateOfFirstInstallment { get; set; }
    public int? MonthsBetweenInstallments { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string FloorNumber { get; set; } = string.Empty;
    public decimal ApartmentSpaceM2 { get; set; }
    public decimal? GardenSpaceM2 { get; set; }
    public string ApartmentStatusDescription { get; set; } = string.Empty;
    
    public decimal? MaintenanceDeposit { get; set; }
    public string StatusId { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;


    // Audit
    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
}