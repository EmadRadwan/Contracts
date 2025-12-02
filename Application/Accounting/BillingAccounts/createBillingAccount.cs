using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.BillingAccounts;

public class CreateBillingAccount
{
    // --------------------------------------------------------------------
    // 1. Command – receives DTO from frontend
    // --------------------------------------------------------------------
    public class Command : IRequest<Result<CreateBillingAccountResponse>>
    {
        public CreateBillingAccountRequest? Request { get; set; }
    }

    // --------------------------------------------------------------------
    // 2. Handler – full implementation
    // --------------------------------------------------------------------
    public class Handler : IRequestHandler<Command, Result<CreateBillingAccountResponse>>
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

        public async Task<Result<CreateBillingAccountResponse>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var dto = request.Request!; // guaranteed non-null by FluentValidation

            // --------------------------------------------------------------------
            // 1. User validation
            // --------------------------------------------------------------------
            var currentUsername = _userAccessor.GetUsername();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == currentUsername, cancellationToken);

            if (user == null)
                return Result<CreateBillingAccountResponse>.Failure("Unauthorized: User not found");

            // --------------------------------------------------------------------
            // 2. Business validation – duplicate PartyId + active account?
            // --------------------------------------------------------------------
            var hasExistingActiveAccount = await _context.BillingAccounts
                .Where(ba =>
                    ba.WorkEffortId == dto.ProjectId && // Same project
                    (ba.ThruDate == null || ba.ThruDate > DateTime.UtcNow)) // Not expired
                .Join(
                    _context.BillingAccountRoles,
                    ba => ba.BillingAccountId,
                    role => role.BillingAccountId,
                    (ba, role) => role)
                .AnyAsync(role =>
                        role.PartyId == dto.PartyId &&
                        (role.RoleTypeId == "BILL_FROM_VENDOR"),
                    cancellationToken);

            if (hasExistingActiveAccount)
            {
                return Result<CreateBillingAccountResponse>.Failure(
                    "This customer already has an active billing account for this project.");
            }

            var now = DateTime.UtcNow;

            // --------------------------------------------------------------------
            // 3. Transaction scope – atomic create
            // --------------------------------------------------------------------
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var newBillingAccountSerial = await _utilityService.GetNextSequence("BillingAccount");

                // ----------------------------------------------------------------
                // 4. Core BillingAccount entity
                // ----------------------------------------------------------------
                var billingAccount = new BillingAccount
                {
                    BillingAccountId = newBillingAccountSerial,
                    WorkEffortId = !string.IsNullOrWhiteSpace(dto.ProjectId) ? dto.ProjectId.Trim() : null,
                    AccountLimit = dto.AccountLimit,
                    AccountCurrencyUomId = "EGP",
                    FromDate = dto.FromDate,
                    ThruDate = dto.ThruDate,
                    Description = dto.Description?.Trim(),
                    CreatedStamp = now,
                    LastUpdatedStamp = now
                };

                _context.BillingAccounts.Add(billingAccount);

                var billingAccountRole = new BillingAccountRole
                {
                    BillingAccountId = newBillingAccountSerial,
                    PartyId = dto.PartyId.Trim(),
                    RoleTypeId = "BILL_FROM_VENDOR", // or BILL_FROM_VENDOR as needed
                    FromDate = dto.FromDate,
                    ThruDate = dto.ThruDate,
                    CreatedStamp = now,
                    LastUpdatedStamp = now
                };

                _context.BillingAccountRoles.Add(billingAccountRole);


                // ----------------------------------------------------------------
                // 6. Persist both records
                // ----------------------------------------------------------------
                var saved = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<CreateBillingAccountResponse>.Failure("Failed to save billing account");
                }

                await transaction.CommitAsync(cancellationToken);

                var result = await (
                        from ba in _context.BillingAccounts
                        join role in _context.BillingAccountRoles on ba.BillingAccountId equals role.BillingAccountId
                        join party in _context.Parties on role.PartyId equals party.PartyId
                        join workEffort in _context.WorkEfforts on ba.WorkEffortId equals workEffort.WorkEffortId
                        where ba.BillingAccountId == newBillingAccountSerial
                        select new CreateBillingAccountResponse
                        {
                            BillingAccountId = ba.BillingAccountId,
                            PartyId = role.PartyId,
                            PartyName = party.Description,
                            ProjectId = ba.WorkEffortId,
                            ProjectName = workEffort.WorkEffortName,
                            AccountLimit = ba.AccountLimit,
                            AvailableBalance = ba.AccountLimit, // initial balance
                            FromDate = ba.FromDate,
                            ThruDate = ba.ThruDate,
                            Description = ba.Description,
                            CreatedDate = ba.CreatedStamp
                        })
                    .FirstOrDefaultAsync(cancellationToken);


                return result != null
                    ? Result<CreateBillingAccountResponse>.Success(result)
                    : Result<CreateBillingAccountResponse>.Failure("Created but failed to retrieve billing account");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<CreateBillingAccountResponse>.Failure(
                    $"Failed to create billing account: {ex.Message}");
            }
        }
    }
}

public class CreateBillingAccountRequest
{
    public string PartyId { get; init; }

    public string? ProjectId { get; init; }

    public decimal AccountLimit { get; init; }

    public DateTime FromDate { get; init; }

    public DateTime? ThruDate { get; init; }

    public string? Description { get; init; }
}

public class CreateBillingAccountResponse
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
    public DateTime? CreatedDate { get; set; }
}