namespace Domain;

public class PartyContactMech
{
    public string PartyId { get; set; } = null!;
    public string ContactMechId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? RoleTypeId { get; set; }
    public string? AllowSolicitation { get; set; }
    public string? Extension { get; set; }
    public string? Verified { get; set; }
    public string? Comments { get; set; }
    public int? YearsWithContactMech { get; set; }
    public int? MonthsWithContactMech { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech ContactMech { get; set; } = null!;
    public Party Party { get; set; } = null!;
    public PartyRole? PartyRole { get; set; }
    public RoleType? RoleType { get; set; }
}