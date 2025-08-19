using Application.Accounting.Services.Models;
using Application.Catalog.ProductStores;
using Application.Core;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Services;

public interface IAcctgMiscService
{
    Task<PartyAcctgPreference?> GetPartyAccountingPreferences(string organizationPartyId);
    Task<decimal> GetGlExchangeRateOfPurchaseInvoice(PaymentApplication paymentApplication);
    Task<decimal> GetGlExchangeRateOfOutgoingPayment(PaymentApplication paymentApplication);
    Task<PartyAcctgPreference> GetPartyAccountingPreference(string orderId);
    Task<bool> IsDebitAccount(string glAccountId);
    MidpointRounding GetMidpointRounding(string roundingMode);
    GlArithmeticSettings GetGlArithmeticSettingsInline();
    decimal CustomRound(decimal value, int decimals, string roundingMode);

    string GenerateGlAccountDiagram(IEnumerable<AcctgTransEntry> entries, string acctgTransTypeId,
        string debitCreditFlag);
}

public class AcctgMiscService : IAcctgMiscService
{
    private readonly DataContext _context;
    private readonly IProductStoreService _productStoreService;
    private readonly IUtilityService _utilityService;

    public AcctgMiscService(DataContext context, IUtilityService utilityService,
        IProductStoreService productStoreService)
    {
        _context = context;
        _utilityService = utilityService;
        _productStoreService = productStoreService;
    }

    public async Task<PartyAcctgPreference?> GetPartyAccountingPreferences(string organizationPartyId)
    {
        var partyAcctgPreference =
            await _context.PartyAcctgPreferences.FirstOrDefaultAsync(pap => pap.PartyId == organizationPartyId);

        return partyAcctgPreference;
    }

    public async Task<decimal> GetGlExchangeRateOfPurchaseInvoice(PaymentApplication paymentApplication)
    {
        var exchangeRate = 1M;

        // Retrieve the payment application's invoice ID
        var invoiceId = paymentApplication!.InvoiceId;

        // Get the origAmount and amount fields from accounting transactions where:
        // - glAccountTypeId is "ACCOUNTS_PAYABLE"
        // - debitCreditFlag is "C"
        // - acctgTransTypeId is "PURCHASE_INVOICE"
        // - invoiceId matches the payment application's invoice ID
        var amounts = await (from a in _context.AcctgTransEntries
            join b in _context.AcctgTrans on a.AcctgTransId equals b.AcctgTransId
            where a.GlAccountTypeId == "ACCOUNTS_PAYABLE" &&
                  a.DebitCreditFlag == "C" &&
                  b.AcctgTransTypeId == "PURCHASE_INVOICE" &&
                  b.InvoiceId == invoiceId
            select new { a.OrigAmount, a.Amount }).FirstOrDefaultAsync();

        // If no matching accounting transactions found, return default exchange rate (1)
        if (amounts == null) return exchangeRate;

        // Calculate exchange rate if origAmount and amount are not null and not zero
        if (amounts.OrigAmount != null && amounts.Amount != null &&
            amounts.OrigAmount != 0 && amounts.Amount != 0 &&
            amounts.Amount != amounts.OrigAmount)
            exchangeRate = (decimal)(amounts.Amount / amounts.OrigAmount);

        return exchangeRate;
    }

