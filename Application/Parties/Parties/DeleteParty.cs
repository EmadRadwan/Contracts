using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Parties.Parties;

public class DeleteParty
{
    public class Command : IRequest<Result<Unit>>
    {
        public string PartyId { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var party = await _context.Parties
                .Include(p => p.PartyRoles)
                .Include(p => p.PartyStatuses)
                .Include(p => p.PartyContactMeches)
                .Include(p => p.PartyContactMechPurposes)
                .Include(p => p.GlAccountOrganizations)
                .Include(p => p.PartyGlAccountParties)
                .Include(p => p.EmplPositionFulfillments)
                .Include(p => p.EmplPositions)
                .Include(p => p.EmploymentPartyIdToNavigations)
                .Include(p => p.RateAmounts)
                .FirstOrDefaultAsync(x => x.PartyId == request.PartyId, cancellationToken);

            if (party == null) return Result<Unit>.Failure("الطرف غير موجود");

            // --- BUSINESS RULES / CHECK CONSTRAINTS ---
            // 1. Check if party has any Invoices (as From or To)
            var hasInvoices = await _context.Invoices.AnyAsync(i => 
                i.PartyIdFrom == request.PartyId || i.PartyId == request.PartyId, cancellationToken);
            if (hasInvoices) return Result<Unit>.Failure("لا يمكن حذف هذا الطرف لوجود فواتير مسجلة عليه");

            // 2. Check if party has any Payments (as From or To)
            var hasPayments = await _context.Payments.AnyAsync(p => 
                p.PartyIdFrom == request.PartyId || p.PartyIdTo == request.PartyId, cancellationToken);
            if (hasPayments) return Result<Unit>.Failure("لا يمكن حذف هذا الطرف لوجود دفعات مسجلة عليه");

            // 3. Check if party has any Orders (via OrderRoles)
            var hasOrders = await _context.OrderRoles.AnyAsync(or => 
                or.PartyId == request.PartyId, cancellationToken);
            if (hasOrders) return Result<Unit>.Failure("لا يمكن حذف هذا الطرف لوجود طلبات (Orders) مسجلة عليه");

            // 4. Check if party is used in AcctgTrans (Accounting Transactions)
            var hasAcctgTrans = await _context.AcctgTrans.AnyAsync(t => t.PartyId == request.PartyId, cancellationToken);
            if (hasAcctgTrans) return Result<Unit>.Failure("لا يمكن حذف هذا الطرف لوجود قيود محاسبية مسجلة عليه");
            
            // 5. Check if party is used in AcctgTransEntries
            var hasAcctgTransEntries = await _context.AcctgTransEntries.AnyAsync(e => e.PartyId == request.PartyId, cancellationToken);
            if (hasAcctgTransEntries) return Result<Unit>.Failure("لا يمكن حذف هذا الطرف لوجود قيود محاسبية مسجلة عليه");

            // 6. Check if party has any SalesRequests (as Customer or Employee)
            var hasSalesRequests = await _context.SalesRequests.AnyAsync(sr => 
                sr.FromPartyId == request.PartyId || sr.EmployeePartyId == request.PartyId, cancellationToken);
            if (hasSalesRequests) return Result<Unit>.Failure("لا يمكن حذف هذا الطرف لوجود طلبات حجز (Sales Requests) مسجلة عليه");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // --- DELETE RELATED RECORDS ---

                // 1. Employment specific (Employee)
                if (party.EmploymentPartyIdToNavigations.Any())
                {
                    _context.Employments.RemoveRange(party.EmploymentPartyIdToNavigations);
                }

                if (party.EmplPositionFulfillments.Any())
                {
                    _context.EmplPositionFulfillments.RemoveRange(party.EmplPositionFulfillments);
                }
                
                // Reporting structure
                var positionIds = party.EmplPositions.Select(p => p.EmplPositionId).ToList();
                var reportingStructs = await _context.EmplPositionReportingStructs
                    .Where(rs => positionIds.Contains(rs.EmplPositionIdManagedBy) || positionIds.Contains(rs.EmplPositionIdReportingTo))
                    .ToListAsync(cancellationToken);
                if (reportingStructs.Any()) _context.EmplPositionReportingStructs.RemoveRange(reportingStructs);

                if (party.EmplPositions.Any())
                {
                    _context.EmplPositions.RemoveRange(party.EmplPositions);
                }

                if (party.RateAmounts.Any())
                {
                    _context.RateAmounts.RemoveRange(party.RateAmounts);
                }

                // 2. Contact Mechanisms
                var contactMechIds = party.PartyContactMeches.Select(pcm => pcm.ContactMechId).ToList();
                
                if (party.PartyContactMechPurposes.Any()) _context.PartyContactMechPurposes.RemoveRange(party.PartyContactMechPurposes);
                if (party.PartyContactMeches.Any()) _context.PartyContactMeches.RemoveRange(party.PartyContactMeches);

                if (contactMechIds.Any())
                {
                    var telecomNumbers = await _context.TelecomNumbers.Where(tn => contactMechIds.Contains(tn.ContactMechId)).ToListAsync(cancellationToken);
                    if (telecomNumbers.Any()) _context.TelecomNumbers.RemoveRange(telecomNumbers);

                    var postalAddresses = await _context.PostalAddresses.Where(pa => contactMechIds.Contains(pa.ContactMechId)).ToListAsync(cancellationToken);
                    if (postalAddresses.Any()) _context.PostalAddresses.RemoveRange(postalAddresses);

                    var contactMeches = await _context.ContactMeches.Where(cm => contactMechIds.Contains(cm.ContactMechId)).ToListAsync(cancellationToken);
                    if (contactMeches.Any()) _context.ContactMeches.RemoveRange(contactMeches);
                }

                // 3. GL Accounts
                // Identify GL accounts that were specifically created for this party
                // They usually have ParentGlAccountId like 121100 (AR), 210000 (AP), 124100 (Loans), 220000 (Accrued)
                var partyGlAccounts = await _context.PartyGlAccounts
                    .Where(pga => pga.PartyId == request.PartyId)
                    .Include(pga => pga.GlAccount)
                    .ToListAsync(cancellationToken);

                var glAccountIdsToDelete = new List<string>();
                foreach (var pga in partyGlAccounts)
                {
                    if (pga.GlAccount != null && 
                        (pga.GlAccount.ParentGlAccountId == "121100" || 
                         pga.GlAccount.ParentGlAccountId == "210000" || 
                         pga.GlAccount.ParentGlAccountId == "124100" || 
                         pga.GlAccount.ParentGlAccountId == "220000"))
                    {
                        glAccountIdsToDelete.Add(pga.GlAccountId);
                    }
                }

                if (party.PartyGlAccountParties.Any()) _context.PartyGlAccounts.RemoveRange(party.PartyGlAccountParties);
                
                if (glAccountIdsToDelete.Any())
                {
                    var glOrgs = await _context.GlAccountOrganizations.Where(gao => glAccountIdsToDelete.Contains(gao.GlAccountId)).ToListAsync(cancellationToken);
                    if (glOrgs.Any()) _context.GlAccountOrganizations.RemoveRange(glOrgs);

                    var glAccounts = await _context.GlAccounts.Where(ga => glAccountIdsToDelete.Contains(ga.GlAccountId)).ToListAsync(cancellationToken);
                    if (glAccounts.Any()) _context.GlAccounts.RemoveRange(glAccounts);
                }

                // 4. Person / PartyGroup
                var person = await _context.Persons.FirstOrDefaultAsync(p => p.PartyId == request.PartyId, cancellationToken);
                if (person != null) _context.Persons.Remove(person);

                var partyGroup = await _context.PartyGroups.FirstOrDefaultAsync(pg => pg.PartyId == request.PartyId, cancellationToken);
                if (partyGroup != null) _context.PartyGroups.Remove(partyGroup);

                // 5. PartyRole and PartyStatus
                if (party.PartyRoles.Any()) _context.PartyRoles.RemoveRange(party.PartyRoles);
                if (party.PartyStatuses.Any()) _context.PartyStatuses.RemoveRange(party.PartyStatuses);

                // 6. Finally delete the Party itself
                _context.Parties.Remove(party);

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (success)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return Result<Unit>.Success(Unit.Value);
                }
                
                await transaction.RollbackAsync(cancellationToken);
                return Result<Unit>.Failure("فشل في حذف الطرف");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error deleting party {PartyId}", request.PartyId);
                
                // Return a suitable message in case of DB constraints not caught by business rules
                return Result<Unit>.Failure("لا يمكن حذف الطرف لوجود بيانات مرتبطة به في النظام: " + ex.Message);
            }
        }
    }
}
