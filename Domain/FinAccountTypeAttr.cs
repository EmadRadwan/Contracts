namespace Domain;

public class FinAccountTypeAttr
{
    public string FinAccountTypeId { get; set; } = null!;
    public string AttrName { get; set; } = null!;
    public string? AttrValue { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FinAccountType FinAccountType { get; set; } = null!;
}