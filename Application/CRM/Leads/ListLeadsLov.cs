using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.Leads
{
    public class ListLeadsLov
    {
        public class Query : IRequest<Result<List<LeadDto>>>
        {
            public string? SearchTerm { get; set; }
            public int Take { get; set; } = 20;
        }

        public class Handler : IRequestHandler<Query, Result<List<LeadDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<LeadDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var search = request.SearchTerm?.Trim().ToLower();

                var query = _context.Parties
                    .Where(p => p.PartyType!.PartyTypeId == "PERSON")
                    .Where(p => p.PartyRoles.Any(pr =>
                        pr.RoleTypeId == "CONTACT" ||
                        pr.RoleTypeId == "LEAD" ||
                        pr.RoleTypeId == "CUSTOMER"))
                    .Select(p => new LeadDto
                    {
                        PartyId = p.PartyId,
                        FirstName = p.Person!.FirstName,
                        LastName = p.Person!.LastName,
                        FullName = (p.Person!.FirstName ?? "") + " " + (p.Person!.MiddleName ?? "") + " " + (p.Person!.LastName ?? ""),
                        Email = p.PartyContactMeches
                            .Where(pcm => pcm.ContactMech!.ContactMechType!.ContactMechTypeId == "EMAIL_ADDRESS")
                            .OrderByDescending(pcm => pcm.FromDate)
                            .Select(pcm => pcm.ContactMech!.InfoString)
                            .FirstOrDefault(),
                        Phone = p.PartyContactMeches
                            .Where(pcm => pcm.ContactMech!.TelecomNumber != null &&
                                pcm.ContactMech.PartyContactMechPurposes.Any(x => x.ContactMechPurposeTypeId == "PHONE"))
                            .OrderByDescending(pcm => pcm.FromDate)
                            .Select(pcm => pcm.ContactMech!.TelecomNumber!.ContactNumber)
                            .FirstOrDefault(),
                        MobilePhone = p.PartyContactMeches
                            .Where(pcm => pcm.ContactMech!.TelecomNumber != null &&
                                pcm.ContactMech.PartyContactMechPurposes.Any(x => x.ContactMechPurposeTypeId == "PHONE_MOBILE"))
                            .OrderByDescending(pcm => pcm.FromDate)
                            .Select(pcm => pcm.ContactMech!.TelecomNumber!.ContactNumber)
                            .FirstOrDefault()
                    });

                // Apply search if provided
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(l =>
                        (l.FirstName != null && l.FirstName.ToLower().Contains(search)) ||
                        (l.LastName != null && l.LastName.ToLower().Contains(search)) ||
                        (l.Email != null && l.Email.ToLower().Contains(search)) ||
                        (l.Phone != null && l.Phone.ToLower().Contains(search)) ||
                        (l.MobilePhone != null && l.MobilePhone.ToLower().Contains(search))
                    );
                }

                // Take limited number of results
                var result = await query
                    .OrderBy(l => l.FirstName)
                    .Take(request.Take)
                    .ToListAsync(cancellationToken);

                return Result<List<LeadDto>>.Success(result);
            }
        }
    }
}