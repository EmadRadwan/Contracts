namespace Domain;

public class SurveyMultiRespColumn
{
    public string SurveyId { get; set; } = null!;
    public string SurveyMultiRespId { get; set; } = null!;
    public string SurveyMultiRespColId { get; set; } = null!;
    public string? ColumnTitle { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SurveyMultiResp Survey { get; set; } = null!;
}