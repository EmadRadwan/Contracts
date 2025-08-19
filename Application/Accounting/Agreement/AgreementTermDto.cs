namespace Application.Shipments.Agreement;

public class AgreementTermDto
{
    public string AgreementTermId { get; set; } = null!;
    public string? TermTypeId { get; set; }
    public string? TermTypeDescription { get; set;}
    public string? AgreementId { get; set; }
    public string? AgreementItemSeqId { get; set; }
    public string? InvoiceItemTypeId { get; set; }
    public string? InvoiceItemTypeDescription { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? TermValue { get; set; }
    public int? TermDays { get; set; }
    public string? TextValue { get; set; }
    public double? MinQuantity { get; set; }
    public double? MaxQuantity { get; set; }
    public string? Description { get; set; }
}