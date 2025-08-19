namespace Domain;

public class ContentApproval
{
    public string ContentApprovalId { get; set; } = null!;
    public string? ContentId { get; set; }
    public string? ContentRevisionSeqId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? ApprovalStatusId { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public int? SequenceNum { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public StatusItem? ApprovalStatus { get; set; }
    public Content? Content { get; set; }
    public Party? Party { get; set; }
    public RoleType? RoleType { get; set; }
}