using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class CreateLeadsBatch
{
    public record Command : IRequest<Result<BatchCreateLeadsResult>>
    {
        public List<PartyDto2> Leads { get; init; } = new();
    }

    public record BatchCreateLeadsResult
    {
        public int TotalReceived { get; init; }
        public int Successful { get; init; }
        public int Failed { get; init; }
        public List<BatchLeadError> Errors { get; init; } = new();

        /// <summary>
        /// PartyIds of the leads created by this batch, so the caller can
        /// follow up with a bulk assignment. Creation itself never assigns.
        /// </summary>
        public List<string> CreatedPartyIds { get; init; } = new();
    }

    public record BatchLeadError
    {
        public int Index { get; init; }
        public string? FirstName { get; init; }
        public string? Email { get; init; }
        public string Reason { get; init; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, Result<BatchCreateLeadsResult>>
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

        public async Task<Result<BatchCreateLeadsResult>> Handle(Command request, CancellationToken ct)
        {
            if (request.Leads == null || request.Leads.Count == 0)
            {
                return Result<BatchCreateLeadsResult>.Success(new BatchCreateLeadsResult
                {
                    TotalReceived = 0,
                    Successful = 0,
                    Failed = 0,
                    Errors = new List<BatchLeadError>()
                });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var dtoList = request.Leads;
                var stamp = DateTime.UtcNow;

                // ==================== SHARED LOOKUPS ====================
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
                        "PRIMARY_PHONE", "PRIMARY_EMAIL", "GENERAL_LOCATION",
                        "SHIPPING_LOCATION", "PHONE_MOBILE"
                    }.Contains(x.ContactMechPurposeTypeId))
                    .ToDictionaryAsync(x => x.ContactMechPurposeTypeId, ct);

                // Detect duplicate emails within the batch
                var emailsInBatch = dtoList
                    .Where(d => !string.IsNullOrWhiteSpace(d.InfoString))
                    .Select(d => d.InfoString!.Trim().ToLower())
                    .GroupBy(e => e)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToHashSet();

                // ==================== MUTABLE COUNTERS ====================
                int successful = 0;
                int failed = 0;
                var errors = new List<BatchLeadError>();
                var createdPartyIds = new List<string>();

                for (int i = 0; i < dtoList.Count; i++)
                {
                    var dto = dtoList[i];

                    try
                    {
                        // ------------------- Validation -------------------
                        if (string.IsNullOrWhiteSpace(dto.FirstName))
                        {
                            errors.Add(new BatchLeadError
                            {
                                Index = i,
                                FirstName = dto.FirstName,
                                Email = dto.InfoString,
                                Reason = "FirstName is required"
                            });
                            failed++;
                            continue;
                        }

                        var email = dto.InfoString?.Trim().ToLower();
                        if (!string.IsNullOrWhiteSpace(email) && emailsInBatch.Contains(email))
                        {
                            errors.Add(new BatchLeadError
                            {
                                Index = i,
                                FirstName = dto.FirstName,
                                Email = dto.InfoString,
                                Reason = "Duplicate email in batch"
                            });
                            failed++;
                            continue;
                        }

                        // ------------------- Create Party -------------------
                        var partyId = (await _utilityService.GetNextSequence("Party")).ToString();
                        var fullName = $"{dto.FirstName ?? ""} {dto.MiddleName ?? ""}".Trim();

                        var party = new Party
                        {
                            PartyId = partyId,
                            PartyType = partyTypePerson,
                            Status = statusEnabled,
                            Description = fullName,
                            LeadTemperatureId = "F",
                            MainRole = "LEAD_CONTACT",
                            
                            DataSourceId = string.IsNullOrWhiteSpace(dto.DataSourceId)
                                ? "OTHER"
                                : dto.DataSourceId,
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

                        // Required OFBiz PartyGroup
                        _context.PartyGroups.Add(new PartyGroup { Party = party });

                        // Role = LEAD
                        _context.PartyRoles.Add(new PartyRole
                        {
                            Party = party,
                            RoleType = roleTypeLead,
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp
                        });

                        // Status History
                        _context.PartyStatuses.Add(new PartyStatus
                        {
                            Party = party,
                            Status = statusEnabled,
                            StatusDate = stamp,
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp
                        });

                        // ------------------- MOBILE PHONE -------------------
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

                        // ------------------- EMAIL -------------------
                        if (!string.IsNullOrWhiteSpace(dto.InfoString))
                        {
                            var cmId = (await _utilityService.GetNextSequence("ContactMech")).ToString();

                            var cm = new ContactMech
                            {
                                ContactMechId = cmId,
                                InfoString = dto.InfoString.Trim(),
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

                        // ------------------- POSTAL ADDRESS -------------------
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

                        // ------------------- DATA SOURCE -------------------
                        _context.PartyDataSources.Add(new PartyDataSource
                        {
                            Party = party,
                            DataSourceId = string.IsNullOrWhiteSpace(dto.DataSourceId)
                                ? "OTHER"
                                : dto.DataSourceId,
                            FromDate = stamp,
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp
                        });

                        createdPartyIds.Add(partyId);
                        successful++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new BatchLeadError
                        {
                            Index = i,
                            FirstName = dto.FirstName,
                            Email = dto.InfoString,
                            Reason = $"Exception: {ex.Message}"
                        });
                        failed++;
                    }
                }

                // ==================== BUILD IMMUTABLE RESULT ====================
                var result = new BatchCreateLeadsResult
                {
                    TotalReceived = dtoList.Count,
                    Successful = successful,
                    Failed = failed,
                    Errors = errors,
                    CreatedPartyIds = createdPartyIds
                };

                if (result.Successful > 0)
                {
                    await _context.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }
                else
                {
                    await transaction.RollbackAsync(ct);
                }

                return Result<BatchCreateLeadsResult>.Success(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<BatchCreateLeadsResult>.Failure($"Batch lead creation failed: {ex.Message}");
            }
        }
    }
}