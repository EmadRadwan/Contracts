using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.Contacts;

/// <summary>
/// Creates a Contact (Person) in the CRM.
/// This is the proper "People" entity - stable identity, not tied to revenue.
/// </summary>
public class CreateContact
{
    public record Command : IRequest<Result<ContactDto>>
    {
        public ContactDto Contact { get; init; } = null!;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Contact.FirstName)
                .NotEmpty().WithMessage("First name is required");

            RuleFor(x => x.Contact.LastName)
                .NotEmpty().WithMessage("Last name is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<ContactDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly IUtilityService _utilityService;

        public Handler(DataContext context, IUserAccessor userAccessor, IUtilityService utilityService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _utilityService = utilityService;
        }

        public async Task<Result<ContactDto>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var stamp = DateTime.UtcNow;
                var dto = request.Contact;

                // Load lookup data
                var partyTypePerson = await _context.PartyTypes.SingleAsync(x => x.PartyTypeId == "PERSON", ct);
                var statusEnabled = await _context.StatusItems.SingleAsync(x => x.StatusId == "PARTY_ENABLED", ct);
                var roleTypeContact = await _context.RoleTypes.FirstOrDefaultAsync(x => x.RoleTypeId == "CONTACT", ct)
                    ?? await _context.RoleTypes.FirstAsync(x => x.RoleTypeId == "LEAD", ct);

                var contactMechTypes = await _context.ContactMechTypes
                    .Where(cmt => new[] { "TELECOM_NUMBER", "EMAIL_ADDRESS", "POSTAL_ADDRESS" }.Contains(cmt.ContactMechTypeId))
                    .ToDictionaryAsync(x => x.ContactMechTypeId, ct);

                var purposeTypes = await _context.ContactMechPurposeTypes
                    .Where(cmp => new[] { "PRIMARY_PHONE", "PRIMARY_EMAIL", "GENERAL_LOCATION" }.Contains(cmp.ContactMechPurposeTypeId))
                    .ToDictionaryAsync(x => x.ContactMechPurposeTypeId, ct);

                // Generate ID
                var partyId = await _utilityService.GetNextSequence("Party");
                var fullName = $"{dto.FirstName} {dto.LastName}".Trim();

                // Create Party
                var party = new Party
                {
                    PartyId = partyId.ToString(),
                    PartyType = partyTypePerson,
                    Status = statusEnabled,
                    Description = fullName,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.Parties.Add(party);

                // Create Person
                _context.Persons.Add(new Person
                {
                    Party = party,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName ?? "",
                    PersonalTitle = dto.PersonalTitle,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });

                // Create PartyGroup (required by OFBiz schema)
                _context.PartyGroups.Add(new PartyGroup { Party = party });

                // Assign CONTACT role
                _context.PartyRoles.Add(new PartyRole
                {
                    Party = party,
                    RoleType = roleTypeContact,
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

                // Phone
                if (!string.IsNullOrWhiteSpace(dto.Phone) || !string.IsNullOrWhiteSpace(dto.MobilePhone))
                {
                    var phoneNumber = dto.MobilePhone ?? dto.Phone;
                    var cmId = await _utilityService.GetNextSequence("ContactMech");
                    var cm = new ContactMech
                    {
                        ContactMechId = cmId.ToString(),
                        ContactMechType = contactMechTypes["TELECOM_NUMBER"],
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };
                    _context.ContactMeches.Add(cm);

                    _context.TelecomNumbers.Add(new TelecomNumber
                    {
                        ContactMech = cm,
                        ContactNumber = phoneNumber,
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

                    if (purposeTypes.ContainsKey("PRIMARY_PHONE"))
                    {
                        _context.PartyContactMechPurposes.Add(new PartyContactMechPurpose
                        {
                            Party = party,
                            ContactMech = cm,
                            ContactMechPurposeType = purposeTypes["PRIMARY_PHONE"],
                            FromDate = stamp,
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp
                        });
                    }
                }

                // Email
                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var cmId = await _utilityService.GetNextSequence("ContactMech");
                    var cm = new ContactMech
                    {
                        ContactMechId = cmId.ToString(),
                        InfoString = dto.Email,
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

                    if (purposeTypes.ContainsKey("PRIMARY_EMAIL"))
                    {
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
                }

                // Address
                if (!string.IsNullOrWhiteSpace(dto.Address1))
                {
                    var cmId = await _utilityService.GetNextSequence("ContactMech");
                    var cm = new ContactMech
                    {
                        ContactMechId = cmId.ToString(),
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
                        PostalCode = dto.PostalCode,
                        CountryGeoId = dto.CountryGeoId,
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

                    if (purposeTypes.ContainsKey("GENERAL_LOCATION"))
                    {
                        _context.PartyContactMechPurposes.Add(new PartyContactMechPurpose
                        {
                            Party = party,
                            ContactMech = cm,
                            ContactMechPurposeType = purposeTypes["GENERAL_LOCATION"],
                            FromDate = stamp,
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp
                        });
                    }
                }

                // Data Source
                if (!string.IsNullOrWhiteSpace(dto.DataSourceId))
                {
                    _context.PartyDataSources.Add(new PartyDataSource
                    {
                        Party = party,
                        DataSourceId = dto.DataSourceId,
                        FromDate = stamp,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });
                }

                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<ContactDto>.Failure("Failed to create contact");
                }

                await transaction.CommitAsync(ct);

                // Return created contact
                var result = new ContactDto
                {
                    PartyId = party.PartyId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PersonalTitle = dto.PersonalTitle,
                    FullName = fullName,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    MobilePhone = dto.MobilePhone,
                    Address1 = dto.Address1,
                    Address2 = dto.Address2,
                    City = dto.City,
                    PostalCode = dto.PostalCode,
                    CountryGeoId = dto.CountryGeoId,
                    DataSourceId = dto.DataSourceId,
                    StatusId = "PARTY_ENABLED",
                    StatusDescription = "Enabled",
                    CreatedStamp = stamp
                };

                return Result<ContactDto>.Success(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<ContactDto>.Failure($"Error creating contact: {ex.Message}");
            }
        }
    }
}
