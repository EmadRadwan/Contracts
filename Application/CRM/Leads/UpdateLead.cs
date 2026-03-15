using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.Leads;

/// <summary>
/// Updates an existing Lead (Person).
/// </summary>
public class UpdateLead
{
    public record Command : IRequest<Result<LeadDto>>
    {
        public LeadDto Lead { get; init; } = null!;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Lead.PartyId)
                .NotEmpty().WithMessage("Party ID is required");

            RuleFor(x => x.Lead.FullName)
                .NotEmpty().WithMessage("Full name is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<LeadDto>>
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;

        public Handler(DataContext context, IUtilityService utilityService)
        {
            _context = context;
            _utilityService = utilityService;
        }

        public async Task<Result<LeadDto>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var stamp = DateTime.UtcNow;
                var dto = request.Lead;

                var party = await _context.Parties
                    .Include(p => p.Person)
                    .Include(p => p.Status)
                    .Include(p => p.PartyContactMeches)
                        .ThenInclude(pcm => pcm.ContactMech)
                            .ThenInclude(cm => cm.TelecomNumber)
                    .Include(p => p.PartyContactMeches)
                        .ThenInclude(pcm => pcm.ContactMech)
                            .ThenInclude(cm => cm.PostalAddress)
                    .FirstOrDefaultAsync(p => p.PartyId == dto.PartyId, ct);

                if (party == null)
                    return Result<LeadDto>.Failure("Lead not found");

                // Split FullName into FirstName and LastName
                var fullName = dto.FullName ?? "";
                var nameParts = fullName.Split(' ', 2);
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                // Update Party
                party.DataSourceId = dto.DataSourceId;
                party.Description = fullName;
                party.LastUpdatedStamp = stamp;

                // Update Person
                if (party.Person != null)
                {
                    party.Person.FirstName = firstName;
                    party.Person.LastName = lastName;
                    party.Person.LastUpdatedStamp = stamp;
                }

                // Load contact mech types
                var contactMechTypes = await _context.ContactMechTypes
                    .Where(cmt => new[] { "TELECOM_NUMBER", "EMAIL_ADDRESS", "POSTAL_ADDRESS" }.Contains(cmt.ContactMechTypeId))
                    .ToDictionaryAsync(x => x.ContactMechTypeId, ct);

                var purposeTypes = await _context.ContactMechPurposeTypes
                    .Where(cmp => new[] { "PRIMARY_PHONE", "PRIMARY_EMAIL", "GENERAL_LOCATION" }.Contains(cmp.ContactMechPurposeTypeId))
                    .ToDictionaryAsync(x => x.ContactMechPurposeTypeId, ct);

                // Update Phone Numbers
                await UpdatePhone(party, dto.Phone, "PRIMARY_PHONE", stamp);
                await UpdatePhone(party, dto.MobilePhone, "PHONE_MOBILE", stamp);

                // Update Email
                var existingEmailCm = party.PartyContactMeches
                    .FirstOrDefault(pcm => pcm.ContactMech?.ContactMechType?.ContactMechTypeId == "EMAIL_ADDRESS");

                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    if (existingEmailCm?.ContactMech != null)
                    {
                        existingEmailCm.ContactMech.InfoString = dto.Email;
                        existingEmailCm.ContactMech.LastUpdatedStamp = stamp;
                    }
                    else
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
                }

                // Update Address
                var existingAddressCm = party.PartyContactMeches
                    .FirstOrDefault(pcm => pcm.ContactMech?.PostalAddress != null);

                if (!string.IsNullOrWhiteSpace(dto.Address1))
                {
                    if (existingAddressCm?.ContactMech?.PostalAddress != null)
                    {
                        var addr = existingAddressCm.ContactMech.PostalAddress;
                        addr.ToName = fullName;
                        addr.Address1 = dto.Address1;
                        addr.Address2 = dto.Address2;
                        addr.City = dto.City;
                        addr.PostalCode = dto.PostalCode;
                        addr.CountryGeoId = dto.CountryGeoId;
                        addr.LastUpdatedStamp = stamp;
                    }
                    else
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
                }

                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<LeadDto>.Failure("Failed to update lead");
                }

                await transaction.CommitAsync(ct);

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
                    StatusId = party.Status?.StatusId,
                    StatusDescription = party.Status?.Description,
                    CreatedStamp = party.CreatedStamp
                };

                return Result<LeadDto>.Success(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<LeadDto>.Failure($"Error updating lead: {ex.Message}");
            }
        }

        private async Task UpdatePhone(Party party, string? phoneNumber, string purposeTypeId, DateTime stamp)
        {
            var existingPcm = await _context.PartyContactMechPurposes
                .Include(pcmp => pcmp.ContactMech)
                    .ThenInclude(cm => cm!.TelecomNumber)
                .FirstOrDefaultAsync(pcmp => pcmp.PartyId == party.PartyId && pcmp.ContactMechPurposeTypeId == purposeTypeId);

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                // If phone is cleared, we could remove it, but often we just leave it or clear the number.
                // For now, if it exists and new value is empty, let's clear it.
                if (existingPcm?.ContactMech?.TelecomNumber != null)
                {
                    existingPcm.ContactMech.TelecomNumber.ContactNumber = "";
                    existingPcm.ContactMech.TelecomNumber.LastUpdatedStamp = stamp;
                }
                return;
            }

            if (existingPcm?.ContactMech?.TelecomNumber != null)
            {
                existingPcm.ContactMech.TelecomNumber.ContactNumber = phoneNumber;
                existingPcm.ContactMech.TelecomNumber.LastUpdatedStamp = stamp;
            }
            else
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
}
