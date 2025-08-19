using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _env = env;
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred processing your request: {ErrorId}", context.TraceIdentifier);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = DetermineStatusCode(ex);

            var response = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = DetermineErrorTitle(ex),
                Detail = _env.IsDevelopment() ? ex.StackTrace : "Refer to the error ID in your logs.",
                Instance = context.TraceIdentifier
            };

            var json = JsonSerializer.Serialize(response, _jsonOptions);

            await context.Response.WriteAsync(json);
        }
    }

    private int DetermineStatusCode(Exception ex)
    {
        if (ex is ApplicationException) return StatusCodes.Status400BadRequest;
        if (ex is NotFoundException) return StatusCodes.Status404NotFound;
        return StatusCodes.Status500InternalServerError;
    }

    private string DetermineErrorTitle(Exception ex)
    {
        if (ex is ApplicationException) return "Application Error";
        if (ex is NotFoundException) return "Not Found";
        return "An unexpected error occurred";
    }
}
