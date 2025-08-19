namespace Domain;

public class ApplicationSandbox
{
    public string ApplicationId { get; set; } = null!;
    public string? WorkEffortId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public string? RuntimeDataId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public RuntimeDatum? RuntimeData { get; set; }
    public WorkEffortPartyAssignment? WorkEffortPartyAssignment { get; set; }
}