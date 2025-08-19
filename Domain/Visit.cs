namespace Domain;

public class Visit
{
    public Visit()
    {
        ServerHits = new HashSet<ServerHit>();
    }

    public string VisitId { get; set; } = null!;
    public string? VisitorId { get; set; }
    public string? UserLoginId { get; set; }
    public string? UserCreated { get; set; }
    public string? SessionId { get; set; }
    public string? ServerIpAddress { get; set; }
    public string? ServerHostName { get; set; }
    public string? WebappName { get; set; }
    public string? InitialLocale { get; set; }
    public string? InitialRequest { get; set; }
    public string? InitialReferrer { get; set; }
    public string? InitialUserAgent { get; set; }
    public string? UserAgentId { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? ClientHostName { get; set; }
    public string? ClientUser { get; set; }
    public string? ClientIpIspName { get; set; }
    public string? ClientIpPostalCode { get; set; }
    public string? Cookie { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? ClientIpStateProvGeoId { get; set; }
    public string? ClientIpCountryGeoId { get; set; }
    public string? ContactMechId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }

    public UserAgent? UserAgent { get; set; }
    public Visitor? Visitor { get; set; }
    public ICollection<ServerHit> ServerHits { get; set; }
}