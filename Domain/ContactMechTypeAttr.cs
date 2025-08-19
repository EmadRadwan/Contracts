namespace Domain;

public class ContactMechTypeAttr
{
    public string ContactMechTypeId { get; set; } = null!;
    public string AttrName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMechType ContactMechType { get; set; } = null!;
}