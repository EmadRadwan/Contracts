using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Services;

public class GetPartySubLedgerDetails
{
    public class Query : IRequest<Result<PartySubLedgerResponse>>
    {
        public string PartyId { get; set; } = string.Empty;
        public string? OrganizationPartyId { get; set; }
        public string? DefaultCurrencyUomId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PartySubLedgerResponse>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly IAcctgMiscService _acctgMiscService; // if needed for currency prefs

        public Handler(
            DataContext context,
            IUserAccessor userAccessor,
            IAcctgMiscService acctgMiscService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _acctgMiscService = acctgMiscService;
        }

        public async Task<Result<PartySubLedgerResponse>> Handle(Query request, CancellationToken ct)
        {
            try
            {
                string orgId = request.OrganizationPartyId
                               ?? (await GetCurrentOrganizationPartyId(ct))
                               ?? throw new Exception("Organization not determined");

                string currencyUomId = request.DefaultCurrencyUomId
                                       ?? (await GetPartyOrOrgCurrency(request.PartyId, orgId, ct));

                // 1. Fetch all relevant PartyGlAccount mappings
                var partyGlAccounts = await _context.PartyGlAccounts
                    .Where(pga => pga.PartyId == request.PartyId
                                  && pga.OrganizationPartyId == orgId)
                    .Select(pga => new
                    {
                        pga.RoleTypeId,
                        pga.GlAccountTypeId,
                        pga.GlAccountId
                    })
                    .Distinct()
                    .ToListAsync(ct);

                if (!partyGlAccounts.Any())
                {
                    return Result<PartySubLedgerResponse>.Failure("No GL accounts configured for this party.");
                }

                // 2. For each GL account → fetch entries where AcctgTransEntry.GlAccountId matches
                var subLedgers = new List<SubLedgerGroup>();

                foreach (var pga in partyGlAccounts)
                {
                    var entries = await _context.AcctgTransEntries
                        .Include(ate => ate.AcctgTrans)
                        .Where(ate => ate.GlAccountId == pga.GlAccountId
                                      && ate.AcctgTrans.IsPosted == "Y"
                                      && ate.AcctgTrans.PartyId == request.PartyId) // important filter!
                        .Select(ate => new SubLedgerEntry
                        {
                            TransactionDate = ate.AcctgTrans.TransactionDate,
                            TransactionId = ate.AcctgTrans.AcctgTransId,
                            Description = ate.AcctgTrans.Description ?? ate.AcctgTrans.AcctgTransTypeId,
                            DebitCreditFlag = ate.DebitCreditFlag,
                            Amount = (decimal)ate.Amount,
                            CurrencyUomId = ate.CurrencyUomId ?? ate.OrigCurrencyUomId ?? currencyUomId,
                            GlAccountId = ate.GlAccountId,
                            GlAccountTypeId = ate.GlAccountTypeId,
                            RoleTypeId = pga.RoleTypeId
                        })
                        .OrderBy(e => e.TransactionDate)
                        .ThenBy(e => e.TransactionId)
                        .ToListAsync(ct);

                    if (!entries.Any()) continue;

                    // Compute running balance (from company perspective or external?)
                    decimal runningBalance = 0m;
                    var detailedRows = new List<SubLedgerEntry>();

                    foreach (var e in entries)
                    {
                        decimal signed = e.DebitCreditFlag == "D" ? e.Amount : -e.Amount;

                        // Decide sign direction based on account type / role
                        // Example logic – adjust per your needs
                        bool isLiabilityOrEquity = pga.GlAccountTypeId?.Contains("PAYABLE") == true
                                                   || pga.GlAccountTypeId?.Contains("EQUITY") == true;

                        decimal impact = isLiabilityOrEquity ? -signed : signed; // typical flip for AP/Equity

                        runningBalance += impact;

                        e.RunningBalance = Math.Round(runningBalance, 2, MidpointRounding.AwayFromZero);
                        detailedRows.Add(e);
                    }

                    subLedgers.Add(new SubLedgerGroup
                    {
                        RoleTypeId = pga.RoleTypeId,
                        GlAccountId = pga.GlAccountId,
                        GlAccountTypeId = pga.GlAccountTypeId,
                        Entries = detailedRows,
                        FinalBalance = runningBalance
                    });
                }

                var response = new PartySubLedgerResponse
                {
                    PartyId = request.PartyId,
                    CurrencyUomId = currencyUomId,
                    SubLedgers = subLedgers
                };

                return Result<PartySubLedgerResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<PartySubLedgerResponse>.Failure(ex.Message);
            }
        }

        private async Task<string?> GetCurrentOrganizationPartyId(CancellationToken ct)
        {
            var username = _userAccessor.GetUsername();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username, ct);
            return user?.OrganizationPartyId;
        }

        private async Task<string> GetPartyOrOrgCurrency(string partyId, string orgId, CancellationToken ct)
        {
            var party = await _context.Parties.FindAsync(new object[] { partyId });
            if (!string.IsNullOrEmpty(party?.PreferredCurrencyUomId))
                return party.PreferredCurrencyUomId;

            var prefs = await _acctgMiscService.GetPartyAccountingPreferences(orgId);
            return prefs?.BaseCurrencyUomId ?? "EGP";
        }
    }
}

// DTOs (add to Models folder)

public class PartySubLedgerResponse
{
    public string PartyId { get; set; } = string.Empty;
    public string CurrencyUomId { get; set; } = "EGP";
    public List<SubLedgerGroup> SubLedgers { get; set; } = new();
}

public class SubLedgerGroup
{
    public string? RoleTypeId { get; set; }
    public string GlAccountId { get; set; } = string.Empty;
    public string? GlAccountTypeId { get; set; }
    public List<SubLedgerEntry> Entries { get; set; } = new();
    public decimal FinalBalance { get; set; }
}

public class SubLedgerEntry
{
    public DateTime? TransactionDate { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DebitCreditFlag { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyUomId { get; set; } = string.Empty;
    public string GlAccountId { get; set; } = string.Empty;
    public string? GlAccountTypeId { get; set; }
    public string? RoleTypeId { get; set; }
    public decimal RunningBalance { get; set; }
}