namespace Domain;

public class ServiceRate
{
    public string ServiceRateId { get; set; }
    public string MakeId { get; set; }
    public string ModelId { get; set; }
    public string ProductStoreId { get; set; }
    public decimal? Rate { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }


    public ProductCategory MakeProductCategory { get; set; }
    public ProductCategory ModelProductCategory { get; set; }
    public ProductStore ProductStore { get; set; }
}