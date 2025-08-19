namespace Domain;

public class ZipSalesTaxLookup
{
    public string ZipCode { get; set; } = null!;
    public string StateCode { get; set; } = null!;
    public string City { get; set; } = null!;
    public string County { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public string? CountyFips { get; set; }
    public string? CountyDefault { get; set; }
    public string? GeneralDefault { get; set; }
    public string? InsideCity { get; set; }
    public string? GeoCode { get; set; }
    public decimal? StateSalesTax { get; set; }
    public decimal? CitySalesTax { get; set; }
    public decimal? CityLocalSalesTax { get; set; }
    public decimal? CountySalesTax { get; set; }
    public decimal? CountyLocalSalesTax { get; set; }
    public decimal? ComboSalesTax { get; set; }
    public decimal? StateUseTax { get; set; }
    public decimal? CityUseTax { get; set; }
    public decimal? CityLocalUseTax { get; set; }
    public decimal? CountyUseTax { get; set; }
    public decimal? CountyLocalUseTax { get; set; }
    public decimal? ComboUseTax { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}