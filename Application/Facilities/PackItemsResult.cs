namespace Application.Facilities;

public class PackItemsResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<PackingSessionLineDto> PackedLines { get; set; }
}