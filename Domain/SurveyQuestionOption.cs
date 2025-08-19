namespace Domain;

public class SurveyQuestionOption
{
    public SurveyQuestionOption()
    {
        SurveyQuestionAppls = new HashSet<SurveyQuestionAppl>();
        SurveyResponseAnswers = new HashSet<SurveyResponseAnswer>();
    }

    public string SurveyQuestionId { get; set; } = null!;
    public string SurveyOptionSeqId { get; set; } = null!;
    public string? Description { get; set; }
    public int? SequenceNum { get; set; }
    public decimal? AmountBase { get; set; }
    public string? AmountBaseUomId { get; set; }
    public double? WeightFactor { get; set; }
    public int? Duration { get; set; }
    public string? DurationUomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SurveyQuestion SurveyQuestion { get; set; } = null!;
    public ICollection<SurveyQuestionAppl> SurveyQuestionAppls { get; set; }
    public ICollection<SurveyResponseAnswer> SurveyResponseAnswers { get; set; }
}