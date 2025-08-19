namespace Application.Services;

public class ServiceRateDto
{
    public string? ServiceRateId { get; set; }
    public string? MakeId { get; set; }
    public string? MakeDescription { get; set; }
    public string? ModelId { get; set; }
    public string? ModelDescription { get; set; }
    public string? ProductStoreId { get; set; }
    public string? ProductStoreName { get; set; }
    public decimal? Rate { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
}