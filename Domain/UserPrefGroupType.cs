namespace Domain;

public class UserPrefGroupType
{
    public UserPrefGroupType()
    {
        UserPreferences = new HashSet<UserPreference>();
    }

    public string UserPrefGroupTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<UserPreference> UserPreferences { get; set; }
}