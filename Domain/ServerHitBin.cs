namespace Domain;

public class ServerHitBin
{
    public string ServerHitBinId { get; set; } = null!;
    public string? ContentId { get; set; }
    public string? HitTypeId { get; set; }
    public string? ServerIpAddress { get; set; }
    public string? ServerHostName { get; set; }
    public DateTime? BinStartDateTime { get; set; }
    public DateTime? BinEndDateTime { get; set; }
    public int? NumberHits { get; set; }
    public int? TotalTimeMillis { get; set; }
    public int? MinTimeMillis { get; set; }
    public int? MaxTimeMillis { get; set; }
    public string? InternalContentId { get; set; }

    public ServerHitType? HitType { get; set; }
}