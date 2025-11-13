namespace Domain;

public class SalesRequest
{
    public SalesRequest()
    {
        // No child collections required for now
    }

    // -----------------------------------------------------------------
    // Primary key
    // -----------------------------------------------------------------
    public string SalesRequestId { get; set; } = null!;

    // -----------------------------------------------------------------
    // 1. FK to Product (the apartment)
    // -----------------------------------------------------------------
    public string ProductId { get; set; } = null!;

    // -----------------------------------------------------------------
    // 2. Pricing (copied from apartment at the moment of request)
    // -----------------------------------------------------------------
    public decimal? ApartmentPricePerM2 { get; set; }
    public decimal? GardenPricePerM2    { get; set; }

    // -----------------------------------------------------------------
    // 3. FK to Party (customer)
    // -----------------------------------------------------------------
    public string CustomerId { get; set; } = null!;

    // -----------------------------------------------------------------
    // 4-11. Business fields
    // -----------------------------------------------------------------
    public DateTime? SaleDate                     { get; set; }
    public decimal?  Discount                     { get; set; }   
    public decimal?  TotalPrice                   { get; set; }
    public string?   Comments                     { get; set; }
    public decimal?  AdvancePayment               { get; set; }
    public int?      NumberOfInstallments         { get; set; }
    public DateTime? DateOfFirstInstallment       { get; set; }
    public int?      DurationBetweenInstallments  { get; set; } 

    // -----------------------------------------------------------------
    // Audit stamps (same pattern as Product)
    // -----------------------------------------------------------------
    public DateTime? LastUpdatedStamp  { get; set; }
    public DateTime? CreatedStamp      { get; set; }

    // -----------------------------------------------------------------
    // Navigation properties
    // -----------------------------------------------------------------
    public virtual Product   Product   { get; set; } = null!;
    public virtual Party     Customer  { get; set; } = null!;
}