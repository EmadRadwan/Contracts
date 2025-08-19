namespace Domain;

public class ContentAssocPredicate
{
    public ContentAssocPredicate()
    {
        ContentAssocs = new HashSet<ContentAssoc>();
    }

    public string ContentAssocPredicateId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ContentAssoc> ContentAssocs { get; set; }
}