namespace Application.Facilities;

public class StatusValidChangeToDetailDto
{
    public string StatusId { get; set; }
    public string StatusIdTo { get; set; }
    public string SequenceId { get; set; }

    // Fields from the "to" StatusItem
    public string ToStatusId { get; set; }
    public string StatusCode { get; set; }
}
