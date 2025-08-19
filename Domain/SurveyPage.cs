namespace Domain;

public class SurveyPage
{
    public string SurveyId { get; set; } = null!;
    public string SurveyPageSeqId { get; set; } = null!;
    public string? PageName { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Survey Survey { get; set; } = null!;
}