using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Parties.Parties;

public class UpdateSupplier
{
    public class Command : IRequest<Result<PartyDto>>
    {
        public PartyDto PartyDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.PartyDto).SetValidator(new PartyValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<PartyDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<PartyDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            var party = await _context.Parties.FindAsync(request.PartyDto.PartyId);

            if (party == null) return null;

            var stamp = DateTime.Now;

            party.LastUpdatedStamp = stamp;
            party.Description = request.PartyDto.GroupName;


            var partyGroup = await _context.PartyGroups.FindAsync(request.PartyDto.PartyId);

            if (partyGroup == null) return null;

            partyGroup.GroupName = request.PartyDto.GroupName;
            partyGroup.LastUpdatedStamp = stamp;

            var telcomNumber = from prty in _context.Parties
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                select tn;


            var primaryTelcomNumber = telcomNumber.SingleOrDefault();

            primaryTelcomNumber.ContactNumber = request.PartyDto.MobileContactNumber;


            var currentPostalAddress = from prty in _context.Parties
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pa in _context.PostalAddresses on cm.ContactMechId equals pa.ContactMechId
                join geo in _context.Geos on pa.CountryGeoId equals geo.GeoId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyDto.PartyId &&
                      pcmp.ContactMechPurposeTypeId == "GENERAL_LOCATION"
                select pa;

            var generalLocation = currentPostalAddress.SingleOrDefault();
            if (generalLocation != null)
            {
                generalLocation.Address1 = request.PartyDto.Address1;
                generalLocation.Address2 = request.PartyDto.Address2;
                generalLocation.ToName = request.PartyDto.FirstName;
                generalLocation.CountryGeoId = request.PartyDto.GeoId;
            }
            else
            {
                if (!string.IsNullOrEmpty(request.PartyDto.Address1))
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
                        RoleTypeId = "SUPPLIER"
                    };
                    _context.PartyContactMeches.Add(partyContactMech);

                    var partyContactMechPurposeGeneralLocation = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "GENERAL_LOCATION",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurposeGeneralLocation);

                    var partyContactMechPurposeShippingLocation = new PartyContactMechPurpose
                    {
                        FromDate = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp,
                        ContactMech = contactMech,
                        ContactMechPurposeTypeId = "SHIPPING_LOCATION",
                        Party = party
                    };
                    _context.PartyContactMechPurposes.Add(partyContactMechPurposeShippingLocation);

                    var postalAddress = new PostalAddress
                    {
                        ContactMech = contactMech,
                        ToName = request.PartyDto.FirstName,
                        Address1 = request.PartyDto.Address1,
                        Address2 = request.PartyDto.Address2,
                        CountryGeoId = request.PartyDto.GeoId
                    };
                    _context.PostalAddresses.Add(postalAddress);
                }
            }

            var currentContactMech = from prty in _context.Parties
                join pt in _context.PartyTypes on prty.PartyTypeId equals pt.PartyTypeId
                join prs in _context.Persons on prty.PartyId equals prs.PartyId
                join pcm in _context.PartyContactMeches on prty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_EMAIL"
                select cm;

            var primaryEmail = currentContactMech.SingleOrDefault();
            if (primaryEmail != null)
            {
                primaryEmail.InfoString = request.PartyDto.InfoString;
            }
            else
            {
                if (!string.IsNullOrEmpty(request.PartyDto.Address1))
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
                        RoleTypeId = "SUPPLIER"
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

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                transaction.Rollback();
                return Result<PartyDto>.Failure("Failed to update Supplier");
            }

            transaction.Commit();


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
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
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
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "GENERAL_LOCATION"
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
                where prty.PartyId == request.PartyDto.PartyId && pcmp.ContactMechPurposeTypeId == "PRIMARY_EMAIL"
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