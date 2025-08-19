using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class MimeType
{
    public MimeType()
    {
        CommunicationEvents = new HashSet<CommunicationEvent>();
        FileExtensions = new HashSet<FileExtension>();
    }

    public string MimeTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public MimeTypeHtmlTemplate MimeTypeHtmlTemplate { get; set; } = null!;
    public ICollection<CommunicationEvent> CommunicationEvents { get; set; }
    public ICollection<FileExtension> FileExtensions { get; set; }
}