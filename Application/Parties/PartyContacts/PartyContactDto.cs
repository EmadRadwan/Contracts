namespace Application.Parties.PartyContacts;

public class PartyContactDto
{
    public string PartyId { get; set; }
    public string ContactMechPurposeTypeId { get; set; }
    public string ContactMechPurposeType { get; set; }
    public string ContactMechId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string CountryCode { get; set; }
    public string AreaCode { get; set; }
    public string ContactNumber { get; set; }
    public string InfoString { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string GeoId { get; set; }
    public string GeoName { get; set; }
}