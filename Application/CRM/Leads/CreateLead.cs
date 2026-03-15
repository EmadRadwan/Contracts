using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.Leads;

/// <summary>
/// Creates a Lead (Person) in the CRM.
/// This is the proper "People" entity - stable identity, not tied to revenue.
/// </summary>
public class CreateLead
{
    public record Command : IRequest<Result<LeadDto>>
    {
        public LeadDto Lead { get; init; } = null!;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Lead.FullName)
                .NotEmpty().WithMessage("Full name is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<LeadDto>>
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

        public async Task<Result<LeadDto>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var stamp = DateTime.UtcNow;
                var dto = request.Lead;

                // Load lookup data
                var partyTypePerson = await _context.PartyTypes.SingleAsync(x => x.PartyTypeId == "PERSON", ct);
                var statusEnabled = await _context.StatusItems.SingleAsync(x => x.StatusId == "PARTY_ENABLED", ct);
                var roleTypeLead = await _context.RoleTypes.FirstOrDefaultAsync(x => x.RoleTypeId == "LEAD", ct)
                    ?? await _context.RoleTypes.FirstAsync(x => x.RoleTypeId == "CONTACT", ct);

                var contactMechTypes = await _context.ContactMechTypes
                    .Where(cmt => new[] { "TELECOM_NUMBER", "EMAIL_ADDRESS", "POSTAL_ADDRESS" }.Contains(cmt.ContactMechTypeId))
                    .ToDictionaryAsync(x => x.ContactMechTypeId, ct);

                var purposeTypes = await _context.ContactMechPurposeTypes
                    .Where(cmp => new[] { "PRIMARY_PHONE", "PRIMARY_EMAIL", "GENERAL_LOCATION" }.Contains(cmp.ContactMechPurposeTypeId))
                    .ToDictionaryAsync(x => x.ContactMechPurposeTypeId, ct);

                // Generate ID
                var partyId = await _utilityService.GetNextSequence("Party");

                // Split FullName into FirstName and LastName
                var fullName = dto.FullName ?? "";
                var nameParts = fullName.Split(' ', 2);
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                // Create Party
                var party = new Party
                {
                    PartyId = partyId.ToString(),
                    PartyType = partyTypePerson,
                    Status = statusEnabled,
                    MainRole = "LEAD",
                    DataSourceId = dto.DataSourceId,
                    Description = fullName,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.Parties.Add(party);

                // Create Person
                _context.Persons.Add(new Person
                {
                    Party = party,
                    FirstName = firstName,
                    LastName = lastName,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });

                // Create PartyGroup (required by OFBiz schema)
                _context.PartyGroups.Add(new PartyGroup { Party = party });

                // Assign LEAD role
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

                // Phone
                if (!string.IsNullOrWhiteSpace(dto.Phone))
                {
                    await AddPhone(party, dto.Phone, "PRIMARY_PHONE", stamp);
                }

                // Mobile Phone
                if (!string.IsNullOrWhiteSpace(dto.MobilePhone))
                {
                    await AddPhone(party, dto.MobilePhone, "PHONE_MOBILE", stamp);
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

                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<LeadDto>.Failure("Failed to create lead");
                }

                await transaction.CommitAsync(ct);

                // Return created lead
                var result = new LeadDto
                {
                    PartyId = party.PartyId,
                    FirstName = firstName,
                    LastName = lastName,
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

                return Result<LeadDto>.Success(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<LeadDto>.Failure($"Error creating lead: {ex.Message}");
            }
        }

        private async Task AddPhone(Party party, string phoneNumber, string purposeTypeId, DateTime stamp)
        {
            var cmId = await _utilityService.GetNextSequence("ContactMech");
            var cm = new ContactMech
            {
                ContactMechId = cmId.ToString(),
                ContactMechTypeId = "TELECOM_NUMBER",
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

            _context.PartyContactMechPurposes.Add(new PartyContactMechPurpose
            {
                Party = party,
                ContactMech = cm,
                ContactMechPurposeTypeId = purposeTypeId,
                FromDate = stamp,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });
        }
    }
}
