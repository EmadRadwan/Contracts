namespace Domain;

public class SurveyQuestionCategory
{
    public SurveyQuestionCategory()
    {
        InverseParentCategory = new HashSet<SurveyQuestionCategory>();
        SurveyQuestions = new HashSet<SurveyQuestion>();
    }

    public string SurveyQuestionCategoryId { get; set; } = null!;
    public string? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SurveyQuestionCategory? ParentCategory { get; set; }
    public ICollection<SurveyQuestionCategory> InverseParentCategory { get; set; }
    public ICollection<SurveyQuestion> SurveyQuestions { get; set; }
}