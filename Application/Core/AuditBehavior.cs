using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Domain;
using MediatR;
using Persistence.Auditing;

namespace Application.Core;

/// <summary>
/// Records one AUDIT_ACTIVITY row per command execution: who ran what, with which input, how long
/// it took, and whether it worked.
///
/// <para>
/// Sits in the same pipeline as LoggingBehavior, which already proved the interception point
/// works. The difference is where the answer lands: LoggingBehavior writes Serilog text, which is
/// pinned at Warning, rolls off after 7 days and lives on the VM's disk. This writes queryable
/// rows you can filter, join to the business tables and pull into Power BI.
/// </para>
///
/// <para>
/// Every row carries the correlation ID that ENTITY_AUDIT_LOG rows carry as CHANGED_SESSION_INFO,
/// so "the user pressed this" and "these fields changed" line up on one key.
/// </para>
/// </summary>
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int MaxRequestJsonLength = 8000;   // TEXT holds ~65k; this stays readable
    private const int MaxErrorMessageLength = 1024;
    private const int MaxNameLength = 255;
    private const int MaxPathLength = 512;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        MaxDepth = 16
    };

    // Static fields on a generic type are per closed type, so both of these are computed once per
    // request type rather than on every dispatch.
    private static readonly bool IsAudited = ComputeIsAudited();
    private static readonly string RequestName = ComputeRequestName();

    private readonly IAuditActivityWriter _writer;
    private readonly IAuditMetadataProvider _metadata;

    public AuditBehavior(IAuditActivityWriter writer, IAuditMetadataProvider metadata)
    {
        _writer = writer;
        _metadata = metadata;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!IsAudited || !_metadata.IsEnabled) return await next();

        var activity = new AuditActivity
        {
            ActivityId = Guid.NewGuid().ToString(),
            CorrelationId = Truncate(_metadata.GetCorrelationId(), 64),
            UserName = Truncate(_metadata.GetUserName(), MaxNameLength),
            UserId = Truncate(_metadata.GetUserId(), 36),
            RequestName = Truncate(RequestName, MaxNameLength),
            RequestPath = Truncate(_metadata.GetRequestPath(), MaxPathLength),
            HttpMethod = Truncate(_metadata.GetHttpMethod(), 10),
            ClientIpAddress = Truncate(_metadata.GetClientIpAddress(), 64),
            RequestJson = SerializeRedacted(request),
            StartedAt = DateTime.UtcNow,
            CreatedStamp = DateTime.UtcNow,
            LastUpdatedStamp = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            // Handlers return failed Results rather than throwing (see CLAUDE.md), so a failure
            // looks like an ordinary return. Unwrap it or the trail would call every call a success.
            if (response is IResult { IsSuccess: false } failed)
            {
                activity.IsSuccess = false;
                activity.ErrorMessage = Truncate(failed.ErrorMessage, MaxErrorMessageLength);
            }
            else
            {
                activity.IsSuccess = true;
            }

            return response;
        }
        catch (Exception ex)
        {
            activity.IsSuccess = false;
            activity.ErrorMessage = Truncate(ex.Message, MaxErrorMessageLength);
            activity.ExceptionType = Truncate(ex.GetType().Name, MaxNameLength);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            activity.DurationMs = (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue);

            // Runs on the success path, the failed-Result path and the thrown-exception path.
            // The writer owns its own DataContext, so this row survives a rolled-back command.
            await _writer.WriteAsync(activity, cancellationToken);
        }
    }

    /// <summary>
    /// Commands only. Queries are roughly two thirds of the ~610 request types and change nothing,
    /// so auditing them would multiply volume for almost no diagnostic value. The convention is a
    /// nested "Command" class; a handful of features use a PascalCase suffix instead, and both are
    /// matched here. Anything else can opt in with IAuditableRequest.
    /// </summary>
    private static bool ComputeIsAudited()
    {
        var type = typeof(TRequest);

        if (typeof(IAuditableRequest).IsAssignableFrom(type)) return true;

        return type.Name.Equals("Command", StringComparison.Ordinal)
               || type.Name.EndsWith("Command", StringComparison.Ordinal);
    }

    /// <summary>
    /// Produces "ResetProjectCertificate.Command" rather than a bare "Command", using the same
    /// DeclaringType trick LoggingBehavior uses for its messages.
    /// </summary>
    private static string ComputeRequestName() =>
        typeof(TRequest).DeclaringType is { } feature
            ? $"{feature.Name}.{typeof(TRequest).Name}"
            : typeof(TRequest).Name;

    /// <summary>
    /// Serializes the command payload with credential-ish fields replaced. Registration and
    /// password-change commands travel through this pipeline, so this is not optional.
    /// </summary>
    private static string? SerializeRedacted(TRequest request)
    {
        try
        {
            var node = JsonSerializer.SerializeToNode(request, SerializerOptions);
            Redact(node);

            var json = node?.ToJsonString();
            if (json is null) return null;

            return json.Length <= MaxRequestJsonLength
                ? json
                : json[..MaxRequestJsonLength] + "…[truncated]";
        }
        catch (Exception ex)
        {
            // Commands carrying streams (file uploads) or graphs deeper than MaxDepth land here.
            // Record why rather than dropping the whole row.
            return $"[payload not serializable: {ex.GetType().Name}]";
        }
    }

    private static void Redact(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var property in obj.ToList())
                {
                    if (AuditedEntities.IsSensitiveField(property.Key))
                        obj[property.Key] = "***REDACTED***";
                    else
                        Redact(property.Value);
                }
                break;

            case JsonArray array:
                foreach (var item in array.ToList())
                    Redact(item);
                break;
        }
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
