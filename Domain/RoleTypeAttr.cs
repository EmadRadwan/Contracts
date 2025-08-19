namespace Domain;

public class RoleTypeAttr
{
    public string RoleTypeId { get; set; } = null!;
    public string AttrName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public RoleType RoleType { get; set; } = null!;
}