namespace Domain;

public class Person
{
    public Person()
    {
        PersonTrainings = new HashSet<PersonTraining>();
    }

    public string PartyId { get; set; } = null!;
    public string? Salutation { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? PersonalTitle { get; set; }
    public string? Suffix { get; set; }
    public string? Nickname { get; set; }
    public string? FirstNameLocal { get; set; }
    public string? MiddleNameLocal { get; set; }
    public string? LastNameLocal { get; set; }
    public string? OtherLocal { get; set; }
    public string? MemberId { get; set; }
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? DeceasedDate { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public string? MothersMaidenName { get; set; }
    public string? MaritalStatus { get; set; }
    public string? MaritalStatusEnumId { get; set; }
    public string? SocialSecurityNumber { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpireDate { get; set; }
    public double? TotalYearsWorkExperience { get; set; }
    public string? Comments { get; set; }
    public string? EmploymentStatusEnumId { get; set; }
    public string? ResidenceStatusEnumId { get; set; }
    public string? Occupation { get; set; }
    public int? YearsWithEmployer { get; set; }
    public int? MonthsWithEmployer { get; set; }
    public string? ExistingCustomer { get; set; }
    public string? CardId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration? EmploymentStatusEnum { get; set; }
    public Enumeration? MaritalStatusEnum { get; set; }
    public Party Party { get; set; } = null!;
    public Enumeration? ResidenceStatusEnum { get; set; }
    public ICollection<PersonTraining> PersonTrainings { get; set; }
}