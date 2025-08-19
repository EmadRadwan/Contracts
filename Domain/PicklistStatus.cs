namespace Domain;

public class PicklistStatus
{
    public string PicklistId { get; set; } = null!;
    public DateTime StatusDate { get; set; }
    public string? ChangeByUserLoginId { get; set; }
    public string? StatusId { get; set; }
    public string? StatusIdTo { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangeByUserLogin { get; set; }
    public Picklist Picklist { get; set; } = null!;
    public StatusItem? Status { get; set; }
    public StatusItem? StatusIdToNavigation { get; set; }
}