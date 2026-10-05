using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Services;

/// <summary>
/// Thrown when a pending ACCTG_TRANS_ENTRY points at a GL account that is not assigned to the
/// entry's organization. Handlers that return Result<T> catch this and surface <see cref="Exception.Message"/>.
/// </summary>
public class GlAccountNotAssignedException : InvalidOperationException
{
    public IReadOnlyList<(string GlAccountId, string OrganizationPartyId)> Missing { get; }

    public GlAccountNotAssignedException(
        IReadOnlyList<(string GlAccountId, string? AccountName, string OrganizationPartyId)> missing)
        : base(BuildMessage(missing))
    {
        Missing = missing.Select(m => (m.GlAccountId, m.OrganizationPartyId)).ToList();
    }

    private static string BuildMessage(
        IReadOnlyList<(string GlAccountId, string? AccountName, string OrganizationPartyId)> missing)
    {
        var ar = string.Join("، ", missing.Select(m =>
            string.IsNullOrWhiteSpace(m.AccountName) ? m.GlAccountId : $"{m.GlAccountId} ({m.AccountName})"));
        var en = string.Join(", ", missing.Select(m => $"{m.GlAccountId} → {m.OrganizationPartyId}"));
        return $"لا يمكن ترحيل القيد لأن حساب الأستاذ {ar} غير معيّن للمؤسسة. " +
               "عيّن الحساب للمؤسسة من شجرة الحسابات، أو اختر حساباً آخر في بند الفاتورة. " +
               $"(GL account not assigned to organization: {en}.)";
    }
}

/// <summary>
/// ACCTG_TRANS_ENTRY has FK ACCTTXENT_GLACOG on (GL_ACCOUNT_ID, ORGANIZATION_PARTY_ID) →
/// GL_ACCOUNT_ORGANIZATION. Without this guard a missing assignment surfaces only as a MySQL
/// FK violation at SaveChanges (Oct 2026: INV1775 approval failed 4× with "حدث خطأ ما" because
/// PINV_FXASTPRD_ITEM defaulted to 650000, which is not assigned to Company).
///
/// Entries are created from several places (AcctgTransService, GeneralLedgerService,
/// ApproveMultiPaymentCertificate), so the guard inspects the ChangeTracker right before save
/// instead of hooking each creation site.
/// </summary>
public interface IGlAccountOrganizationGuard
{
    /// <summary>Throws <see cref="GlAccountNotAssignedException"/> if any added/modified entry's GL account is unassigned.</summary>
    Task EnsurePendingEntriesAssignedAsync(CancellationToken ct = default);
}

public class GlAccountOrganizationGuard : IGlAccountOrganizationGuard
{
    private readonly DataContext _context;

    public GlAccountOrganizationGuard(DataContext context)
    {
        _context = context;
    }

    public async Task EnsurePendingEntriesAssignedAsync(CancellationToken ct = default)
    {
        // A NULL in either column is not checked by the FK, so it is not this guard's concern.
        var pairs = _context.ChangeTracker.Entries<AcctgTransEntry>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity)
            .Where(e => !string.IsNullOrEmpty(e.GlAccountId) && !string.IsNullOrEmpty(e.OrganizationPartyId))
            .Select(e => (GlAccountId: e.GlAccountId!, OrganizationPartyId: e.OrganizationPartyId!))
            .Distinct()
            .ToList();

        if (pairs.Count == 0) return;

        var accountIds = pairs.Select(p => p.GlAccountId).Distinct().ToList();

        var assigned = (await _context.GlAccountOrganizations
                .Where(o => accountIds.Contains(o.GlAccountId))
                .Select(o => new { o.GlAccountId, o.OrganizationPartyId })
                .ToListAsync(ct))
            .Select(o => (o.GlAccountId, o.OrganizationPartyId))
            .ToHashSet();

        // Assignments added in the same unit of work satisfy the FK too.
        foreach (var o in _context.ChangeTracker.Entries<GlAccountOrganization>()
                     .Where(e => e.State == EntityState.Added))
            assigned.Add((o.Entity.GlAccountId, o.Entity.OrganizationPartyId));

        var missingPairs = pairs.Where(p => !assigned.Contains(p)).ToList();
        if (missingPairs.Count == 0) return;

        var missingIds = missingPairs.Select(p => p.GlAccountId).Distinct().ToList();
        var names = await _context.GlAccounts
            .Where(g => missingIds.Contains(g.GlAccountId))
            .ToDictionaryAsync(g => g.GlAccountId, g => g.AccountNameArabic ?? g.AccountName, ct);

        throw new GlAccountNotAssignedException(missingPairs
            .Select(p => (p.GlAccountId, names.GetValueOrDefault(p.GlAccountId), p.OrganizationPartyId))
            .ToList());
    }
}
