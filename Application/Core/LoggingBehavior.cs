using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Core;

// Handlers in this codebase catch their own exceptions and return Result<T>/Results<T> instead of
// throwing (see CLAUDE.md), which means a failure never reaches the global ExceptionMiddleware/
// Serilog request logging unless the handler explicitly logs it before returning. That blind spot
// let ResetProjectCertificate silently orphan certificate 233-0026's invoices (INV1362/INV1363)
// with zero trace in logs/. This behavior closes the gap for every handler, not just that one:
// it logs any failed IResult and any exception that still escapes a handler.
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;

    public LoggingBehavior(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).DeclaringType is { } feature
            ? $"{feature.Name}.{typeof(TRequest).Name}"
            : typeof(TRequest).Name;

        TResponse response;
        try
        {
            response = await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{RequestName} threw an unhandled exception", requestName);
            throw;
        }

        if (response is IResult { IsSuccess: false } result)
        {
            _logger.LogWarning("{RequestName} returned a failed result: {Error}", requestName, result.ErrorMessage);
        }

        return response;
    }
}
