using System.ComponentModel.DataAnnotations;

namespace Application.Parties.Parties;

public class PartyRecord
{
    [Key] public string? PartyId { get; set; }

    public FromPartyDto? FromPartyId { get; set; }
    public string? Description { get; set; }
    public string? GroupName { get; set; }
    public string? MainRole { get; set; }
    public string? PartyTypeId { get; set; }
    public string? PartyTypeDescription { get; set; }
    public string? CreatedByUserName { get; set; }
    public string? StatusDescription { get; set; }
    public string? PersonalTitleFirstName { get; set; }
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
    public DateTime? LastUpdatedStamp { get; set; }
}