using Application.Core;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.CRM.Leads;

public class ListLeads
{
    public class Query : IRequest<IQueryable<LeadRecord>>
    {
        public ODataQueryOptions<LeadRecord> Options { get; set; } = null!;
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
            var query =
                _context.Parties
                .Where(p => p.PartyType!.PartyTypeId == "PERSON")
                .Where(p =>
                    p.PartyRoles.Any(pr =>
                        pr.RoleTypeId == "LEAD"))
                .Select(p => new LeadRecord
                {
                    PartyId = p.PartyId,

                    FirstName = p.Person != null ? p.Person.FirstName : null,
                    MiddleName = p.Person != null ? p.Person.MiddleName : null,
                    LastName = p.Person != null ? p.Person.LastName : null,

                    FullName =
                        (p.Person!.FirstName ?? "") + " " +
                        (p.Person!.MiddleName ?? "") + " " +
                        (p.Person!.LastName ?? ""),

                    DataSourceId = p.DataSourceId,

                    Email = p.PartyContactMeches
                        .Where(pcm =>
                            pcm.ContactMech!.ContactMechType!.ContactMechTypeId == "EMAIL_ADDRESS")
                        .OrderByDescending(pcm => pcm.FromDate)
                        .Select(pcm => pcm.ContactMech!.InfoString)
                        .FirstOrDefault(),

                    MobilePhone = p.PartyContactMeches
                        .Where(pcm =>
                            pcm.ContactMech!.TelecomNumber != null &&
                            pcm.ContactMech.PartyContactMechPurposes
                                .Any(x => x.ContactMechPurposeTypeId == "PHONE_MOBILE"))
                        .OrderByDescending(pcm => pcm.FromDate)
                        .Select(pcm => pcm.ContactMech!.TelecomNumber!.ContactNumber)
                        .FirstOrDefault(),

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

                    CountryGeoId = p.PartyContactMeches
                        .Where(pcm => pcm.ContactMech!.PostalAddress != null)
                        .OrderByDescending(pcm => pcm.FromDate)
                        .Select(pcm => pcm.ContactMech!.PostalAddress!.CountryGeoId)
                        .FirstOrDefault(),

                    StatusId = p.StatusId,
                    StatusDescription = p.Status!.Description,

                    LeadTemperatureId = p.LeadTemperatureId,

                    CreatedStamp = p.CreatedStamp
                })
                .OrderByDescending(x => x.CreatedStamp);

            return query;
        }
    }
}