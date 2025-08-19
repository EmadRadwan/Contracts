namespace Domain;

public class JobRequisition
{
    public JobRequisition()
    {
        EmploymentApps = new HashSet<EmploymentApp>();
        JobInterviews = new HashSet<JobInterview>();
    }

    public string JobRequisitionId { get; set; } = null!;
    public int? DurationMonths { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public int? ExperienceMonths { get; set; }
    public int? ExperienceYears { get; set; }
    public string? Qualification { get; set; }
    public string? JobLocation { get; set; }
    public string? SkillTypeId { get; set; }
    public int? NoOfResources { get; set; }
    public string? JobPostingTypeEnumId { get; set; }
    public DateTime? JobRequisitionDate { get; set; }
    public string? ExamTypeEnumId { get; set; }
    public DateTime? RequiredOnDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration? ExamTypeEnum { get; set; }
    public Enumeration? JobPostingTypeEnum { get; set; }
    public SkillType? SkillType { get; set; }
    public ICollection<EmploymentApp> EmploymentApps { get; set; }
    public ICollection<JobInterview> JobInterviews { get; set; }
}