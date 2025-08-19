namespace Domain;

public class PicklistStatusHistory
{
    public string PicklistId { get; set; } = null!;
    public DateTime ChangeDate { get; set; }
    public string? ChangeUserLoginId { get; set; }
    public string? StatusId { get; set; }
    public string? StatusIdTo { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangeUserLogin { get; set; }
    public Picklist Picklist { get; set; } = null!;
    public StatusItem? Status { get; set; }
    public StatusItem? StatusIdToNavigation { get; set; }
    public StatusValidChange? StatusNavigation { get; set; }
}