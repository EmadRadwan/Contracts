var query = from ate in _context.AcctgTransEntries
join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
// REFACTOR: Added left join with AcctgTransTypes to include transaction type description
// Improves: Provides human-readable transaction type name alongside AcctgTransTypeId
// Context: AcctgTransTypeId is a foreign key; description enhances reporting clarity
join att in _context.AcctgTransTypes on act.AcctgTransTypeId equals att.AcctgTransTypeId into transTypes
from att in transTypes.DefaultIfEmpty()
join p in _context.Parties on ate.PartyId equals p.PartyId into parties
from p in parties.DefaultIfEmpty()
join prod in _context.Products on ate.ProductId equals prod.ProductId into products
from prod in products.DefaultIfEmpty()
where ate.OrganizationPartyId == request.OrganizationPartyId
&& ate.GlAccountId == request.GlAccountId
&& act.IsPosted == "Y"
&& act.GlFiscalTypeId == "ACTUAL"
select new TransactionEntryDto
{
    AcctgTransId = ate.AcctgTransId,
        AcctgTransEntrySeqId = ate.AcctgTransEntrySeqId,
        TransactionDate = (DateTime)act.TransactionDate,
    // REFACTOR: Use actual description from AcctgTransTypes if available, fallback to ID or "Unknown"
    // Improves: Eliminates need for separate lookup; ensures consistent naming
    AcctgTransTypeId = act.AcctgTransTypeId ?? "Unknown",
    AcctgTransTypeDescription = att != null ? att.Description : (act.AcctgTransTypeId ?? "Unknown"),
    GlFiscalTypeId = act.GlFiscalTypeId,
    InvoiceId = act.InvoiceId,
    PaymentId = act.PaymentId,
    WorkEffortId = act.WorkEffortId,
    ShipmentId = act.ShipmentId,
    PartyId = ate.PartyId,
    PartyName = p != null ? p.Description : null,
    ProductId = ate.ProductId,
    ProductName = prod != null ? prod.ProductName : null,
    IsPosted = act.IsPosted,
    PostedDate = act.PostedDate,
    DebitCreditFlag = ate.DebitCreditFlag,
    Amount = (decimal)ate.Amount,
    Description = ate.Description,
    CurrencyUomId = ate.CurrencyUomId
};