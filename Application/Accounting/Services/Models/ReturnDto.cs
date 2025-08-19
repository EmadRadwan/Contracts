namespace Application.Accounting.Services.Models;

public class ReturnDto
{
    public string ReturnId { get; set; }
    public string StatusDescription { get; set; }
    public string FromPartyId { get; set; }
    public string ToPartyId { get; set; }
}