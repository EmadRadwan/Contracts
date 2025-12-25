using Application.CRM.Contacts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.CRM;

/// <summary>
/// API Controller for CRM Contacts (People).
///
/// KEY CONCEPT:
/// A Contact is a PERSON - someone you know.
/// Contacts are linked to Sales Opportunities via SalesOpportunityRole.
/// </summary>
public class ContactsController : BaseApiController
{
    /// <summary>
    /// Get all contacts with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetContacts(
        [FromQuery] string? search,
        [FromQuery] string? dataSourceId,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDesc = false)
    {
        return HandleResult(await Mediator.Send(new ListContacts.Query
        {
            SearchTerm = search,
            DataSourceId = dataSourceId,
            SortBy = sortBy,
            SortDescending = sortDesc
        }));
    }

    /// <summary>
    /// Get contacts for LOV/picker (lightweight).
    /// </summary>
    [HttpGet("lov")]
    public async Task<IActionResult> GetContactsLov(
        [FromQuery] string? search,
        [FromQuery] int take = 20)
    {
        return HandleResult(await Mediator.Send(new ListContactsLov.Query
        {
            SearchTerm = search,
            Take = take
        }));
    }

    /// <summary>
    /// Create a new contact.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateContact([FromBody] ContactDto contact)
    {
        return HandleResult(await Mediator.Send(new CreateContact.Command
        {
            Contact = contact
        }));
    }

    /// <summary>
    /// Update an existing contact.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContact(string id, [FromBody] ContactDto contact)
    {
        contact.PartyId = id;
        return HandleResult(await Mediator.Send(new UpdateContact.Command
        {
            Contact = contact
        }));
    }
}
