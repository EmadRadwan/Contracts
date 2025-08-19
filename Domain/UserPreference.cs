namespace Domain;

public class UserPreference
{
    public string UserLoginId { get; set; } = null!;
    public string UserPrefTypeId { get; set; } = null!;
    public string? UserPrefGroupTypeId { get; set; }
    public string? UserPrefValue { get; set; }
    public string? UserPrefDataType { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserPrefGroupType? UserPrefGroupType { get; set; }
}