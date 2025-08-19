using Application.Content;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IContentAccessor
{
    Task<ContentActionResult> AddContent(IFormFile file);
    Task<ContentActionResult> DeleteContent(string publicId);
}