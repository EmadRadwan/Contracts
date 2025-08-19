namespace Domain;

public class StandardLanguage
{
    public string StandardLanguageId { get; set; } = null!;
    public string? LangCode3t { get; set; }
    public string? LangCode3b { get; set; }
    public string? LangCode2 { get; set; }
    public string? LangName { get; set; }
    public string? LangFamily { get; set; }
    public string? LangCharset { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}