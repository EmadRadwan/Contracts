namespace Domain;

public class ProdCatalogRole
{
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public string ProdCatalogId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyRole PartyRole { get; set; } = null!;
    public ProdCatalog ProdCatalog { get; set; } = null!;
}