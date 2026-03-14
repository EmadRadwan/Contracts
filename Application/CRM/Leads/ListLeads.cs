using Application.Core;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.Leads;

/// <summary>
/// Lists Leads (People) with filtering and search.
/// </summary>
public class ListLeads
{
    public class Query : IRequest<IQueryable<LeadRecord>>
    {
        public ODataQueryOptions<LeadRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<LeadRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<LeadRecord>> Handle(Query request, CancellationToken ct)
        {
            // Get all parties that are PERSON type with CONTACT or LEAD role
            var query = _context.Parties
                .Where(p => p.PartyType!.PartyTypeId == "PERSON")
                .Where(p => p.PartyRoles.Any(pr =>
                    pr.RoleTypeId == "CONTACT" || pr.RoleTypeId == "LEAD" || pr.RoleTypeId == "CUSTOMER"))
                .Select(p => new LeadRecord
                {
                    PartyId = p.PartyId,

                    // Identity
                    FirstName = p.Person != null ? p.Person.FirstName : null,
                    LastName = p.Person != null ? p.Person.LastName : null,
                    PersonalTitle = p.Person != null ? p.Person.PersonalTitle : null,
                    FullName = p.Person != null
                        ? (p.Person.FirstName + " " + p.Person.LastName).Trim()
                        : p.Description,

                    // Communication - Primary Email
                    Email = p.PartyContactMeches
                        .Where(pcm => pcm.ContactMech!.ContactMechType!.ContactMechTypeId == "EMAIL_ADDRESS")
                        .OrderByDescending(pcm => pcm.FromDate)
                        .Select(pcm => pcm.ContactMech!.InfoString)
                        .FirstOrDefault(),

                    // Communication - Primary Phone
                    Phone = p.PartyContactMeches
                        .Where(pcm => pcm.ContactMech!.TelecomNumber != null)
                        .OrderByDescending(pcm => pcm.FromDate)
                        .Select(pcm => pcm.ContactMech!.TelecomNumber!.ContactNumber)
                        .FirstOrDefault(),

                    MobilePhone = p.PartyContactMeches
                        .Where(pcm => pcm.ContactMech!.TelecomNumber != null)
                        .OrderByDescending(pcm => pcm.FromDate)
                        .Select(pcm => pcm.ContactMech!.TelecomNumber!.ContactNumber)
                        .FirstOrDefault(),

                    // Address
                    Address1 = p.PartyContactMeches
                        .Where(pcm => pcm.ContactMech!.PostalAddress != null)
                        .OrderByDescending(pcm => pcm.FromDate)
                        .Select(pcm => pcm.ContactMech!.PostalAddress!.Address1)
                        .FirstOrDefault(),
                    Address2 = p.PartyContactMeches
                        .Where(pcm => pcm.ContactMech!.PostalAddress != null)
                        .OrderByDescending(pcm => pcm.FromDate)
                        .Select(pcm => pcm.ContactMech!.PostalAddress!.Address2)
                        .FirstOrDefault(),
                    City = p.PartyContactMeches
                        .Where(pcm => pcm.ContactMech!.PostalAddress != null)
                        .OrderByDescending(pcm => pcm.FromDate)
                        .Select(pcm => pcm.ContactMech!.PostalAddress!.City)
                        .FirstOrDefault(),
                    PostalCode = p.PartyContactMeches
                        .Where(pcm => pcm.ContactMech!.PostalAddress != null)
                        .OrderByDescending(pcm => pcm.FromDate)
                        .Select(pcm => pcm.ContactMech!.PostalAddress!.PostalCode)
                        .FirstOrDefault(),
                    CountryGeoId = p.PartyContactMeches
                        .Where(pcm => pcm.ContactMech!.PostalAddress != null)
                        .OrderByDescending(pcm => pcm.FromDate)
                        .Select(pcm => pcm.ContactMech!.PostalAddress!.CountryGeoId)
                        .FirstOrDefault(),

                    // CRM metadata
                    DataSourceId = p.PartyDataSources.Select(pds => pds.DataSourceId).FirstOrDefault(),

                    // Status
                    StatusId = p.StatusId,
                    StatusDescription = p.Status != null ? p.Status.Description : null,

                    // Audit
                    CreatedStamp = p.CreatedStamp,
                });

            return query;
        }
    }
}

/// <summary>
/// Lists Leads for LOV/Picker dropdowns (lightweight).
/// </summary>
public class ListLeadsLov
{
    public record Query : IRequest<Result<List<LeadLovDto>>>
    {
        public string? SearchTerm { get; init; }
        public int Take { get; init; } = 20;
    }

    public class Handler : IRequestHandler<Query, Result<List<LeadLovDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<LeadLovDto>>> Handle(Query request, CancellationToken ct)
        {
            var query = _context.Parties
                .Include(p => p.Person)
                .Include(p => p.PartyContactMeches)
                    .ThenInclude(pcm => pcm.ContactMech)
                        .ThenInclude(cm => cm.TelecomNumber)
                .Where(p => p.PartyType!.PartyTypeId == "PERSON")
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    (p.Person != null && (
                        (p.Person.FirstName != null && p.Person.FirstName.ToLower().Contains(term)) ||
                        (p.Person.LastName != null && p.Person.LastName.ToLower().Contains(term))
                    )) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)) ||
                    p.PartyContactMeches.Any(pcm =>
                        pcm.ContactMech != null &&
                        pcm.ContactMech.InfoString != null &&
                        pcm.ContactMech.InfoString.ToLower().Contains(term))
                );
            }

            var parties = await query
                .OrderBy(p => p.Person != null ? p.Person.FirstName : p.Description)
                .Take(request.Take)
                .ToListAsync(ct);

            var result = parties.Select(p =>
            {
                var emailCm = p.PartyContactMeches
                    .FirstOrDefault(pcm => pcm.ContactMech?.InfoString != null);
                var phoneCm = p.PartyContactMeches
                    .FirstOrDefault(pcm => pcm.ContactMech?.TelecomNumber != null);

                var firstName = p.Person?.FirstName ?? "";
                var lastName = p.Person?.LastName ?? "";

                return new LeadLovDto
                {
                    PartyId = p.PartyId,
                    FullName = $"{firstName} {lastName}".Trim(),
                    Email = emailCm?.ContactMech?.InfoString,
                    Phone = phoneCm?.ContactMech?.TelecomNumber?.ContactNumber
                };
            }).ToList();

            return Result<List<LeadLovDto>>.Success(result);
        }
    }
}
