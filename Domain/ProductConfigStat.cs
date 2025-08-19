namespace Domain;

public class ProductConfigStat
{
    public string ConfigId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public int? NumOfConfs { get; set; }
    public string? ConfigTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product Product { get; set; } = null!;
}