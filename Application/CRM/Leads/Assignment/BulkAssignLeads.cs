using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.Leads.Assignment;

/// <summary>
/// Assigns many Leads to one sales rep in a single transaction.
///
/// Mirrors CreateLeadsBatch: individual leads can fail without sinking the
/// whole batch, and the caller gets a per-lead reason for every failure.
/// </summary>
public class BulkAssignLeads
{
    public record Command : IRequest<Result<BulkAssignResult>>
    {
        public List<string> LeadPartyIds { get; init; } = new();
        public string OwnerPartyId { get; init; } = null!;
        public string? Comments { get; init; }
    }

    public record BulkAssignResult
    {
        public int TotalReceived { get; init; }
        public int Successful { get; init; }
        public int Failed { get; init; }
        public int AlreadyOwned { get; init; }
        public string? OwnerPartyId { get; init; }
        public string? OwnerName { get; init; }
        public List<BulkAssignError> Errors { get; init; } = new();
    }

    public record BulkAssignError
    {
        public string LeadPartyId { get; init; } = string.Empty;
        public string? LeadName { get; init; }
        public string Reason { get; init; } = string.Empty;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.OwnerPartyId)
                .NotEmpty().WithMessage("Owner is required");

            RuleFor(x => x.LeadPartyIds)
                .NotEmpty().WithMessage("At least one lead is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<BulkAssignResult>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<BulkAssignResult>> Handle(Command request, CancellationToken ct)
        {
            if (request.LeadPartyIds == null || request.LeadPartyIds.Count == 0)
            {
                return Result<BulkAssignResult>.Success(new BulkAssignResult
                {
                    TotalReceived = 0,
                    OwnerPartyId = request.OwnerPartyId
                });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var stamp = DateTime.UtcNow;
                var actingUserId = _userAccessor.GetUserId();

                // ---------- Validate the owner once, not per lead ----------
                var owner = await _context.Parties
                    .FirstOrDefaultAsync(p => p.PartyId == request.OwnerPartyId, ct);

                if (owner == null)
                    return Result<BulkAssignResult>.Failure($"Owner '{request.OwnerPartyId}' not found");

                var isSalesRep = await _context.PartyRoles.AnyAsync(
                    pr => pr.PartyId == request.OwnerPartyId
                       && pr.RoleTypeId == LeadAssignmentConstants.OwnerRoleTypeId, ct);

                if (!isSalesRep)
                    return Result<BulkAssignResult>.Failure(
                        $"Party '{request.OwnerPartyId}' is not a Sales Rep and cannot own leads");

                // De-duplicate the incoming list so the same lead cannot be
                // processed twice within one batch.
                var leadIds = request.LeadPartyIds
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim())
                    .Distinct()
                    .ToList();

                // ---------- Bulk-load everything the loop needs ----------
                var leads = await _context.Parties
                    .Where(p => leadIds.Contains(p.PartyId))
                    .Select(p => new { p.PartyId, p.Description })
                    .ToDictionaryAsync(x => x.PartyId, ct);

                var leadRoleHolders = await _context.PartyRoles
                    .Where(pr => leadIds.Contains(pr.PartyId)
                              && pr.RoleTypeId == LeadAssignmentConstants.LeadRoleTypeId)
                    .Select(pr => pr.PartyId)
                    .ToListAsync(ct);

                var leadRoleSet = leadRoleHolders.ToHashSet();

                var openAssignments = await _context.PartyRelationships
                    .Where(pr => leadIds.Contains(pr.PartyIdTo)
                              && pr.PartyRelationshipTypeId == LeadAssignmentConstants.RelationshipTypeId
                              && pr.ThruDate == null)
                    .ToListAsync(ct);

                var openByLead = openAssignments.ToDictionary(pr => pr.PartyIdTo);

                int successful = 0, failed = 0, alreadyOwned = 0;
                var errors = new List<BulkAssignError>();

                foreach (var leadId in leadIds)
                {
                    if (!leads.TryGetValue(leadId, out var lead))
                    {
                        errors.Add(new BulkAssignError
                        {
                            LeadPartyId = leadId,
                            Reason = "Lead not found"
                        });
                        failed++;
                        continue;
                    }

                    if (!leadRoleSet.Contains(leadId))
                    {
                        errors.Add(new BulkAssignError
                        {
                            LeadPartyId = leadId,
                            LeadName = lead.Description,
                            Reason = "Party is not a Lead"
                        });
                        failed++;
                        continue;
                    }

                    openByLead.TryGetValue(leadId, out var current);

                    // Already owned by this rep - skip rather than writing a duplicate.
                    if (current != null && current.PartyIdFrom == request.OwnerPartyId)
                    {
                        alreadyOwned++;
                        continue;
                    }

                    if (current != null)
                    {
                        current.ThruDate = stamp;
                        current.LastUpdatedStamp = stamp;
                        current.LastUpdatedTxStamp = stamp;
                    }

                    _context.PartyRelationships.Add(new PartyRelationship
                    {
                        PartyIdFrom = request.OwnerPartyId,
                        RoleTypeIdFrom = LeadAssignmentConstants.OwnerRoleTypeId,
                        PartyIdTo = leadId,
                        RoleTypeIdTo = LeadAssignmentConstants.LeadRoleTypeId,
                        PartyRelationshipTypeId = LeadAssignmentConstants.RelationshipTypeId,
                        FromDate = stamp,
                        ThruDate = null,
                        Comments = request.Comments,
                        CreatedByUserLogin = actingUserId,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedTxStamp = stamp,
                        LastUpdatedTxStamp = stamp
                    });

                    successful++;
                }

                if (successful > 0)
                {
                    await _context.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }
                else
                {
                    await transaction.RollbackAsync(ct);
                }

                return Result<BulkAssignResult>.Success(new BulkAssignResult
                {
                    TotalReceived = leadIds.Count,
                    Successful = successful,
                    Failed = failed,
                    AlreadyOwned = alreadyOwned,
                    OwnerPartyId = request.OwnerPartyId,
                    OwnerName = owner.Description,
                    Errors = errors
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<BulkAssignResult>.Failure($"Bulk assign failed: {ex.Message}");
            }
        }

    }
}
