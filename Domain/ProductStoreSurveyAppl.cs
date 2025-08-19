namespace Domain;

public class ProductStoreSurveyAppl
{
    public ProductStoreSurveyAppl()
    {
        WorkEffortSurveyAppls = new HashSet<WorkEffortSurveyAppl>();
    }

    public string ProductStoreSurveyId { get; set; } = null!;
    public string? ProductStoreId { get; set; }
    public string? SurveyApplTypeId { get; set; }
    public string? GroupName { get; set; }
    public string? SurveyId { get; set; }
    public string? ProductId { get; set; }
    public string? ProductCategoryId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? SurveyTemplate { get; set; }
    public string? ResultTemplate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductStore? ProductStore { get; set; }
    public Survey? Survey { get; set; }
    public SurveyApplType? SurveyApplType { get; set; }
    public ICollection<WorkEffortSurveyAppl> WorkEffortSurveyAppls { get; set; }
}