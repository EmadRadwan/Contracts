namespace Application.Content;

public class ContentActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ContentUploadResult Result { get; set; }
}