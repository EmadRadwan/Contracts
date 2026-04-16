namespace Application.Order.SalesRequests;

public class SalesRequestDto
{
    // Core fields (mirrors the form)
    public string? SalesRequestId { get; set; }
    public string? ProductId { get; set; }
    public DateOnly? SaleDate { get; set; }
    public string? FromPartyId { get; set; }
    public string? EmployeePartyId { get; set; }

    public decimal? ApartmentPricePerM2 { get; set; }
    public decimal? GardenPricePerM2 { get; set; }
    public decimal? Discount { get; set; }
    public decimal? TotalPrice { get; set; }
   

    public decimal? AdvancePayment { get; set; }
    public decimal? MaintenanceDeposit { get; set; }
    public decimal? AdvancePercent { get; set; }
    public decimal? MaintenancePercent { get; set; }
    public int? NumberOfInstallments { get; set; }
    public DateOnly? DateOfFirstInstallment { get; set; }
    public int? MonthsBetweenInstallments { get; set; }
    public bool? IsChequesDelivered { get; set; }

    public string? Comments { get; set; }

    // -----------------------------------------------------------------
    // Enriched fields returned from the query (descriptions, names…)
    // -----------------------------------------------------------------
    public string? ProductName { get; set; }
    public string? ProjectName { get; set; }
    public decimal? ApartmentSpaceM2 { get; set; }
    public decimal? GardenSpaceM2 { get; set; }
    public string? ApartmentStatusDescription { get; set; }
    public string? FromPartyName { get; set; }
    
    public List<SalesRequestInstallmentDto>? CustomInstallments { get; set; }
}