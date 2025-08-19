namespace Domain;

public class Visitor
{
    public Visitor()
    {
        Visits = new HashSet<Visit>();
    }

    public string VisitorId { get; set; } = null!;
    public string? UserLoginId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public string? PartyId { get; set; }

    public ICollection<Visit> Visits { get; set; }
}