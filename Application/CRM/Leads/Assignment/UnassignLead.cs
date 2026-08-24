using Application.Core;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.Leads.Assignment;

/// <summary>
/// Removes the current owner from a Lead, returning it to the unassigned pool.
///
/// The open LEAD_OWNER relationship is closed rather than deleted, so the record
/// of who used to own the lead survives.
/// </summary>
public class UnassignLead
{
    public record Command : IRequest<Result<LeadAssignmentDto>>
    {
        public string LeadPartyId { get; init; } = null!;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.LeadPartyId)
                .NotEmpty().WithMessage("Lead is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<LeadAssignmentDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<LeadAssignmentDto>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var stamp = DateTime.UtcNow;

                var lead = await _context.Parties
                    .FirstOrDefaultAsync(p => p.PartyId == request.LeadPartyId, ct);

                if (lead == null)
                    return Result<LeadAssignmentDto>.Failure($"Lead '{request.LeadPartyId}' not found");

                var current = await _context.PartyRelationships
                    .FirstOrDefaultAsync(pr =>
                        pr.PartyIdTo == request.LeadPartyId
                        && pr.PartyRelationshipTypeId == LeadAssignmentConstants.RelationshipTypeId
                        && pr.ThruDate == null, ct);

                // Already unassigned - treat as success so the call is idempotent.
                if (current == null)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<LeadAssignmentDto>.Success(new LeadAssignmentDto
                    {
                        LeadPartyId = request.LeadPartyId,
                        LeadName = lead.Description
                    });
                }

                current.ThruDate = stamp;
                current.LastUpdatedStamp = stamp;
                current.LastUpdatedTxStamp = stamp;

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return Result<LeadAssignmentDto>.Success(new LeadAssignmentDto
                {
                    LeadPartyId = request.LeadPartyId,
                    LeadName = lead.Description,
                    OwnerPartyId = null,
                    OwnerName = null
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<LeadAssignmentDto>.Failure($"Error unassigning lead: {ex.Message}");
            }
        }
    }
}
