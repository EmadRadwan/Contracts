using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects
{
    public class DeleteProjectCertificate
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string WorkEffortId { get; set; } = string.Empty;
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.WorkEffortId)
                    .NotEmpty().WithMessage("Work Effort ID is required");
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                _context.Database.SetCommandTimeout(300);
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var certificateHeader = await _context.WorkEfforts
                        .Include(we => we.CurrentStatus)
                        .FirstOrDefaultAsync(we => we.WorkEffortId == request.WorkEffortId, cancellationToken);

                    if (certificateHeader == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<Unit>.Failure("Certificate not found");
                    }


                    var relatedOrderId = certificateHeader.RelatedOrderId;
                    certificateHeader.RelatedOrderId = null;
                    await _context.SaveChangesAsync(cancellationToken);

                    // 1. Delete child certificate items
                    var childItems = await _context.WorkEfforts
                        .Where(we => we.WorkEffortParentId == request.WorkEffortId)
                        .ToListAsync(cancellationToken);

                    _context.WorkEfforts.RemoveRange(childItems);

                    // 2. If there is a related order → delete it and all dependent rows first
                    if (!string.IsNullOrEmpty(relatedOrderId))
                    {
                        // Delete dependent tables in correct order (children first)
                        await _context.Set<OrderItemShipGroupAssoc>()
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);
                        
                        await _context.OrderItemShipGroups
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);
                        
                        await _context.Set<OrderItemBilling>()
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);

                        await _context.OrderItems
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);

                        await _context.OrderAdjustments
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);

                        await _context.OrderStatuses
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);

                        await _context.OrderRoles
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);
                        
                        
                        
                        var paymentPrefIds = await _context.OrderPaymentPreferences
                            .Where(p => p.OrderId == relatedOrderId)
                            .Select(p => p.OrderPaymentPreferenceId)
                            .ToListAsync(cancellationToken);

                        if (paymentPrefIds.Any())
                        {
                            await _context.Payments
                                .Where(p => paymentPrefIds.Contains(p.PaymentPreferenceId))
                                .ExecuteDeleteAsync(cancellationToken);
                        }

                        await _context.OrderPaymentPreferences
                            .Where(p => p.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);


                        // ← Delete the order header BEFORE touching the WorkEffort
                        await _context.OrderHeaders
                            .Where(h => h.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);
                    }

                    // 3. Now it's safe to delete the certificate header
                    // (no FK violation because RELATED_ORDER_ID either was null or now points to a non-existing row — but MySQL won't complain because the row is gone)
                    _context.WorkEfforts.Remove(certificateHeader);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<Unit>.Failure($"Failed to delete certificate: {ex.Message}");
                }
            }
        }
    }
}