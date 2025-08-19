namespace Domain;

public class ValidContactMechRole
{
    public string RoleTypeId { get; set; } = null!;
    public string ContactMechTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMechType ContactMechType { get; set; } = null!;
    public RoleType RoleType { get; set; } = null!;
}