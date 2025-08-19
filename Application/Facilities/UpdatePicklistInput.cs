namespace Application.Facilities;

public class UpdatePicklistInput
{
    public string PicklistId { get; set; } // required primary key
    public string StatusId { get; set; }   // optional
    // other non-PK fields from the picklist table
    // e.g. public string Description { get; set; }
    // ...
    
    public string UserLoginId { get; set; } // mirrors userLogin.userLoginId
}
