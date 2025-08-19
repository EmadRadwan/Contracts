namespace Domain;

public class PartyProfileDefault
{
    public string PartyId { get; set; } = null!;
    public string ProductStoreId { get; set; } = null!;
    public string? DefaultShipAddr { get; set; }
    public string? DefaultBillAddr { get; set; }
    public string? DefaultPayMeth { get; set; }
    public string? DefaultShipMeth { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public ProductStore ProductStore { get; set; } = null!;
}