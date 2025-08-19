namespace Domain;

public class FtpAddress
{
    public string ContactMechId { get; set; } = null!;
    public string? Hostname { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? FtpPassword { get; set; }
    public string? BinaryTransfer { get; set; }
    public string? FilePath { get; set; }
    public string? ZipFile { get; set; }
    public string? PassiveMode { get; set; }
    public int? DefaultTimeout { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech ContactMech { get; set; } = null!;
}