namespace Application.Facilities;

public class CreatePicklistFromOrdersResult
{
    public bool HasError { get; set; } // Indicates if an error occurred
    public string? ErrorMessage { get; set; } // Detailed error message if any
    public string? PicklistId { get; set; } // The ID of the newly created picklist (if successful)
}
