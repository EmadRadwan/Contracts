namespace Domain;

public class SurveyResponseAnswer
{
    public string SurveyResponseId { get; set; } = null!;
    public string SurveyQuestionId { get; set; } = null!;
    public string SurveyMultiRespColId { get; set; } = null!;
    public string? SurveyMultiRespId { get; set; }
    public string? BooleanResponse { get; set; }
    public decimal? CurrencyResponse { get; set; }
    public double? FloatResponse { get; set; }
    public int? NumericResponse { get; set; }
    public string? TextResponse { get; set; }
    public string? SurveyOptionSeqId { get; set; }
    public string? ContentId { get; set; }
    public DateTime? AnsweredDate { get; set; }
    public decimal? AmountBase { get; set; }
    public string? AmountBaseUomId { get; set; }
    public double? WeightFactor { get; set; }
    public int? Duration { get; set; }
    public string? DurationUomId { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content? Content { get; set; }
    public SurveyQuestionOption? Survey { get; set; }
    public SurveyQuestion SurveyQuestion { get; set; } = null!;
    public SurveyResponse SurveyResponse { get; set; } = null!;
}