namespace Domain;

public class GlAccountGroupMember
{
    public string GlAccountId { get; set; } = null!;
    public string GlAccountGroupTypeId { get; set; } = null!;
    public string? GlAccountGroupId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccount GlAccount { get; set; } = null!;
    public GlAccountGroup? GlAccountGroup { get; set; }
    public GlAccountGroupType GlAccountGroupType { get; set; } = null!;
}