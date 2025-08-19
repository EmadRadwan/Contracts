namespace Domain;

public class InvoiceItemAssocType
{
    public InvoiceItemAssocType()
    {
        InverseParentType = new HashSet<InvoiceItemAssocType>();
        InvoiceItemAssocs = new HashSet<InvoiceItemAssoc>();
    }

    public string InvoiceItemAssocTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InvoiceItemAssocType? ParentType { get; set; }
    public ICollection<InvoiceItemAssocType> InverseParentType { get; set; }
    public ICollection<InvoiceItemAssoc> InvoiceItemAssocs { get; set; }
}