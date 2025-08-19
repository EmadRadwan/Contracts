namespace Domain;

public class ServerHit
{
    public string VisitId { get; set; } = null!;
    public string ContentId { get; set; } = null!;
    public DateTime HitStartDateTime { get; set; }
    public string HitTypeId { get; set; } = null!;
    public int? NumOfBytes { get; set; }
    public int? RunningTimeMillis { get; set; }
    public string? UserLoginId { get; set; }
    public string? StatusId { get; set; }
    public string? RequestUrl { get; set; }
    public string? ReferrerUrl { get; set; }
    public string? ServerIpAddress { get; set; }
    public string? ServerHostName { get; set; }
    public string? InternalContentId { get; set; }
    public string? PartyId { get; set; }
    public string? IdByIpContactMechId { get; set; }
    public string? RefByWebContactMechId { get; set; }

    public ServerHitType HitType { get; set; } = null!;
    public Visit Visit { get; set; } = null!;
}