using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Parties.Parties;

public class GetSupplier
{
    public class Query : IRequest<Result<PartyDto>>
    {
        public string PartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PartyDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<PartyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query1 = from prty in _context.Parties
                join st in _context.StatusItems on prty.StatusId equals st.StatusId
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join ptgr in _context.PartyGroups on prty.PartyId equals ptgr.PartyId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                select new PartyDto
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description + " ( " + prty.MainRole + " )",
                    PartyTypeId = pt.PartyTypeId,
                    PartyTypeDescription = pt.Description,
                    GroupName = ptgr.GroupName,
                    MobileContactNumber = tn.ContactNumber,
                    ContactType = cmpt.Description,
                    InfoString = cm.InfoString,
                    MainRole = prty.MainRole,
                    StatusDescription = st.Description
                };

            var query2 = from prty in _context.Parties
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join ptgr in _context.PartyGroups on prty.PartyId equals ptgr.PartyId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pa in _context.PostalAddresses on cm.ContactMechId equals pa.ContactMechId
                join geo in _context.Geos on pa.CountryGeoId equals geo.GeoId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyId && pcmp.ContactMechPurposeTypeId == "GENERAL_LOCATION"
                select new PartyDto
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description + " ( " + prty.MainRole + " )",
                    PartyTypeId = pt.PartyTypeId,
                    PartyTypeDescription = pt.Description,
                    GroupName = ptgr.GroupName,
                    ContactType = cmpt.Description,
                    InfoString = cm.InfoString,
                    Address1 = pa.Address1,
                    Address2 = pa.Address2,
                    GeoId = geo.GeoId,
                    GeoName = geo.GeoName,
                    MainRole = prty.MainRole
                };

            var query3 = from prty in _context.Parties
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join ptgr in _context.PartyGroups on prty.PartyId equals ptgr.PartyId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_EMAIL"
                select new PartyDto
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description,
                    PartyTypeId = pt.PartyTypeId,
                    PartyTypeDescription = pt.Description,
                    GroupName = ptgr.GroupName,
                    ContactType = cmpt.Description,
                    InfoString = cm.InfoString,
                    MainRole = prty.MainRole
                };

            var results1 = query1.ToList();
            var results2 = query2.ToList();
            var results3 = query3.ToList();

            var partyToReturn = new PartyDto();

            if (results1.Count > 0)
            {
                partyToReturn.PartyId = results1[0].PartyId;
                partyToReturn.Description = results1[0].Description;
                partyToReturn.PartyTypeId = results1[0].PartyTypeId;
                partyToReturn.PartyTypeDescription = results1[0].PartyTypeDescription;
                partyToReturn.GroupName = results1[0].GroupName;
                partyToReturn.MobileContactNumber = results1[0].MobileContactNumber;
                partyToReturn.MainRole = results1[0].MainRole;
                partyToReturn.StatusDescription = results1[0].StatusDescription;
            }

            if (results2.Count > 0)
            {
                partyToReturn.Address1 = results2[0].Address1;
                partyToReturn.Address2 = results2[0].Address2;
                partyToReturn.GeoId = results2[0].GeoId;
                partyToReturn.GeoName = results2[0].GeoName;
                partyToReturn.MainRole = results2[0].MainRole;
            }

            if (results3.Count > 0)
            {
                partyToReturn.InfoString = results3[0].InfoString;
                partyToReturn.MainRole = results3[0].MainRole;
            }


            return Result<PartyDto>.Success(partyToReturn);
        }
    }
}