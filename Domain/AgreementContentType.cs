namespace Domain;

public class AgreementContentType
{
    public AgreementContentType()
    {
        AgreementContents = new HashSet<AgreementContent>();
        InverseParentType = new HashSet<AgreementContentType>();
    }

    public string AgreementContentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AgreementContentType? ParentType { get; set; }
    public ICollection<AgreementContent> AgreementContents { get; set; }
    public ICollection<AgreementContentType> InverseParentType { get; set; }
}