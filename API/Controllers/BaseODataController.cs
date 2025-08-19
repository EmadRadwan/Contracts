using System.Text.Json;
using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace API.Controllers.OData;

[ApiController]
[Route("api/odata/[controller]")]
public abstract class BaseODataController<T> : ODataController
{
    private IMediator _mediator;

    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices
        .GetService<IMediator>();

    protected string GetLanguage()
    {
        // Retrieve the 'Accept-Language' header
        return Request.Headers["Accept-Language"].ToString();
    }

    protected async Task<IActionResult> HandleODataQueryAsync(IQueryable<T> query, ODataQueryOptions<T> options)
    {
        try
        {
            var queryString = options.Request.QueryString;

            // Remove the $top and $skip parameters from the query string
            var queryParameters = QueryHelpers.ParseQuery(queryString.Value);
            RemoveParameter(queryParameters, "$top");
            RemoveParameter(queryParameters, "$skip");

            var modifiedQueryString = QueryString.Create(queryParameters);

            // Create a new HttpRequest with the modified query string
            var request = new DefaultHttpContext().Request;
            request.QueryString = new QueryString(modifiedQueryString.Value);

            var modifiedOptions = new ODataQueryOptions<T>(options.Context, request);

            // Apply the modified query options to the base query without paging
            var queryWithFilterOnly = modifiedOptions.ApplyTo(query) as IQueryable<T>;

            // Calculate the total count respecting the filter
            var totalCount = await queryWithFilterOnly.CountAsync();

            var headerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var metaData = new MetaData
            {
                TotalCount = totalCount
            };
            Response.Headers.Add("Count", JsonSerializer.Serialize(metaData, headerOptions));
            Response.Headers.Add("Access-Control-Expose-Headers", "Count");

            // You can use the language here as needed, for example:
            var language = GetLanguage();
            // Optionally log or process the language value as required.

            // Return the query result
            return Ok(queryWithFilterOnly);
        }
        catch (Exception ex)
        {
            // Log the exception if needed
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    private void RemoveParameter(Dictionary<string, StringValues> parameters, string parameterName)
    {
        if (parameters.ContainsKey(parameterName))
        {
            parameters.Remove(parameterName);
        }
        else
        {
            // Check for the parameter with &$ prefix
            var prefix = $"&{parameterName}=";
            var parameterWithPrefix = parameters.FirstOrDefault(p => p.Key.Contains(prefix));
            if (!string.IsNullOrEmpty(parameterWithPrefix.Key)) parameters.Remove(parameterWithPrefix.Key);
        }
    }
}
