using Application.Core;
using Application.CRM.Leads.Assignment;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<IQueryable<LeadRecord>> Handle(Query request, CancellationToken ct)
        {
            // Visibility scoping: without CRM_Leads_ViewAll a user sees only the
            // leads currently assigned to them. Applied server-side - hiding a
            // column in the UI would not be a permission.
            var seesAllLeads = _userAccessor.IsInRole(LeadAssignmentConstants.ViewAllSecurityRole);

            string? currentPartyId = null;
            if (!seesAllLeads)
            {
                var username = _userAccessor.GetUsername();
                currentPartyId = await _context.Users
                    .Where(u => u.UserName == username)
                    .Select(u => u.PartyId)
                    .FirstOrDefaultAsync(ct);

                // Fail closed: a user we cannot resolve to a party sees nothing,
                // rather than falling through to every lead in the system.
                if (string.IsNullOrEmpty(currentPartyId))
                    return Enumerable.Empty<LeadRecord>().AsQueryable();
            }

            var query =
                _context.Parties
                .Where(p => p.PartyType!.PartyTypeId == "PERSON")
                .Where(p =>
                    p.PartyRoles.Any(pr =>
                        pr.RoleTypeId == "LEAD"))
                .Where(p => seesAllLeads || _context.PartyRelationships.Any(pr =>
                        pr.PartyIdTo == p.PartyId
                        && pr.PartyRelationshipTypeId == LeadAssignmentConstants.RelationshipTypeId
                        && pr.ThruDate == null
                        && pr.PartyIdFrom == currentPartyId))
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

                    // Broker behind an indirect lead - the open AGENT relationship
                    // from a BROKER. Projected here so the edit form can prefill it
                    // (it is required to save, so making the user re-pick it on
                    // every edit would be punishing).
                    BrokerPartyId = _context.PartyRelationships
                        .Where(pr => pr.PartyIdTo == p.PartyId
                                  && pr.PartyRelationshipTypeId == LeadBrokerConstants.RelationshipTypeId
                                  && pr.RoleTypeIdFrom == LeadBrokerConstants.BrokerRoleTypeId
                                  && pr.ThruDate == null)
                        .Select(pr => pr.PartyIdFrom)
                        .FirstOrDefault(),

                    BrokerName = (from pr in _context.PartyRelationships
                                  join b in _context.Parties on pr.PartyIdFrom equals b.PartyId
                                  where pr.PartyIdTo == p.PartyId
                                     && pr.PartyRelationshipTypeId == LeadBrokerConstants.RelationshipTypeId
                                     && pr.RoleTypeIdFrom == LeadBrokerConstants.BrokerRoleTypeId
                                     && pr.ThruDate == null
                                  select b.Description).FirstOrDefault(),

                    // Current owner - the open LEAD_OWNER relationship, if any.
                    // Kept inside the IQueryable so OData can sort and filter on
                    // owner server-side rather than per-page.
                    OwnerPartyId = _context.PartyRelationships
                        .Where(pr => pr.PartyIdTo == p.PartyId
                                  && pr.PartyRelationshipTypeId == LeadAssignmentConstants.RelationshipTypeId
                                  && pr.ThruDate == null)
                        .Select(pr => pr.PartyIdFrom)
                        .FirstOrDefault(),

                    OwnerName = (from pr in _context.PartyRelationships
                                 join owner in _context.Parties on pr.PartyIdFrom equals owner.PartyId
                                 where pr.PartyIdTo == p.PartyId
                                    && pr.PartyRelationshipTypeId == LeadAssignmentConstants.RelationshipTypeId
                                    && pr.ThruDate == null
                                 select owner.Description).FirstOrDefault(),

                    AssignedDate = _context.PartyRelationships
                        .Where(pr => pr.PartyIdTo == p.PartyId
                                  && pr.PartyRelationshipTypeId == LeadAssignmentConstants.RelationshipTypeId
                                  && pr.ThruDate == null)
                        .Select(pr => (DateTime?)pr.FromDate)
                        .FirstOrDefault(),

                    CreatedStamp = p.CreatedStamp
                })
                .OrderByDescending(x => x.CreatedStamp);

            return query;
        }
    }
}