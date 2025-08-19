namespace Domain;

public class ReturnContactMech
{
    public string ReturnId { get; set; } = null!;
    public string ContactMechPurposeTypeId { get; set; } = null!;
    public string ContactMechId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech ContactMech { get; set; } = null!;
    public ContactMechPurposeType ContactMechPurposeType { get; set; } = null!;
    public ReturnHeader Return { get; set; } = null!;
}