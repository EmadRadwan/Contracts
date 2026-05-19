namespace Application.Parties.Parties;

public class PartyDto2
{
    public string? PartyId { get; set; }
    public FromPartyDto? FromPartyId { get; set; }
    public string? Description { get; set; }
    public string? GroupName { get; set; }
    public string? MainRole { get; set; }
    public string? DataSourceId { get; set; }
    public string? PartyTypeId { get; set; }
    public string? PartyTypeDescription { get; set; }
    public string? CreatedGlAccountId { get; set; }
    public string? CreatedGlAccountName { get; set; }
    public string? CreatedGlAccountArabicName { get; set; }
    public string? GlAccountType { get; set; }
    public string? ParentGlAccountId { get; set; }
    public string? CreatedByUserName { get; set; }
    public string? StatusDescription { get; set; }
    public string? PersonalTitleFirstName { get; set; }
    // Add to PartyDto (or create SupplierDto if you want separation)
    public string? LoanGlAccountId { get; set; }                // 1241XX - existing
    public string? LoanGlAccountName { get; set; }
    public string? LoanGlAccountNameArabic { get; set; }
    public string? AccruedGlAccountId { get; set; }             // 2200XX - existing
    public string? AccruedGlAccountName { get; set; }
    public string? AccruedGlAccountNameArabic { get; set; }

// For success feedback
    public string? CreatedLoanGlAccountId { get; set; }
    public string? CreatedLoanGlAccountName { get; set; }
    public string? CreatedLoanGlAccountArabicName { get; set; }
    public string? CreatedAccruedGlAccountId { get; set; }
    public string? CreatedAccruedGlAccountName { get; set; }
    public string? CreatedAccruedGlAccountArabicName { get; set; }

// For success feedback after create/update
    public string? CreatedApGlAccountId { get; set; }
    public string? CreatedApGlAccountName { get; set; }
    public string? CreatedApGlAccountArabicName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? PersonalTitle { get; set; }
    public string? MobileContactNumber { get; set; }
    public string? WorkCountryCode { get; set; }
    public string? WorkAreaCode { get; set; }
    public string? WorkContactNumber { get; set; }
    public string? FaxCountryCode { get; set; }
    public string? FaxAreaCode { get; set; }
    public string? FaxContactNumber { get; set; }
    public string? InfoString { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? GeoId { get; set; }
    public string? GeoName { get; set; }
    public string? ContactType { get; set; }
    public string? EmplPositionTypeId { get; set; }
    public decimal? MonthlyBaseSalary { get; set; } 
    public FromPartyDto? ReportingTo { get; set; }
    public string? ReportingToPartyId { get; set; }
    public string? GlAccountIdAdvancedPayment { get; set; }
    public string? PreferredPayrollPaymentMethodId { get; set; }
    public string? LeadTemperatureId { get; set; }
    public string? DepartmentPartyId { get; set; }
    public string? FingerPrintAttendanceId { get; set; }
    public TimeSpan? AttendanceStartsAt { get; set; }
    public decimal? VacationBalance { get; set; }
}