using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class CreateLead
{
    public record Command : IRequest<Result<PartyDto2>>
    {
        public PartyDto2 PartyDto { get; init; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<PartyDto2>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly IUtilityService _utilityService;

        public Handler(
            DataContext context,
            IUserAccessor userAccessor,
            IUtilityService utilityService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _utilityService = utilityService;
        }

        public async Task<Result<PartyDto2>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var dto = request.PartyDto;
                var stamp = DateTime.UtcNow;

                // Lookups
                var partyTypePerson = await _context.PartyTypes
                    .SingleAsync(x => x.PartyTypeId == "PERSON", ct);

                var statusEnabled = await _context.StatusItems
                    .SingleAsync(x => x.StatusId == "PARTY_ENABLED", ct);

                var roleTypeLead = await _context.RoleTypes
                    .SingleAsync(x => x.RoleTypeId == "LEAD", ct);

                var contactMechTypes = await _context.ContactMechTypes
                    .Where(x => new[] { "TELECOM_NUMBER", "EMAIL_ADDRESS", "POSTAL_ADDRESS" }
                    .Contains(x.ContactMechTypeId))
                    .ToDictionaryAsync(x => x.ContactMechTypeId, ct);

                var purposeTypes = await _context.ContactMechPurposeTypes
                    .Where(x => new[]
                    {
                        "PRIMARY_PHONE",
                        "PRIMARY_EMAIL",
                        "GENERAL_LOCATION",
                        "SHIPPING_LOCATION",
                        "PHONE_MOBILE"
                    }.Contains(x.ContactMechPurposeTypeId))
                    .ToDictionaryAsync(x => x.ContactMechPurposeTypeId, ct);

                // ---------------------
                // DUPLICATE CHECK
                // ---------------------

                Party? existingParty = null;

                if (!string.IsNullOrWhiteSpace(dto.InfoString))
                {
                    existingParty = await _context.ContactMeches
                        .Where(cm =>
                            cm.InfoString == dto.InfoString &&
                            cm.ContactMechType.ContactMechTypeId == "EMAIL_ADDRESS")
                        .SelectMany(cm => cm.PartyContactMeches)
                        .Select(pcm => pcm.Party)
                        .Where(p => p.PartyRoles.Any(r => r.RoleType.RoleTypeId == "LEAD"))
                        .FirstOrDefaultAsync(ct);
                }

                if (existingParty == null && !string.IsNullOrWhiteSpace(dto.MobileContactNumber))
                {
                    existingParty = await _context.TelecomNumbers
                        .Where(t => t.ContactNumber == dto.MobileContactNumber)
                        .Select(t => t.ContactMech)
                        .SelectMany(cm => cm.PartyContactMeches)
                        .Select(pcm => pcm.Party)
                        .Where(p => p.PartyRoles.Any(r => r.RoleType.RoleTypeId == "LEAD"))
                        .FirstOrDefaultAsync(ct);
                }

                if (existingParty != null)
                {
                    await transaction.RollbackAsync(ct);

                    var existingPerson = await _context.Persons
                        .FirstOrDefaultAsync(p => p.Party == existingParty, ct);

                    return Result<PartyDto2>.Success(new PartyDto2
                    {
                        PartyId = existingParty.PartyId,
                        FirstName = existingPerson?.FirstName,
                        MiddleName = existingPerson?.MiddleName,
                        Description = existingParty.Description,
                        PartyTypeDescription = "Lead",
                        IsAlreadyCreated = true
                    });
                }

                var partyId = (await _utilityService.GetNextSequence("Party")).ToString();

                var fullName = $"{dto.FirstName ?? ""} {dto.MiddleName ?? ""}".Trim();

                var party = new Party
                {
                    PartyId = partyId,
                    PartyType = partyTypePerson,
                    Status = statusEnabled,
                    Description = fullName,
                    MainRole = "LEAD_CONTACT",
                    LeadTemperatureId = dto.LeadTemperatureId ?? "C",
                    DataSourceId = string.IsNullOrWhiteSpace(dto.DataSourceId) ? null : dto.DataSourceId,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };

                _context.Parties.Add(party);

                // Person
                _context.Persons.Add(new Person
                {
                    Party = party,
                    FirstName = dto.FirstName,
                    MiddleName = dto.MiddleName,
                    PersonalTitle = dto.PersonalTitle,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });

                // Required OFBiz table
                _context.PartyGroups.Add(new PartyGroup
                {
                    Party = party
                });

                // Role
                _context.PartyRoles.Add(new PartyRole
                {
                    Party = party,
                    RoleType = roleTypeLead,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });

                // Status history
                _context.PartyStatuses.Add(new PartyStatus
                {
                    Party = party,
                    Status = statusEnabled,
                    StatusDate = stamp,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });

                // ---------------------
                // MOBILE PHONE
                // ---------------------

                if (!string.IsNullOrWhiteSpace(dto.MobileContactNumber))
                {
                    var cmId = (await _utilityService.GetNextSequence("ContactMech")).ToString();

                    var cm = new ContactMech
                    {
                        ContactMechId = cmId,
                        ContactMechType = contactMechTypes["TELECOM_NUMBER"],
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };

                    _context.ContactMeches.Add(cm);

                    _context.TelecomNumbers.Add(new TelecomNumber
                    {
                        ContactMech = cm,
                        ContactNumber = dto.MobileContactNumber,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });

                    _context.PartyContactMeches.Add(new PartyContactMech
                    {
                        Party = party,
                        ContactMech = cm,
                        FromDate = stamp,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });

                    _context.PartyContactMechPurposes.Add(new PartyContactMechPurpose
                    {
                        Party = party,
                        ContactMech = cm,
                        ContactMechPurposeType = purposeTypes["PHONE_MOBILE"],
                        FromDate = stamp,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });
                }

                // ---------------------
                // EMAIL
                // ---------------------

                if (!string.IsNullOrWhiteSpace(dto.InfoString))
                {
                    var cmId = (await _utilityService.GetNextSequence("ContactMech")).ToString();

                    var cm = new ContactMech
                    {
                        ContactMechId = cmId,
                        InfoString = dto.InfoString,
                        ContactMechType = contactMechTypes["EMAIL_ADDRESS"],
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };

                    _context.ContactMeches.Add(cm);

                    _context.PartyContactMeches.Add(new PartyContactMech
                    {
                        Party = party,
                        ContactMech = cm,
                        FromDate = stamp,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });

                    _context.PartyContactMechPurposes.Add(new PartyContactMechPurpose
                    {
                        Party = party,
                        ContactMech = cm,
                        ContactMechPurposeType = purposeTypes["PRIMARY_EMAIL"],
                        FromDate = stamp,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });
                }

                // ---------------------
                // POSTAL ADDRESS
                // ---------------------

                if (!string.IsNullOrWhiteSpace(dto.Address1))
                {
                    var cmId = (await _utilityService.GetNextSequence("ContactMech")).ToString();

                    var cm = new ContactMech
                    {
                        ContactMechId = cmId,
                        ContactMechType = contactMechTypes["POSTAL_ADDRESS"],
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };

                    _context.ContactMeches.Add(cm);

                    _context.PostalAddresses.Add(new PostalAddress
                    {
                        ContactMech = cm,
                        ToName = fullName,
                        Address1 = dto.Address1,
                        Address2 = dto.Address2,
                        City = dto.City,
                        CountryGeoId = dto.GeoId,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });

                    _context.PartyContactMeches.Add(new PartyContactMech
                    {
                        Party = party,
                        ContactMech = cm,
                        FromDate = stamp,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });

                    _context.PartyContactMechPurposes.AddRange(new[]
                    {
                        new PartyContactMechPurpose
                        {
                            Party = party,
                            ContactMech = cm,
                            ContactMechPurposeType = purposeTypes["GENERAL_LOCATION"],
                            FromDate = stamp,
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp
                        },
                        new PartyContactMechPurpose
                        {
                            Party = party,
                            ContactMech = cm,
                            ContactMechPurposeType = purposeTypes["SHIPPING_LOCATION"],
                            FromDate = stamp,
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp
                        }
                    });
                }

                // Data Source
                _context.PartyDataSources.Add(new PartyDataSource
                {
                    Party = party,
                    DataSourceId = dto.DataSourceId ?? "COLD_CALL",
                    FromDate = stamp,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return Result<PartyDto2>.Success(new PartyDto2
                {
                    PartyId = party.PartyId,
                    FirstName = dto.FirstName,
                    MiddleName = dto.MiddleName,
                    Description = fullName,
                    PartyTypeDescription = "Lead"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<PartyDto2>.Failure($"Failed to create lead: {ex.Message}");
            }
        }
    }
}