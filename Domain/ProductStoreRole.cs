namespace Domain;

public class ProductStoreRole
{
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public string ProductStoreId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyRole PartyRole { get; set; } = null!;
    public ProductStore ProductStore { get; set; } = null!;
}