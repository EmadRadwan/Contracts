namespace Application.Shipments.Agreement;

public class AgreementItemDto 
{
    public string AgreementId { get; set; } = null!;
    public string AgreementItemSeqId { get; set; } = null!;
    public string? AgreementItemTypeId { get; set; }
    public string? AgreementItemTypeDescription { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? CurrencyUomDescription { get; set; }
    public string? AgreementText { get; set; }
    public byte[]? AgreementImage { get; set; }
}