namespace Domain;

public class SurveyQuestion
{
    public SurveyQuestion()
    {
        SurveyQuestionAppls = new HashSet<SurveyQuestionAppl>();
        SurveyQuestionOptions = new HashSet<SurveyQuestionOption>();
        SurveyResponseAnswers = new HashSet<SurveyResponseAnswer>();
    }

    public string SurveyQuestionId { get; set; } = null!;
    public string? SurveyQuestionCategoryId { get; set; }
    public string? SurveyQuestionTypeId { get; set; }
    public string? Description { get; set; }
    public string? Question { get; set; }
    public string? Hint { get; set; }
    public string? EnumTypeId { get; set; }
    public string? GeoId { get; set; }
    public string? FormatString { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Geo? Geo { get; set; }
    public SurveyQuestionCategory? SurveyQuestionCategory { get; set; }
    public SurveyQuestionType? SurveyQuestionType { get; set; }
    public ICollection<SurveyQuestionAppl> SurveyQuestionAppls { get; set; }
    public ICollection<SurveyQuestionOption> SurveyQuestionOptions { get; set; }
    public ICollection<SurveyResponseAnswer> SurveyResponseAnswers { get; set; }
}