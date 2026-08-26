using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Persistence.Auditing;

namespace Infrastructure.Security;

/// <summary>
/// Supplies actor and correlation context to the audit interceptor from the current request.
/// </summary>
public class HttpAuditMetadataProvider : IAuditMetadataProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly bool _isEnabled;

    public HttpAuditMetadataProvider(IHttpContextAccessor httpContextAccessor, IConfiguration config)
    {
        _httpContextAccessor = httpContextAccessor;

        // Defaults to on; "Auditing": { "Enabled": false } in appsettings stops capture without a
        // code change if volume ever becomes a problem. The JSON boolean arrives here as the
        // string "False", which bool.TryParse handles. Read via the indexer to match EmailSender
        // and avoid depending on the ConfigurationBinder extensions.
        _isEnabled = !bool.TryParse(config["Auditing:Enabled"], out var enabled) || enabled;
    }

    public bool IsEnabled => _isEnabled;

    /// <summary>
    /// Records both the display name and the AspNetUsers.Id, because usernames can be changed
    /// later and a historical audit row must still resolve to the right person.
    /// <para>
    /// Reads the claims directly rather than going through IUserAccessor: that dereferences
    /// HttpContext.User unguarded and would throw for any save originating outside a request
    /// (seeding, background work, design-time tooling).
    /// </para>
    /// </summary>
    public string? GetChangedByInfo()
    {
        var name = GetUserName();
        var id = GetUserId();

        if (string.IsNullOrWhiteSpace(id)) return name;
        if (string.IsNullOrWhiteSpace(name)) return id;

        return $"{name} ({id})";
    }

    public string? GetUserName()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true) return "system";

        var name = user.FindFirstValue(ClaimTypes.Name);
        return string.IsNullOrWhiteSpace(name) ? "system" : name;
    }

    public string? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true) return null;

        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrWhiteSpace(id) ? null : id;
    }

    public string? GetRequestPath()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null) return null;

        return request.QueryString.HasValue
            ? $"{request.Path}{request.QueryString}"
            : request.Path.ToString();
    }

    public string? GetHttpMethod() => _httpContextAccessor.HttpContext?.Request.Method;

    /// <summary>
    /// Prefers the leftmost X-Forwarded-For entry, since production runs behind a reverse proxy
    /// (Kestrel on 5100/8544 via Docker) where RemoteIpAddress would otherwise be the proxy.
    /// </summary>
    public string? GetClientIpAddress()
    {
        var http = _httpContextAccessor.HttpContext;
        if (http is null) return null;

        var forwarded = http.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            var first = forwarded.Split(',')[0].Trim();
            if (!string.IsNullOrWhiteSpace(first)) return first;
        }

        return http.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// HttpContext.TraceIdentifier — the same value ExceptionMiddleware returns as the error ID
    /// and Serilog logs as {RequestId}, so an error report ties directly to the rows it changed.
    /// </summary>
    public string? GetCorrelationId() => _httpContextAccessor.HttpContext?.TraceIdentifier;
}
