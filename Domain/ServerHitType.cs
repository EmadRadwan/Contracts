namespace Domain;

public class ServerHitType
{
    public ServerHitType()
    {
        ServerHitBins = new HashSet<ServerHitBin>();
        ServerHits = new HashSet<ServerHit>();
    }

    public string HitTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ServerHitBin> ServerHitBins { get; set; }
    public ICollection<ServerHit> ServerHits { get; set; }
}