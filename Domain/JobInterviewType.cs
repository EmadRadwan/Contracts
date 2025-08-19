namespace Domain;

public class JobInterviewType
{
    public JobInterviewType()
    {
        JobInterviews = new HashSet<JobInterview>();
    }

    public string JobInterviewTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<JobInterview> JobInterviews { get; set; }
}