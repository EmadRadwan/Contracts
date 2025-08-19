namespace Domain;

public class ContentAssoc
{
    public string ContentId { get; set; } = null!;
    public string ContentIdTo { get; set; } = null!;
    public string ContentAssocTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? ContentAssocPredicateId { get; set; }
    public string? DataSourceId { get; set; }
    public int? SequenceNum { get; set; }
    public string? MapKey { get; set; }
    public int? UpperCoordinate { get; set; }
    public int? LeftCoordinate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
    public ContentAssocPredicate? ContentAssocPredicate { get; set; }
    public ContentAssocType ContentAssocType { get; set; } = null!;
    public Content ContentIdToNavigation { get; set; } = null!;
    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public DataSource? DataSource { get; set; }
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
}