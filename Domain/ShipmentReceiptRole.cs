namespace Domain;

public class ShipmentReceiptRole
{
    public string ReceiptId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public PartyRole PartyRole { get; set; } = null!;
    public ShipmentReceipt Receipt { get; set; } = null!;
}