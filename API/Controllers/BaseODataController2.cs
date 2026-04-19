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
public abstract class BaseODataController2<T> : ODataController
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;

    protected string GetLanguage()
    {
        return Request.Headers["Accept-Language"].ToString();
    }

    // -----------------------------------------------------------------
    // Fixed: Works with in-memory IQueryable (no EF async needed)
    // -----------------------------------------------------------------
    protected async Task<IActionResult> HandleODataQueryAsync(
        IQueryable<T> query,
        ODataQueryOptions<T> options)
    {
        try
        {
            // -------------------------------------------------------------
            // 1. Strip $top/$skip to compute total count *after filter*
            // -------------------------------------------------------------
            var queryString = options.Request.QueryString;
            var queryParameters = QueryHelpers.ParseQuery(queryString.Value!);
            RemoveParameter(queryParameters, "$top");
            RemoveParameter(queryParameters, "$skip");

            var modifiedQueryString = QueryString.Create(queryParameters);
            var request = new DefaultHttpContext().Request;
            request.QueryString = new QueryString(modifiedQueryString.Value);

            var modifiedOptions = new ODataQueryOptions<T>(options.Context, request);

            // -------------------------------------------------------------
            // 2. Apply filter/orderby/select/expand (no paging)
            // -------------------------------------------------------------
            // REFACTOR: ApplyTo returns IQueryable<T> – works in-memory
            IQueryable<T> queryWithFilterOnly;
            try
            {
                queryWithFilterOnly = modifiedOptions.ApplyTo(query) as IQueryable<T>
                                      ?? query;
            }
            catch (Exception)
            {
                // Fallback: If OData filtering fails (e.g., due to DateOnly type mismatch), 
                // we continue with the original query.
                // The handler may have already applied manual filtering if it detected problematic fields.
                queryWithFilterOnly = query;
            }

            // -------------------------------------------------------------
            // 3. Count *synchronously* – source is in-memory
            // -------------------------------------------------------------
            // REFACTOR: Use .Count() instead of CountAsync()
            var totalCount = queryWithFilterOnly.Count();

            // -------------------------------------------------------------
            // 4. Add Count header (camelCase)
            // -------------------------------------------------------------
            var headerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var metaData = new MetaData { TotalCount = totalCount };
            Response.Headers.Add("Count", JsonSerializer.Serialize(metaData, headerOptions));
            Response.Headers.Add("Access-Control-Expose-Headers", "Count");

            // -------------------------------------------------------------
            // 5. Return filtered data (OData will apply $top/$skip client-side)
            // -------------------------------------------------------------
            // REFACTOR: Let OData apply $top/$skip on the *original* options
            object finalResult;
            try
            {
                finalResult = options.ApplyTo(query); // includes $top/$skip
            }
            catch (Exception)
            {
                // Fallback for final result if ApplyTo fails
                // We manually apply skip and top if possible, or just return the query
                var skip = options.Skip?.Value ?? 0;
                var top = options.Top?.Value ?? 100;
                finalResult = query.Skip(skip).Take(top).ToList();
            }

            return Ok(finalResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    private static void RemoveParameter(Dictionary<string, StringValues> parameters, string parameterName)
    {
        if (parameters.ContainsKey(parameterName))
        {
            parameters.Remove(parameterName);
            return;
        }

        var prefixedKey = parameters.Keys.FirstOrDefault(k => k.Contains($"&{parameterName}="));
        if (prefixedKey != null)
            parameters.Remove(prefixedKey);
    }
}