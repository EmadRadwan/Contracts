namespace Domain;

public class PartyNameHistory
{
    public string PartyId { get; set; } = null!;
    public DateTime ChangeDate { get; set; }
    public string? GroupName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? PersonalTitle { get; set; }
    public string? Suffix { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
}