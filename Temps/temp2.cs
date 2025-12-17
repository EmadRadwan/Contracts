public class StatusItem
{
    // existing properties...

    // existing SalesRequests collection...

    // REFACTOR: Add collection for ReserveRequest entities using this status
    // Why: Completes bidirectional navigation for status-based queries
    public ICollection<ReserveRequest> ReserveRequests { get; set; } = new List<ReserveRequest>();
}