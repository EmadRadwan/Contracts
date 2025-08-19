namespace Domain;

public class PartyQual
{
    public string PartyId { get; set; } = null!;
    public string PartyQualTypeId { get; set; } = null!;
    public string? QualificationDesc { get; set; }
    public string? Title { get; set; }
    public string? StatusId { get; set; }
    public string? VerifStatusId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public PartyQualType PartyQualType { get; set; } = null!;
    public StatusItem? Status { get; set; }
    public StatusItem? VerifStatus { get; set; }
}