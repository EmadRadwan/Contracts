namespace Domain;

public class NoteDatum
{
    public NoteDatum()
    {
        CustRequestItemNotes = new HashSet<CustRequestItemNote>();
        CustRequestNotes = new HashSet<CustRequestNote>();
        InvoiceNotes = new HashSet<InvoiceNote>();
        MarketingCampaignNotes = new HashSet<MarketingCampaignNote>();
        OrderHeaderNotes = new HashSet<OrderHeaderNote>();
        PartyNotes = new HashSet<PartyNote>();
        QuoteNotes = new HashSet<QuoteNote>();
        WorkEffortNotes = new HashSet<WorkEffortNote>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string NoteId { get; set; } = null!;
    public string? NoteName { get; set; }
    public string? NoteInfo { get; set; }
    public DateTime? NoteDateTime { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public string? NoteParty { get; set; }
    public string? MoreInfoUrl { get; set; }
    public string? MoreInfoItemId { get; set; }
    public string? MoreInfoItemName { get; set; }

    public Party? NotePartyNavigation { get; set; }
    public ICollection<CustRequestItemNote> CustRequestItemNotes { get; set; }
    public ICollection<CustRequestNote> CustRequestNotes { get; set; }
    public ICollection<InvoiceNote> InvoiceNotes { get; set; }
    public ICollection<MarketingCampaignNote> MarketingCampaignNotes { get; set; }
    public ICollection<OrderHeaderNote> OrderHeaderNotes { get; set; }
    public ICollection<PartyNote> PartyNotes { get; set; }
    public ICollection<QuoteNote> QuoteNotes { get; set; }
    public ICollection<WorkEffortNote> WorkEffortNotes { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}