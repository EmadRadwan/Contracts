namespace Domain;

public class SurveyResponse
{
    public SurveyResponse()
    {
        DataResources = new HashSet<DataResource>();
        GiftCardFulfillments = new HashSet<GiftCardFulfillment>();
        ShoppingListItemSurveys = new HashSet<ShoppingListItemSurvey>();
        SurveyResponseAnswers = new HashSet<SurveyResponseAnswer>();
    }

    public string SurveyResponseId { get; set; } = null!;
    public string? SurveyId { get; set; }
    public string? PartyId { get; set; }
    public DateTime? ResponseDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? ReferenceId { get; set; }
    public string? GeneralFeedback { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? StatusId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public StatusItem? Status { get; set; }
    public Survey? Survey { get; set; }
    public ICollection<DataResource> DataResources { get; set; }
    public ICollection<GiftCardFulfillment> GiftCardFulfillments { get; set; }
    public ICollection<ShoppingListItemSurvey> ShoppingListItemSurveys { get; set; }
    public ICollection<SurveyResponseAnswer> SurveyResponseAnswers { get; set; }
}