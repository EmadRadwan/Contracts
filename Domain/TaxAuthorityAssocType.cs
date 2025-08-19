namespace Domain;

public class TaxAuthorityAssocType
{
    public TaxAuthorityAssocType()
    {
        TaxAuthorityAssocs = new HashSet<TaxAuthorityAssoc>();
    }

    public string TaxAuthorityAssocTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<TaxAuthorityAssoc> TaxAuthorityAssocs { get; set; }
}