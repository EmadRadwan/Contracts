namespace Domain;

public class ReserveRequest
{
    public ReserveRequest()
    {
        // No child collections required for now
    }

    // -----------------------------------------------------------------
    // Primary key
    // -----------------------------------------------------------------
    public string ReserveRequestId { get; set; } = null!;

    public string ProductId { get; set; } = null!;
    public string FromPartyId { get; set; } = null!;

    public DateTime? ReserveDate                     { get; set; }
    public decimal?  ReserveAmount                   { get; set; }
    public string?   Comments                     { get; set; }
    public string?   PayMethod                     { get; set; }
    public string?   ChequeStatus                     { get; set; }
    public string? StatusId { get; set; }
    public string? EmployeePartyId { get; set; }


    // -----------------------------------------------------------------
    // Audit stamps (same pattern as Product)
    // -----------------------------------------------------------------
    public DateTime? LastUpdatedStamp  { get; set; }
    public DateTime? CreatedStamp      { get; set; }

    // -----------------------------------------------------------------
    // Navigation properties
    // -----------------------------------------------------------------
    public virtual StatusItem? Status { get; set; }
    public virtual Product   Product   { get; set; } = null!;
    public virtual Party     Customer  { get; set; } = null!;
    public virtual Party? Employee { get; set; }      // navigation for the employee

}