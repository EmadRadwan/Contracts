using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain;

public class FacilityInventoryRecordView
{
    [Key]
    public string FacilityId { get; set; }
    [Key]
    public string FacilityName { get; set; }
    
    [Column("FACILITY_NAME_ARABIC")]
    public string? FacilityNameArabic { get; set; }  // Arabic name for Facility
    [Key]
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public string QuantityUomId { get; set; }
    public decimal QuantityOnHandTotal { get; set; }
    public decimal AvailableToPromiseTotal { get; set; }
    public decimal DefaultPrice { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal ReorderQuantity { get; set; }
    public decimal AvailableToPromiseMinusMinimumStock { get; set; }
    public decimal QuantityOnHandMinusMinimumStock { get; set; }
}
