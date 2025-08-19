namespace Domain;

public class Survey
{
    public Survey()
    {
        DataResources = new HashSet<DataResource>();
        ProductStoreFinActSettings = new HashSet<ProductStoreFinActSetting>();
        ProductStoreSurveyAppls = new HashSet<ProductStoreSurveyAppl>();
        SurveyMultiResps = new HashSet<SurveyMultiResp>();
        SurveyPages = new HashSet<SurveyPage>();
        SurveyQuestionAppls = new HashSet<SurveyQuestionAppl>();
        SurveyResponses = new HashSet<SurveyResponse>();
        SurveyTriggers = new HashSet<SurveyTrigger>();
        WorkEffortSurveyAppls = new HashSet<WorkEffortSurveyAppl>();
    }

    public string SurveyId { get; set; } = null!;
    public string? SurveyName { get; set; }
    public string? Description { get; set; }
    public string? Comments { get; set; }
    public string? SubmitCaption { get; set; }
    public string? ResponseService { get; set; }
    public string? IsAnonymous { get; set; }
    public string? AllowMultiple { get; set; }
    public string? AllowUpdate { get; set; }
    public string? AcroFormContentId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<DataResource> DataResources { get; set; }
    public ICollection<ProductStoreFinActSetting> ProductStoreFinActSettings { get; set; }
    public ICollection<ProductStoreSurveyAppl> ProductStoreSurveyAppls { get; set; }
    public ICollection<SurveyMultiResp> SurveyMultiResps { get; set; }
    public ICollection<SurveyPage> SurveyPages { get; set; }
    public ICollection<SurveyQuestionAppl> SurveyQuestionAppls { get; set; }
    public ICollection<SurveyResponse> SurveyResponses { get; set; }
    public ICollection<SurveyTrigger> SurveyTriggers { get; set; }
    public ICollection<WorkEffortSurveyAppl> WorkEffortSurveyAppls { get; set; }
}