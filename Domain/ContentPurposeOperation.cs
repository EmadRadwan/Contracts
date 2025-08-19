namespace Domain;

public class ContentPurposeOperation
{
    public string ContentPurposeTypeId { get; set; } = null!;
    public string ContentOperationId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public string StatusId { get; set; } = null!;
    public string PrivilegeEnumId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContentOperation ContentOperation { get; set; } = null!;
    public ContentPurposeType ContentPurposeType { get; set; } = null!;
    public Enumeration PrivilegeEnum { get; set; } = null!;
    public RoleType RoleType { get; set; } = null!;
    public StatusItem Status { get; set; } = null!;
}