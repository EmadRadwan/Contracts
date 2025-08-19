using Application.Core;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Parties.PartyContacts;

public class ListPartyContacts
{
    public class Query : IRequest<Result<List<PartyContactDto>>>
    {
        public string PartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<PartyContactDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;


        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<PartyContactDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query1 = (from pcm in _context.PartyContactMeches
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where pcm.PartyId == request.PartyId && pcmp.ContactMechPurposeTypeId != "PRIMARY_PHONE" &&
                      cm.ContactMechTypeId == "TELECOM_NUMBER"
                select new PartyContactDto
                {
                    PartyId = pcm.PartyId,
                    ContactMechPurposeType = cmpt.Description,
                    ContactMechPurposeTypeId = cmpt.ContactMechPurposeTypeId,
                    ContactMechId = pcm.ContactMechId,
                    FromDate = DateTime.SpecifyKind(pcm.FromDate.Truncate(TimeSpan.FromSeconds(1)),
                        DateTimeKind.Utc),
                    ThruDate = pcm.ThruDate,
                    AreaCode = tn.AreaCode,
                    ContactNumber = tn.ContactNumber,
                    InfoString = ""
                }).ToList();

            var query2 = (from pcm in _context.PartyContactMeches
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where pcm.PartyId == request.PartyId && pcmp.ContactMechPurposeTypeId != "PRIMARY_EMAIL" &&
                      cm.ContactMechTypeId == "EMAIL_ADDRESS"
                select new PartyContactDto
                {
                    PartyId = pcm.PartyId,
                    ContactMechPurposeType = cmpt.Description,
                    ContactMechPurposeTypeId = cmpt.ContactMechPurposeTypeId,
                    ContactMechId = pcm.ContactMechId,
                    FromDate = DateTime.SpecifyKind(pcm.FromDate.Truncate(TimeSpan.FromSeconds(1)), DateTimeKind.Utc),
                    ThruDate = pcm.ThruDate,
                    AreaCode = "",
                    ContactNumber = "",
                    InfoString = cm.InfoString
                }).ToList();

            var query3 = (from pcm in _context.PartyContactMeches
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pa in _context.PostalAddresses on cm.ContactMechId equals pa.ContactMechId
                join geo in _context.Geos on pa.CountryGeoId equals geo.GeoId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where pcm.PartyId == request.PartyId && pcmp.ContactMechPurposeTypeId != "GENERAL_LOCATION" &&
                      cm.ContactMechTypeId == "POSTAL_ADDRESS"
                select new PartyContactDto
                {
                    PartyId = pcm.PartyId,
                    ContactMechPurposeType = cmpt.Description,
                    ContactMechPurposeTypeId = cmpt.ContactMechPurposeTypeId,
                    ContactMechId = pcm.ContactMechId,
                    FromDate = DateTime.SpecifyKind(pcm.FromDate.Truncate(TimeSpan.FromSeconds(1)), DateTimeKind.Utc),
                    ThruDate = pcm.ThruDate,
                    AreaCode = "",
                    ContactNumber = "",
                    InfoString = cm.InfoString,
                    Address1 = pa.Address1,
                    Address2 = pa.Address2,
                    City = pa.CityGeo.GeoName,
                    GeoId = pa.CountryGeo.GeoName
                }).ToList();


            var query = (from pcm in _context.PartyContactMeches
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where pcm.PartyId == request.PartyId && pcmp.ContactMechPurposeTypeId != "PRIMARY_PHONE" &&
                      cm.ContactMechTypeId == "TELECOM_NUMBER"
                select new PartyContactDto
                {
                    PartyId = pcm.PartyId,
                    ContactMechPurposeType = cmpt.Description,
                    ContactMechPurposeTypeId = cmpt.ContactMechPurposeTypeId,
                    ContactMechId = pcm.ContactMechId,
                    FromDate = DateTime.SpecifyKind(pcm.FromDate.Truncate(TimeSpan.FromSeconds(1)),
                        DateTimeKind.Utc),
                    ThruDate = pcm.ThruDate,
                    AreaCode = tn.AreaCode,
                    ContactNumber = tn.ContactNumber,
                    InfoString = "",
                    Address1 = "",
                    Address2 = "",
                    City = "",
                    GeoId = ""
                }).AsEnumerable().Union(from pcm in _context.PartyContactMeches
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where pcm.PartyId == request.PartyId && pcmp.ContactMechPurposeTypeId != "PRIMARY_EMAIL" &&
                      cm.ContactMechTypeId == "EMAIL_ADDRESS"
                select new PartyContactDto
                {
                    PartyId = pcm.PartyId,
                    ContactMechPurposeType = cmpt.Description,
                    ContactMechPurposeTypeId = cmpt.ContactMechPurposeTypeId,
                    ContactMechId = pcm.ContactMechId,
                    FromDate = DateTime.SpecifyKind(pcm.FromDate.Truncate(TimeSpan.FromSeconds(1)), DateTimeKind.Utc),
                    ThruDate = pcm.ThruDate,
                    AreaCode = "",
                    ContactNumber = "",
                    InfoString = cm.InfoString,
                    Address1 = "",
                    Address2 = "",
                    City = "",
                    GeoId = ""
                }).AsEnumerable().Union(from pcm in _context.PartyContactMeches
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pa in _context.PostalAddresses on cm.ContactMechId equals pa.ContactMechId
                join geo in _context.Geos on pa.CountryGeoId equals geo.GeoId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where pcm.PartyId == request.PartyId && pcmp.ContactMechPurposeTypeId != "GENERAL_LOCATION" &&
                      cm.ContactMechTypeId == "POSTAL_ADDRESS"
                select new PartyContactDto
                {
                    PartyId = pcm.PartyId,
                    ContactMechPurposeType = cmpt.Description,
                    ContactMechPurposeTypeId = cmpt.ContactMechPurposeTypeId,
                    ContactMechId = pcm.ContactMechId,
                    FromDate = DateTime.SpecifyKind(pcm.FromDate.Truncate(TimeSpan.FromSeconds(1)), DateTimeKind.Utc),
                    ThruDate = pcm.ThruDate,
                    AreaCode = "",
                    ContactNumber = "",
                    InfoString = "",
                    Address1 = pa.Address1,
                    Address2 = pa.Address2,
                    City = pa.CityGeo.GeoName,
                    GeoId = pa.CountryGeo.GeoName
                });


            //var queryString = query.ToQueryString();

            var result = query.ToList();
            return Result<List<PartyContactDto>>.Success(result);
        }
    }
}