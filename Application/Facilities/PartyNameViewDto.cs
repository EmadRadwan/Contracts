namespace Application.Facilities;

public class PartyNameViewDto
{
    public string PartyId { get; set; }
    public string PartyTypeId { get; set; }
    public string StatusId { get; set; }
    public string Description { get; set; }

    // Person fields
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string FirstNameLocal { get; set; }
    public string LastNameLocal { get; set; }
    public string PersonalTitle { get; set; }
    public string Suffix { get; set; }

    // PartyGroup fields
    public string GroupName { get; set; }
    public string GroupNameLocal { get; set; }
}
