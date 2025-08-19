namespace Domain;

public class WebSiteContactList
{
    public string WebSiteId { get; set; } = null!;
    public string ContactListId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactList ContactList { get; set; } = null!;
    public WebSite WebSite { get; set; } = null!;
}