    public async Task<decimal> GetGlExchangeRateOfOutgoingPayment(PaymentApplication paymentApplication)
    {
        var exchangeRate = 1M;

        // Retrieve the payment application's payment ID
        var paymentId = paymentApplication.PaymentId;

        // Get the origAmount and amount fields from accounting transactions where:
        // - glAccountTypeId is "CURRENT_ASSET"
        // - debitCreditFlag is "C"
        // - acctgTransTypeId is "OUTGOING_PAYMENT"
        // - paymentId matches the payment application's payment ID
        var amounts = await (from a in _context.AcctgTransEntries
            join b in _context.AcctgTrans on a.AcctgTransId equals b.AcctgTransId
            where a.GlAccountTypeId == "CURRENT_ASSET" &&
                  a.DebitCreditFlag == "C" &&
                  b.AcctgTransTypeId == "OUTGOING_PAYMENT" &&
                  b.PaymentId == paymentId
            select new { a.OrigAmount, a.Amount }).FirstOrDefaultAsync();

        // If no matching accounting transactions found, return default exchange rate (1)
        if (amounts == null) return exchangeRate;

        // Calculate exchange rate if origAmount and amount are not null and not zero
        if (amounts.OrigAmount != null && amounts.Amount != null &&
            amounts.OrigAmount != 0 && amounts.Amount != 0 &&
            amounts.Amount != amounts.OrigAmount)
            exchangeRate = (decimal)(amounts.Amount / amounts.OrigAmount);

        return exchangeRate;
    }

    public async Task<PartyAcctgPreference> GetPartyAccountingPreference(string orderId)
    {
        // get orderRoleBillFromVendor
        var orderRoleBillFromVendor = await _utilityService.GetOrderRole(orderId, "BILL_FROM_VENDOR");
        // get productStore
        var productStore = await _productStoreService.GetProductStoreForLoggedInUser();

        // get partyAcctgPreference for partyIdTo based on product store pay to company
        var partyAcctgPreference = await _context.PartyAcctgPreferences.SingleOrDefaultAsync(x =>
            x.PartyId == orderRoleBillFromVendor.PartyId
            && x.PartyId == productStore.PayToPartyId);

        return partyAcctgPreference;
    }

    // Checks if a GL account is a debit account
    public async Task<bool> IsDebitAccount(string glAccountId)
    {
        return await IsAccountClass(glAccountId, "DEBIT");
    }

    // Checks if a GL account is of a specified GlAccountClass.glAccountClassId
    public async Task<bool> IsAccountClass(string glAccountId, string glAccountClassId)
    {
        var glAccount = await _context.GlAccounts
            .FirstOrDefaultAsync(gla => gla.GlAccountId == glAccountId);

        if (glAccount == null)
        {
            return false;
        }

        var glAccountClass = await _context.GlAccountClasses
            .FirstOrDefaultAsync(gac => gac.GlAccountClassId == glAccount.GlAccountClassId);

        if (glAccountClass == null)
        {
            throw new Exception($"Cannot find GlAccountClass for glAccountId {glAccountId}");
        }

        return await IsAccountClassClass(glAccountClass, glAccountClassId);
    }

    // Checks if a GL account class is of a specified GlAccountClass.glAccountClassId
    private async Task<bool> IsAccountClassClass(GlAccountClass glAccountClass, string parentGlAccountClassId)
    {
        if (glAccountClass == null) return false;

        // check current class against input classId
        if (parentGlAccountClassId == glAccountClass.GlAccountClassId)
        {
            return true;
        }

        // check parentClassId against inputClassId
        var parentClassId = glAccountClass.ParentClassId;
        if (parentClassId == null)
        {
            return false;
        }

        if (parentClassId == parentGlAccountClassId)
        {
            return true;
        }

        // otherwise, we have to go to the grandparent (recurse)
        var parentGlAccountClass = await _context.GlAccountClasses
            .FirstOrDefaultAsync(gac => gac.GlAccountClassId == parentClassId);

        return await IsAccountClassClass(parentGlAccountClass, parentGlAccountClassId);
    }

    public GlArithmeticSettings GetGlArithmeticSettingsInline()
    {
        // Set default properties
        decimal ledgerDecimals = 4; // Default value as specified
        string roundingMode = "HalfUp"; // Default value as specified

        return new GlArithmeticSettings
        {
            DecimalScale = ledgerDecimals,
            RoundingMode = roundingMode
        };
    }

    public MidpointRounding GetMidpointRounding(string roundingMode)
    {
        return roundingMode switch
        {
            "HalfUp" => MidpointRounding.AwayFromZero, // Equivalent to HALF_UP
            "HalfDown" => MidpointRounding.ToEven, // No direct equivalent; mapped to ToEven
            _ => MidpointRounding.ToEven // Default to ToEven
        };
    }

