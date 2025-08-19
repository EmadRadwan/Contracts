namespace Domain;

public class GoodIdentification
{
    public string GoodIdentificationTypeId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string? IdValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GoodIdentificationType GoodIdentificationType { get; set; } = null!;
    public Product Product { get; set; } = null!;
}