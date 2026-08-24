using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.Leads.Assignment;

/// <summary>
/// Assigns a Lead to a sales rep, or reassigns it to a different one.
///
/// Reassignment closes the currently open LEAD_OWNER relationship (sets ThruDate)
/// and opens a new one, so the full ownership history is preserved.
/// Assigning a lead to the rep who already owns it is a no-op.
/// </summary>
public class AssignLead
{
    public record Command : IRequest<Result<LeadAssignmentDto>>
    {
        public string LeadPartyId { get; init; } = null!;
        public string OwnerPartyId { get; init; } = null!;
        public string? Comments { get; init; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.LeadPartyId)
                .NotEmpty().WithMessage("Lead is required");

            RuleFor(x => x.OwnerPartyId)
                .NotEmpty().WithMessage("Owner is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<LeadAssignmentDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<LeadAssignmentDto>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var stamp = DateTime.UtcNow;
                var actingUserId = _userAccessor.GetUserId();

                // ---------- Validate the lead ----------
                var lead = await _context.Parties
                    .Include(p => p.Person)
                    .FirstOrDefaultAsync(p => p.PartyId == request.LeadPartyId, ct);

                if (lead == null)
                    return Result<LeadAssignmentDto>.Failure($"Lead '{request.LeadPartyId}' not found");

                var isLead = await _context.PartyRoles.AnyAsync(
                    pr => pr.PartyId == request.LeadPartyId
                       && pr.RoleTypeId == LeadAssignmentConstants.LeadRoleTypeId, ct);

                if (!isLead)
                    return Result<LeadAssignmentDto>.Failure($"Party '{request.LeadPartyId}' is not a Lead");

                // ---------- Validate the owner ----------
                var owner = await _context.Parties
                    .FirstOrDefaultAsync(p => p.PartyId == request.OwnerPartyId, ct);

                if (owner == null)
                    return Result<LeadAssignmentDto>.Failure($"Owner '{request.OwnerPartyId}' not found");

                // PARTY_RELATIONSHIP has FKs on (PartyId, RoleTypeId) at both ends, so the
                // rep must genuinely hold SALES_REP - we do not create the role implicitly.
                var isSalesRep = await _context.PartyRoles.AnyAsync(
                    pr => pr.PartyId == request.OwnerPartyId
                       && pr.RoleTypeId == LeadAssignmentConstants.OwnerRoleTypeId, ct);

                if (!isSalesRep)
                    return Result<LeadAssignmentDto>.Failure(
                        $"Party '{request.OwnerPartyId}' is not a Sales Rep and cannot own leads");

                // ---------- Current assignment ----------
                var current = await _context.PartyRelationships
                    .FirstOrDefaultAsync(pr =>
                        pr.PartyIdTo == request.LeadPartyId
                        && pr.PartyRelationshipTypeId == LeadAssignmentConstants.RelationshipTypeId
                        && pr.ThruDate == null, ct);

                // Already owned by this rep - nothing to do.
                if (current != null && current.PartyIdFrom == request.OwnerPartyId)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<LeadAssignmentDto>.Success(new LeadAssignmentDto
                    {
                        LeadPartyId = request.LeadPartyId,
                        LeadName = lead.Description,
                        OwnerPartyId = current.PartyIdFrom,
                        OwnerName = owner.Description,
                        FromDate = current.FromDate,
                        ThruDate = current.ThruDate,
                        Comments = current.Comments
                    });
                }

                // Close the outgoing assignment.
                if (current != null)
                {
                    current.ThruDate = stamp;
                    current.LastUpdatedStamp = stamp;
                    current.LastUpdatedTxStamp = stamp;
                }

                // Open the new one.
                var assignment = new PartyRelationship
                {
                    PartyIdFrom = request.OwnerPartyId,
                    RoleTypeIdFrom = LeadAssignmentConstants.OwnerRoleTypeId,
                    PartyIdTo = request.LeadPartyId,
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
                };

                _context.PartyRelationships.Add(assignment);

                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<LeadAssignmentDto>.Failure("Failed to assign lead");
                }

                await transaction.CommitAsync(ct);

                return Result<LeadAssignmentDto>.Success(new LeadAssignmentDto
                {
                    LeadPartyId = request.LeadPartyId,
                    LeadName = lead.Description,
                    OwnerPartyId = assignment.PartyIdFrom,
                    OwnerName = owner.Description,
                    FromDate = assignment.FromDate,
                    ThruDate = assignment.ThruDate,
                    Comments = assignment.Comments
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<LeadAssignmentDto>.Failure($"Error assigning lead: {ex.Message}");
            }
        }

    }
}
