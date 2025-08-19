namespace Domain;

public class ShoppingListItemSurvey
{
    public string ShoppingListId { get; set; } = null!;
    public string ShoppingListItemSeqId { get; set; } = null!;
    public string SurveyResponseId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ShoppingList ShoppingList { get; set; } = null!;
    public ShoppingListItem ShoppingListI { get; set; } = null!;
    public SurveyResponse SurveyResponse { get; set; } = null!;
}