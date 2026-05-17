using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class UpdateBroker
{
    public class Command : IRequest<Result<PartyDto>>
    {
        public PartyDto PartyDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<PartyDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<PartyDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var party = await _context.Parties.FindAsync(request.PartyDto.PartyId);

            if (party == null) return null;

            var stamp = DateTime.UtcNow;

            party.LastUpdatedStamp = stamp;
            party.Description = request.PartyDto.GroupName;

            var telcomNumberQuery = from prty in _context.Parties
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                select tn;

            var primaryTelcomNumber = telcomNumberQuery.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(request.PartyDto.MobileContactNumber))
            {
                if (primaryTelcomNumber != null)
                {
                    primaryTelcomNumber.ContactNumber = request.PartyDto.MobileContactNumber;
                }
                else
                {
                    var contactMech = new ContactMech
                    {
                        ContactMechId = Guid.NewGuid().ToString(),
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMechTypeId = "TELECOM_NUMBER"
                    };
                    _context.ContactMeches.Add(contactMech);

                    var telecomNumber = new TelecomNumber
                    {
                        ContactMech = contactMech,
                        ContactNumber = request.PartyDto.MobileContactNumber,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp
                    };
                    _context.TelecomNumbers.Add(telecomNumber);

                    var partyContactMech = new PartyContactMech
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        Party = party,
                        RoleTypeId = "BROKER"
                    };
                    _context.PartyContactMeches.Add(partyContactMech);

                    var partyContactMechPurpose = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "PRIMARY_PHONE",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurpose);
                }
            }

            var currentPostalAddressQuery = from prty in _context.Parties
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pa in _context.PostalAddresses on cm.ContactMechId equals pa.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals new
                    { pcmp.PartyId, pcmp.ContactMechId }
                where prty.PartyId == request.PartyDto.PartyId
                      && pcmp.ContactMechPurposeTypeId == "GENERAL_LOCATION"
                select pa;

            var generalLocation = currentPostalAddressQuery.FirstOrDefault();

            bool hasAddressData = !string.IsNullOrWhiteSpace(request.PartyDto.Address1) ||
                                  !string.IsNullOrWhiteSpace(request.PartyDto.Address2) ||
                                  !string.IsNullOrWhiteSpace(request.PartyDto.GeoId);

            if (hasAddressData)
            {
                if (generalLocation != null)
                {
                    generalLocation.Address1 = request.PartyDto.Address1 ?? generalLocation.Address1;
                    generalLocation.Address2 = request.PartyDto.Address2 ?? generalLocation.Address2;
                    generalLocation.ToName = request.PartyDto.GroupName ?? generalLocation.ToName;
                    generalLocation.CountryGeoId = request.PartyDto.GeoId ?? generalLocation.CountryGeoId;
                }
                else if (!string.IsNullOrWhiteSpace(request.PartyDto.Address1))
                {
                    var contactMech = new ContactMech
                    {
                        ContactMechId = Guid.NewGuid().ToString(),
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMechTypeId = "POSTAL_ADDRESS"
                    };
                    _context.ContactMeches.Add(contactMech);

                    var partyContactMech = new PartyContactMech
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        Party = party,
                        RoleTypeId = "BROKER"
                    };
                    _context.PartyContactMeches.Add(partyContactMech);

                    var partyContactMechPurposeGeneral = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "GENERAL_LOCATION",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurposeGeneral);

                    var postalAddress = new PostalAddress
                    {
                        ContactMech = contactMech,
                        ToName = request.PartyDto.GroupName,
                        Address1 = request.PartyDto.Address1,
                        Address2 = request.PartyDto.Address2,
                        CountryGeoId = request.PartyDto.GeoId
                    };
                    _context.PostalAddresses.Add(postalAddress);
                }
            }

            var currentContactMechQuery = from prty in _context.Parties
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals new
                    { pcmp.PartyId, pcmp.ContactMechId }
                where prty.PartyId == request.PartyDto.PartyId
                      && pcmp.ContactMechPurposeTypeId == "PRIMARY_EMAIL"
                select cm;

            var primaryEmail = currentContactMechQuery.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(request.PartyDto.InfoString))
            {
                if (primaryEmail != null)
                {
                    primaryEmail.InfoString = request.PartyDto.InfoString;
                }
                else
                {
                    var contactMech = new ContactMech
                    {
                        ContactMechId = Guid.NewGuid().ToString(),
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        InfoString = request.PartyDto.InfoString,
                        ContactMechTypeId = "EMAIL_ADDRESS"
                    };
                    _context.ContactMeches.Add(contactMech);

                    var partyContactMech = new PartyContactMech
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        Party = party,
                        RoleTypeId = "BROKER"
                    };
                    _context.PartyContactMeches.Add(partyContactMech);

                    var partyContactMechPurpose = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "PRIMARY_EMAIL",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurpose);
                }
            }

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                return Result<PartyDto>.Failure("Failed to update Broker");
            }

            var query1 = from prty in _context.Parties
                join st in _context.StatusItems on prty.StatusId equals st.StatusId
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join ptgr in _context.PartyGroups on prty.PartyId equals ptgr.PartyId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                select new PartyDto
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description + " ( " + prty.MainRole + " )",
                    PartyTypeId = pt.PartyTypeId,
                    PartyTypeDescription = pt.Description,
                    GroupName = ptgr.GroupName,
                    MobileContactNumber = tn.ContactNumber,
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
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "GENERAL_LOCATION"
                select new PartyDto
                {
                    PartyId = prty.PartyId,
                    Description = prty.Description + " ( " + prty.MainRole + " )",
                    PartyTypeId = pt.PartyTypeId,
                    PartyTypeDescription = pt.Description,
                    GroupName = ptgr.GroupName,
                    InfoString = cm.InfoString,
                    Address1 = pa.Address1,
                    Address2 = pa.Address2,
                    GeoId = geo.GeoId,
                    GeoName = geo.GeoName,
                    MainRole = prty.MainRole
                };

            var results1 = await query1.ToListAsync(cancellationToken);
            var results2 = await query2.ToListAsync(cancellationToken);

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
                partyToReturn.InfoString = results1[0].InfoString;
            }

            if (results2.Count > 0)
            {
                partyToReturn.Address1 = results2[0].Address1;
                partyToReturn.Address2 = results2[0].Address2;
                partyToReturn.GeoId = results2[0].GeoId;
                partyToReturn.GeoName = results2[0].GeoName;
                if (string.IsNullOrEmpty(partyToReturn.PartyId))
                {
                    partyToReturn.PartyId = results2[0].PartyId;
                    partyToReturn.Description = results2[0].Description;
                    partyToReturn.MainRole = results2[0].MainRole;
                }
            }

            return Result<PartyDto>.Success(partyToReturn);
        }
    }
}
