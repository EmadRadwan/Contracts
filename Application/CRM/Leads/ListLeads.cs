using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.Leads;

/// <summary>
/// Lists Leads (People) with filtering and search.
/// </summary>
public class ListLeads
{
    public record Query : IRequest<Result<List<LeadDto>>>
    {
        public string? SearchTerm { get; init; }
        public string? DataSourceId { get; init; }
        public string? SortBy { get; init; } = "name";
        public bool SortDescending { get; init; } = false;
    }

    public class Handler : IRequestHandler<Query, Result<List<LeadDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<LeadDto>>> Handle(Query request, CancellationToken ct)
        {
            // Get all parties that are PERSON type with CONTACT or LEAD role
            var query = _context.Parties
                .Include(p => p.Person)
                .Include(p => p.PartyType)
                .Include(p => p.Status)
                .Include(p => p.PartyContactMeches)
                    .ThenInclude(pcm => pcm.ContactMech)
                        .ThenInclude(cm => cm.TelecomNumber)
                .Include(p => p.PartyContactMeches)
                    .ThenInclude(pcm => pcm.ContactMech)
                        .ThenInclude(cm => cm.PostalAddress)
                .Include(p => p.PartyDataSources)
                .Include(p => p.PartyRoles)
                    .ThenInclude(pr => pr.RoleType)
                .Where(p => p.PartyType!.PartyTypeId == "PERSON")
                .Where(p => p.PartyRoles.Any(pr =>
                    pr.RoleTypeId == "CONTACT" || pr.RoleTypeId == "LEAD" || pr.RoleTypeId == "CUSTOMER"))
                .AsQueryable();

            // Search by name or email
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

            // Filter by data source
            if (!string.IsNullOrEmpty(request.DataSourceId))
            {
                query = query.Where(p => p.PartyDataSources.Any(pds => pds.DataSourceId == request.DataSourceId));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "email" => request.SortDescending
                    ? query.OrderByDescending(p => p.PartyContactMeches
                        .Where(pcm => pcm.ContactMech!.ContactMechType!.ContactMechTypeId == "EMAIL_ADDRESS")
                        .Select(pcm => pcm.ContactMech!.InfoString)
                        .FirstOrDefault())
                    : query.OrderBy(p => p.PartyContactMeches
                        .Where(pcm => pcm.ContactMech!.ContactMechType!.ContactMechTypeId == "EMAIL_ADDRESS")
                        .Select(pcm => pcm.ContactMech!.InfoString)
                        .FirstOrDefault()),
                "created" => request.SortDescending
                    ? query.OrderByDescending(p => p.CreatedStamp)
                    : query.OrderBy(p => p.CreatedStamp),
                _ => request.SortDescending
                    ? query.OrderByDescending(p => p.Person != null ? p.Person.FirstName : p.Description)
                    : query.OrderBy(p => p.Person != null ? p.Person.FirstName : p.Description)
            };

            var parties = await query.ToListAsync(ct);

            var result = parties.Select(p =>
            {
                var emailCm = p.PartyContactMeches
                    .FirstOrDefault(pcm => pcm.ContactMech?.ContactMechType?.ContactMechTypeId == "EMAIL_ADDRESS");
                var phoneCm = p.PartyContactMeches
                    .FirstOrDefault(pcm => pcm.ContactMech?.TelecomNumber != null);
                var addressCm = p.PartyContactMeches
                    .FirstOrDefault(pcm => pcm.ContactMech?.PostalAddress != null);
                var dataSource = p.PartyDataSources.FirstOrDefault();

                var firstName = p.Person?.FirstName ?? "";
                var lastName = p.Person?.LastName ?? "";

                return new LeadDto
                {
                    PartyId = p.PartyId,
                    FirstName = firstName,
                    LastName = lastName,
                    PersonalTitle = p.Person?.PersonalTitle,
                    FullName = $"{firstName} {lastName}".Trim(),
                    Email = emailCm?.ContactMech?.InfoString,
                    Phone = phoneCm?.ContactMech?.TelecomNumber?.ContactNumber,
                    MobilePhone = phoneCm?.ContactMech?.TelecomNumber?.ContactNumber,
                    Address1 = addressCm?.ContactMech?.PostalAddress?.Address1,
                    Address2 = addressCm?.ContactMech?.PostalAddress?.Address2,
                    City = addressCm?.ContactMech?.PostalAddress?.City,
                    PostalCode = addressCm?.ContactMech?.PostalAddress?.PostalCode,
                    CountryGeoId = addressCm?.ContactMech?.PostalAddress?.CountryGeoId,
                    DataSourceId = dataSource?.DataSourceId,
                    StatusId = p.Status?.StatusId,
                    StatusDescription = p.Status?.Description,
                    CreatedStamp = p.CreatedStamp
                };
            }).ToList();

            return Result<List<LeadDto>>.Success(result);
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
