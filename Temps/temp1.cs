// GetPartySubLedgerDetails.cs  (or a new file GetPartySubLedgerQuery.cs)

namespace Application.Accounting.Services;

public class GetPartySubLedgerDetailsQuery : IRequest<Result<PartySubLedgerResponse>>
{
    public string PartyId { get; set; } = string.Empty;
    public string? OrganizationPartyId { get; set; }
    public string? DefaultCurrencyUomId { get; set; }
}

public class GetPartySubLedgerDetailsHandler 
    : IRequestHandler<GetPartySubLedgerDetailsQuery, Result<PartySubLedgerResponse>>
{
    // ... same constructor and Handle method ...

    // Just change Query → GetPartySubLedgerDetailsQuery everywhere inside Handle
}