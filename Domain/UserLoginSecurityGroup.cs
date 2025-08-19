namespace Domain;

public class UserLoginSecurityGroup
{
    public string UserLoginId { get; set; } = null!;
    public string GroupId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SecurityGroup Group { get; set; } = null!;
    public UserLogin UserLogin { get; set; } = null!;
}