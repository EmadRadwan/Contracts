#nullable enable
using Application.Core;
using Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.BillingAccounts;

public class UpdateBillingAccount
{
    // --------------------------------------------------------------------
    // 1. Command – receives DTO from frontend
    // --------------------------------------------------------------------
    public class Command : IRequest<Result<UpdateBillingAccountResponse>>
    {
        public UpdateBillingAccountRequest? Request { get; set; }
    }

    // --------------------------------------------------------------------
    // 2. Validator
    // --------------------------------------------------------------------
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Request!.BillingAccountId)
                .NotEmpty().WithMessage("Billing Account ID is required");
        }
    }

    // --------------------------------------------------------------------
    // 3. Handler – full implementation
    // --------------------------------------------------------------------
    public class Handler : IRequestHandler<Command, Result<UpdateBillingAccountResponse>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<UpdateBillingAccountResponse>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var dto = request.Request!;

            // --------------------------------------------------------------------
            // 1. User validation
            // --------------------------------------------------------------------
            var currentUsername = _userAccessor.GetUsername();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == currentUsername, cancellationToken);

            if (user == null)
                return Result<UpdateBillingAccountResponse>.Failure("Unauthorized: User not found");

            // --------------------------------------------------------------------
            // 2. Find existing billing account
            // --------------------------------------------------------------------
            var billingAccount = await _context.BillingAccounts
                .FirstOrDefaultAsync(ba => ba.BillingAccountId == dto.BillingAccountId, cancellationToken);

            if (billingAccount == null)
            {
                _logger.LogWarning("Billing Account with ID {BillingAccountId} not found.", dto.BillingAccountId);
                return Result<UpdateBillingAccountResponse>.Failure(
                    $"Billing Account with ID {dto.BillingAccountId} not found.");
            }

            // --------------------------------------------------------------------
            // 3. Transaction scope – atomic update
            // --------------------------------------------------------------------
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var now = DateTime.UtcNow;

                // ----------------------------------------------------------------
                // 4. Update allowed fields only (PartyId, ProjectId, Limit cannot be changed)
                // ----------------------------------------------------------------
                billingAccount.FromDate = dto.FromDate ?? billingAccount.FromDate;
                billingAccount.ThruDate = dto.ThruDate;
                billingAccount.Description = dto.Description?.Trim() ?? billingAccount.Description;
                billingAccount.LastUpdatedStamp = now;

                // ----------------------------------------------------------------
                // 5. Update BillingAccountRole dates if needed
                // ----------------------------------------------------------------
                var billingAccountRole = await _context.BillingAccountRoles
                    .FirstOrDefaultAsync(r => r.BillingAccountId == dto.BillingAccountId, cancellationToken);

                if (billingAccountRole != null)
                {
                    billingAccountRole.FromDate = dto.FromDate ?? billingAccountRole.FromDate;
                    billingAccountRole.ThruDate = dto.ThruDate;
                    billingAccountRole.LastUpdatedStamp = now;
                }

                // ----------------------------------------------------------------
                // 6. Persist changes
                // ----------------------------------------------------------------
                var saved = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogWarning("Failed to update billing account {BillingAccountId}.", dto.BillingAccountId);
                    return Result<UpdateBillingAccountResponse>.Failure("Failed to update billing account.");
                }

                await transaction.CommitAsync(cancellationToken);

                // ----------------------------------------------------------------
                // 7. Return updated billing account data
                // ----------------------------------------------------------------
                var result = await (
                        from ba in _context.BillingAccounts
                        join role in _context.BillingAccountRoles on ba.BillingAccountId equals role.BillingAccountId
                        join party in _context.Parties on role.PartyId equals party.PartyId
                        join workEffort in _context.WorkEfforts on ba.WorkEffortId equals workEffort.WorkEffortId
                        where ba.BillingAccountId == dto.BillingAccountId
                        select new UpdateBillingAccountResponse
                        {
                            BillingAccountId = ba.BillingAccountId,
                            PartyId = role.PartyId,
                            PartyName = party.Description,
                            ProjectId = ba.WorkEffortId,
                            ProjectName = workEffort.WorkEffortName,
                            AccountLimit = ba.AccountLimit,
                            AvailableBalance = ba.AccountLimit, // TODO: calculate actual balance
                            FromDate = ba.FromDate,
                            ThruDate = ba.ThruDate,
                            Description = ba.Description,
                            LastUpdatedDate = ba.LastUpdatedStamp
                        })
                    .FirstOrDefaultAsync(cancellationToken);

                return result != null
                    ? Result<UpdateBillingAccountResponse>.Success(result)
                    : Result<UpdateBillingAccountResponse>.Failure("Updated but failed to retrieve billing account");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to update billing account {BillingAccountId}.", dto.BillingAccountId);
                return Result<UpdateBillingAccountResponse>.Failure(
                    $"Failed to update billing account: {ex.Message}");
            }
        }
    }
}

public class UpdateBillingAccountRequest
{
    public string BillingAccountId { get; init; } = null!;

    // PartyId, ProjectId, and AccountLimit are intentionally excluded - cannot be changed

    public DateTime? FromDate { get; init; }

    public DateTime? ThruDate { get; init; }

    public string? Description { get; init; }
}

public class UpdateBillingAccountResponse
{
    public string? BillingAccountId { get; set; }
    public string? PartyId { get; set; }
    public string? PartyName { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public decimal? AccountLimit { get; set; }
    public decimal? AvailableBalance { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}
