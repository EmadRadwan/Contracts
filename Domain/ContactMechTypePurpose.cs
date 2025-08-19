namespace Domain;

public class ContactMechTypePurpose
{
    public string ContactMechTypeId { get; set; } = null!;
    public string ContactMechPurposeTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMechPurposeType ContactMechPurposeType { get; set; } = null!;
    public ContactMechType ContactMechType { get; set; } = null!;
}