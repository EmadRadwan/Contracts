using System.Text.RegularExpressions;
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

    /// <summary>
    /// A lead already in the database that holds one of the contacts being imported.
    /// </summary>
    public record ExistingLeadRow
    {
        public string Value { get; init; } = string.Empty;
        public string PartyId { get; init; } = string.Empty;
        public string? Name { get; init; }

        public string Describe() =>
            string.IsNullOrWhiteSpace(Name) ? $"Lead {PartyId}" : $"{Name} ({PartyId})";
    }

    public class Handler : IRequestHandler<Command, Result<BatchCreateLeadsResult>>
    {
        /// <summary>
        /// The shape optionalEmailValidator accepts on the single lead form, anchored
        /// so a value cannot pass by merely containing an address somewhere inside it.
        /// </summary>
        private static readonly Regex EmailPattern =
            new(@"^\S+@\S+\.\S+$", RegexOptions.Compiled);

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

                // Country resolution table. PostalAddress.CountryGeoId is a foreign
                // key (POST_ADDR_CGEO) and this batch saves once, after the loop -
                // so an unknown country cannot be caught by the per-row try/catch
                // below. It would take the whole import down, valid rows included.
                // Resolving (and rejecting) up front is what keeps a bad cell a
                // row-level failure.
                var countries = await _context.Geos
                    .Where(g => g.GeoTypeId == "COUNTRY")
                    .Select(g => new { g.GeoId, g.GeoName, g.GeoCode, g.Abbreviation })
                    .ToListAsync(ct);

                // Spreadsheets carry country names far more often than geo ids, so
                // accept either. Ids are loaded first: an id always beats another
                // country's name if the two ever collide.
                var countryLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in countries)
                    countryLookup.TryAdd(c.GeoId.Trim(), c.GeoId);
                foreach (var c in countries)
                    foreach (var alias in new[] { c.GeoCode, c.Abbreviation, c.GeoName })
                        if (!string.IsNullOrWhiteSpace(alias))
                            countryLookup.TryAdd(alias.Trim(), c.GeoId);

                // Detect duplicate emails within the batch
                var emailsInBatch = dtoList
                    .Where(d => !string.IsNullOrWhiteSpace(d.InfoString))
                    .Select(d => d.InfoString!.Trim().ToLower())
                    .GroupBy(e => e)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToHashSet();

                // ... and duplicate mobile numbers, which the single-lead path also
                // treats as identifying.
                var mobilesInBatch = dtoList
                    .Where(d => !string.IsNullOrWhiteSpace(d.MobileContactNumber))
                    .Select(d => d.MobileContactNumber!.Trim())
                    .GroupBy(m => m)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToHashSet();

                // Leads that ALREADY EXIST with one of the contacts in this file.
                // CreateLead (single) has always refused these; the batch used to
                // import them happily, so re-uploading a file duplicated everyone.
                // Two queries for the whole batch, not two per row.
                var batchEmails = dtoList
                    .Where(d => !string.IsNullOrWhiteSpace(d.InfoString))
                    .Select(d => d.InfoString!.Trim())
                    .Distinct()
                    .ToList();

                var batchMobiles = dtoList
                    .Where(d => !string.IsNullOrWhiteSpace(d.MobileContactNumber))
                    .Select(d => d.MobileContactNumber!.Trim())
                    .Distinct()
                    .ToList();

                // Comparison is left to the column collation, exactly as the single
                // create does - no ToLower() on the column, so the index still works.
                var existingEmailRows = batchEmails.Count == 0
                    ? new List<ExistingLeadRow>()
                    : await _context.PartyContactMeches
                        .Where(pcm => pcm.ContactMech.ContactMechTypeId == "EMAIL_ADDRESS"
                                   && pcm.ContactMech.InfoString != null
                                   && batchEmails.Contains(pcm.ContactMech.InfoString)
                                   && pcm.Party.PartyRoles.Any(r => r.RoleType.RoleTypeId == "LEAD"))
                        .Select(pcm => new ExistingLeadRow
                        {
                            Value = pcm.ContactMech.InfoString!,
                            PartyId = pcm.PartyId,
                            Name = pcm.Party.Description
                        })
                        .ToListAsync(ct);

                var existingMobileRows = batchMobiles.Count == 0
                    ? new List<ExistingLeadRow>()
                    : await _context.PartyContactMeches
                        .Where(pcm => pcm.ContactMech.TelecomNumber != null
                                   && pcm.ContactMech.TelecomNumber.ContactNumber != null
                                   && batchMobiles.Contains(pcm.ContactMech.TelecomNumber.ContactNumber)
                                   && pcm.Party.PartyRoles.Any(r => r.RoleType.RoleTypeId == "LEAD"))
                        .Select(pcm => new ExistingLeadRow
                        {
                            Value = pcm.ContactMech.TelecomNumber!.ContactNumber!,
                            PartyId = pcm.PartyId,
                            Name = pcm.Party.Description
                        })
                        .ToListAsync(ct);

                var existingByEmail = existingEmailRows
                    .GroupBy(x => x.Value.Trim().ToLower())
                    .ToDictionary(g => g.Key, g => g.First());

                var existingByMobile = existingMobileRows
                    .GroupBy(x => x.Value.Trim())
                    .ToDictionary(g => g.Key, g => g.First());

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
                        // Mirrors the lead form: first name, last name, mobile and
                        // lead source are required; email is optional. Both entry
                        // points must agree on what a usable lead is.
                        string? missing = null;

                        if (string.IsNullOrWhiteSpace(dto.FirstName))
                            missing = "First Name is required";
                        else if (string.IsNullOrWhiteSpace(dto.MiddleName))
                            missing = "Last Name is required";
                        else if (string.IsNullOrWhiteSpace(dto.MobileContactNumber))
                            missing = "Mobile Number is required";
                        else if (string.IsNullOrWhiteSpace(dto.DataSourceId))
                            missing = "Lead source is required";

                        if (missing != null)
                        {
                            errors.Add(new BatchLeadError
                            {
                                Index = i,
                                FirstName = dto.FirstName,
                                Email = dto.InfoString,
                                Reason = missing
                            });
                            failed++;
                            continue;
                        }

                        var email = dto.InfoString?.Trim().ToLower();

                        // Email stays optional, but a supplied one has to be usable -
                        // the single form has always enforced this and the import
                        // did not, so nonsense addresses went straight into
                        // ContactMech rows typed EMAIL_ADDRESS.
                        if (!string.IsNullOrWhiteSpace(email) && !EmailPattern.IsMatch(email))
                        {
                            errors.Add(new BatchLeadError
                            {
                                Index = i,
                                FirstName = dto.FirstName,
                                Email = dto.InfoString,
                                Reason = "Email is not a valid format"
                            });
                            failed++;
                            continue;
                        }

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

                        var mobile = dto.MobileContactNumber?.Trim();
                        if (!string.IsNullOrWhiteSpace(mobile) && mobilesInBatch.Contains(mobile))
                        {
                            errors.Add(new BatchLeadError
                            {
                                Index = i,
                                FirstName = dto.FirstName,
                                Email = dto.InfoString,
                                Reason = "Duplicate mobile number in batch"
                            });
                            failed++;
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(email) &&
                            existingByEmail.TryGetValue(email, out var emailOwner))
                        {
                            errors.Add(new BatchLeadError
                            {
                                Index = i,
                                FirstName = dto.FirstName,
                                Email = dto.InfoString,
                                Reason = $"{emailOwner.Describe()} already has this email address"
                            });
                            failed++;
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(mobile) &&
                            existingByMobile.TryGetValue(mobile, out var mobileOwner))
                        {
                            errors.Add(new BatchLeadError
                            {
                                Index = i,
                                FirstName = dto.FirstName,
                                Email = dto.InfoString,
                                Reason = $"{mobileOwner.Describe()} already has this mobile number"
                            });
                            failed++;
                            continue;
                        }

                        // Resolve the country before anything is written. Validated
                        // whenever a value is supplied, even on rows with no address
                        // line - so a whole column of country names is reported at
                        // once instead of row by row as addresses happen to appear.
                        string? countryGeoId = null;
                        if (!string.IsNullOrWhiteSpace(dto.GeoId))
                        {
                            if (!countryLookup.TryGetValue(dto.GeoId.Trim(), out var resolvedCountry))
                            {
                                errors.Add(new BatchLeadError
                                {
                                    Index = i,
                                    FirstName = dto.FirstName,
                                    Email = dto.InfoString,
                                    Reason = $"Country '{dto.GeoId.Trim()}' is not a known country"
                                });
                                failed++;
                                continue;
                            }

                            countryGeoId = resolvedCountry;
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
                            MainRole = "LEAD",
                            
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
                                CountryGeoId = countryGeoId,
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