    public decimal CustomRound(decimal value, int decimals, string roundingMode)
    {
        decimal multiplier = (decimal)Math.Pow(10, decimals);
        decimal scaledValue = value * multiplier;
        decimal roundedValue;

        switch (roundingMode)
        {
            case "HalfUp":
                roundedValue = Math.Round(scaledValue, 0, MidpointRounding.AwayFromZero);
                break;
            case "HalfDown":
                // Implement HalfDown: round to nearest integer, but .5 rounds down
                if (scaledValue % 1 == 0.5m)
                {
                    roundedValue = Math.Floor(scaledValue);
                }
                else
                {
                    roundedValue = Math.Round(scaledValue, 0, MidpointRounding.ToEven);
                }

                break;
            default:
                roundedValue = Math.Round(scaledValue, 0, MidpointRounding.ToEven);
                break;
        }

        return roundedValue / multiplier;
    }

    public string GenerateGlAccountDiagram(IEnumerable<AcctgTransEntry> entries, string acctgTransTypeId,
        string debitCreditFlag)
    {
        var diagram = new System.Text.StringBuilder();
        diagram.AppendLine("flowchart LR");
        diagram.AppendLine("  Start((Start))");

        // For example, if the transaction type is ITEM_VARIANCE and there's an entry with a variance account:
        if (acctgTransTypeId == "ITEM_VARIANCE" && entries.Any(e => e.GlAccountTypeId == "VARIANCE"))
        {
            diagram.AppendLine("  Start --> ITEM_VARIANCE[ITEM_VARIANCE path]");
            diagram.AppendLine("  ITEM_VARIANCE --> End((End))");
            return diagram.ToString();
        }

        // If the transaction type is DEPRECIATION, check the debit/credit flag and which account is set:
        if (acctgTransTypeId == "DEPRECIATION")
        {
            diagram.AppendLine("  Start --> DEPRECIATION[DEPRECIATION path]");
            if (debitCreditFlag == "C")
            {
                // Check if an entry uses the AccDepGlAccountId
                if (entries.Any(e => !string.IsNullOrEmpty(e.GlAccountId) && e.DebitCreditFlag == "C"))
                {
                    diagram.AppendLine("  DEPRECIATION --> ACCDEP[Use AccDepGlAccountId]");
                }
            }
            else if (debitCreditFlag == "D")
            {
                // Check for DepGlAccountId usage
                if (entries.Any(e => !string.IsNullOrEmpty(e.GlAccountId) && e.DebitCreditFlag == "D"))
                {
                    diagram.AppendLine("  DEPRECIATION --> DEP[Use DepGlAccountId]");
                }
            }

            diagram.AppendLine("  ... --> End((End))");
            return diagram.ToString();
        }

        // Check for party-specific mapping: if an entry indicates a party-specific GL account:
        if (!string.IsNullOrEmpty(acctgTransTypeId) && entries.Any(e => e.GlAccountTypeId == "PARTY_SPECIFIC"))
        {
            diagram.AppendLine("  Start --> PARTY_SPECIFIC[Party-specific mapping]");
            diagram.AppendLine("  PARTY_SPECIFIC --> End((End))");
            return diagram.ToString();
        }

        // Check for payment-specific conditions:
        if ((acctgTransTypeId == "OUTGOING_PAYMENT" && debitCreditFlag == "C") ||
            (acctgTransTypeId == "INCOMING_PAYMENT" && debitCreditFlag == "D"))
        {
            diagram.AppendLine("  Start --> PAYMENT[Payment conditions]");
            // Optionally, add more detail if you can determine if a credit card, etc. was used.
            diagram.AppendLine("  PAYMENT --> End((End))");
            return diagram.ToString();
        }

        // Fallback/default branch
        diagram.AppendLine("  Start --> DEFAULT[Default GL Account]");
        diagram.AppendLine("  DEFAULT --> End((End))");

        return diagram.ToString();
    }
}