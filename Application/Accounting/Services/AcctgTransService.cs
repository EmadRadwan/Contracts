using API.Controllers.Accounting.Transactions;
using Application.Accounting.Services.Models;
using Application.Core;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Services;

public interface IAcctgTransService
{
    Task<string> CreateAcctgTrans(CreateAcctgTransParams parameters);
    Task<AcctgTransEntry> CreateAcctgTransEntry(AcctgTransEntry acctgTransEntry);
    Task<List<string>> UpdateAcctgTrans(AcctgTran parameters);
    Task<AcctgTransEntryDto> UpdateAcctgTransEntry(AcctgTransEntry acctgTransEntry);
    Task DeleteAcctgTransEntry(string acctgTransId, string acctgTransEntrySeqId);
}

public class AcctgTransService : IAcctgTransService
{
    private readonly DataContext _context;
    private readonly IUtilityService _utilityService;

    public AcctgTransService(DataContext context, IUtilityService utilityService)
    {
        _context = context;
        _utilityService = utilityService;
    }

    public async Task<string> CreateAcctgTrans(CreateAcctgTransParams parameters)
    {
        // Create an accounting transaction
        var stamp = DateTime.UtcNow;
        var newAcctgTransSequence = await _utilityService.GetNextSequence("AcctgTrans");


        var acctgTran = new AcctgTran
        {
            AcctgTransId = newAcctgTransSequence,
            AcctgTransTypeId = parameters.AcctgTransTypeId,
            TransactionDate = parameters.TransactionDate,
            IsPosted = parameters.IsPosted,
            PostedDate = parameters.PostedDate,
            GlFiscalTypeId = parameters.GlFiscalTypeId,
            InvoiceId = parameters.InvoiceId,
            PaymentId = parameters.PaymentId,
            PartyId = parameters.PartyId,
            RoleTypeId = parameters.RoleTypeId,
            ShipmentId = parameters.ShipmentId,
            WorkEffortId = parameters.WorkEffortId,
            Description = parameters.Description,

            // Additional fields from the record
            ScheduledPostingDate = parameters.ScheduledPostingDate,
            GlJournalId = parameters.GlJournalId,
            VoucherRef = parameters.VoucherRef,
            VoucherDate = parameters.VoucherDate,
            GroupStatusId = parameters.GroupStatusId,
            FixedAssetId = parameters.FixedAssetId,
            InventoryItemId = parameters.InventoryItemId,
            PhysicalInventoryId = parameters.PhysicalInventoryId,
            FinAccountTransId = parameters.FinAccountTransId,
            ReceiptId = parameters.ReceiptId,
            TheirAcctgTransId = parameters.TheirAcctgTransId,

            // Audit Fields
            CreatedDate = stamp,
            CreatedByUserLogin = null,
            LastModifiedDate = stamp,
            LastModifiedByUserLogin = null,
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp,
            LastUpdatedTxStamp = stamp,
            CreatedTxStamp = stamp
        };

        _context.AcctgTrans.Add(acctgTran);

        return newAcctgTransSequence;
    }

    public async Task<AcctgTransEntry> CreateAcctgTransEntry(AcctgTransEntry acctgTransEntry)
    {
        // get base currency for logged in user
        var baseCurrency = await _utilityService.GetBaseCurrencyForLoggedInUser();

        var entry = new AcctgTransEntry
        {
            AcctgTransEntrySeqId = acctgTransEntry.AcctgTransEntrySeqId,
            AcctgTransId = acctgTransEntry.AcctgTransId,
            AcctgTransEntryTypeId = acctgTransEntry.AcctgTransEntryTypeId,
            PartyId = acctgTransEntry.PartyId,
            RoleTypeId = acctgTransEntry.RoleTypeId,
            ProductId = acctgTransEntry.ProductId,
            GlAccountTypeId = acctgTransEntry.GlAccountTypeId,
            GlAccountId = acctgTransEntry.GlAccountId,
            OrganizationPartyId = acctgTransEntry.OrganizationPartyId,
            Amount = acctgTransEntry.Amount,
            OrigAmount = acctgTransEntry.OrigAmount,
            OrigCurrencyUomId = acctgTransEntry.OrigCurrencyUomId ?? baseCurrency,
            CurrencyUomId = acctgTransEntry.CurrencyUomId ?? baseCurrency,
            DebitCreditFlag = acctgTransEntry.DebitCreditFlag,
            ReconcileStatusId = acctgTransEntry.ReconcileStatusId,
            Description = acctgTransEntry.Description,
            CreatedStamp = acctgTransEntry.CreatedStamp,
            LastUpdatedStamp = acctgTransEntry.LastUpdatedStamp
        };
        _context.AcctgTransEntries.Add(entry);

        return entry;
    }

    public async Task<AcctgTransEntryDto> UpdateAcctgTransEntry(AcctgTransEntry acctgTransEntry)
    {
        // Retrieve the existing AcctgTransEntry by primary key
        var existingEntry = await _context.AcctgTransEntries
            .FirstOrDefaultAsync(e => e.AcctgTransId == acctgTransEntry.AcctgTransId &&
                                      e.AcctgTransEntrySeqId == acctgTransEntry.AcctgTransEntrySeqId);

        if (existingEntry == null)
        {
            throw new Exception("Accounting transaction entry not found.");
        }

        // Check if the associated AcctgTrans is posted
        var acctgTrans = await _context.AcctgTrans
            .FirstOrDefaultAsync(t => t.AcctgTransId == acctgTransEntry.AcctgTransId);

        if (acctgTrans?.IsPosted == "Y")
        {
            // Only allow reconcileStatusId change for posted entries
            if (existingEntry.AcctgTransEntryTypeId != acctgTransEntry.AcctgTransEntryTypeId ||
                existingEntry.PartyId != acctgTransEntry.PartyId ||
                existingEntry.RoleTypeId != acctgTransEntry.RoleTypeId ||
                existingEntry.ProductId != acctgTransEntry.ProductId ||
                existingEntry.GlAccountTypeId != acctgTransEntry.GlAccountTypeId ||
                existingEntry.GlAccountId != acctgTransEntry.GlAccountId ||
                existingEntry.OrganizationPartyId != acctgTransEntry.OrganizationPartyId ||
                existingEntry.Amount != acctgTransEntry.Amount ||
                existingEntry.OrigAmount != acctgTransEntry.OrigAmount ||
                existingEntry.OrigCurrencyUomId != acctgTransEntry.OrigCurrencyUomId ||
                existingEntry.CurrencyUomId != acctgTransEntry.CurrencyUomId ||
                existingEntry.DebitCreditFlag != acctgTransEntry.DebitCreditFlag ||
                existingEntry.Description != acctgTransEntry.Description)
            {
                throw new Exception("Cannot update non-status fields of a posted accounting transaction entry.");
            }
        }

        // Update non-primary key fields
        existingEntry.AcctgTransEntryTypeId =
            acctgTransEntry.AcctgTransEntryTypeId ?? existingEntry.AcctgTransEntryTypeId;
        existingEntry.PartyId = acctgTransEntry.PartyId ?? existingEntry.PartyId;
        existingEntry.RoleTypeId = acctgTransEntry.RoleTypeId ?? existingEntry.RoleTypeId;
        existingEntry.ProductId = acctgTransEntry.ProductId ?? existingEntry.ProductId;
        existingEntry.GlAccountTypeId = acctgTransEntry.GlAccountTypeId ?? existingEntry.GlAccountTypeId;
        existingEntry.GlAccountId = acctgTransEntry.GlAccountId ?? existingEntry.GlAccountId;
        existingEntry.OrganizationPartyId = acctgTransEntry.OrganizationPartyId ?? existingEntry.OrganizationPartyId;
        existingEntry.Amount = acctgTransEntry.Amount ?? existingEntry.Amount;
        existingEntry.OrigAmount = acctgTransEntry.OrigAmount ?? existingEntry.OrigAmount;
        existingEntry.OrigCurrencyUomId = acctgTransEntry.OrigCurrencyUomId ?? existingEntry.OrigCurrencyUomId;
        existingEntry.CurrencyUomId = acctgTransEntry.CurrencyUomId ?? existingEntry.CurrencyUomId;
        existingEntry.DebitCreditFlag = acctgTransEntry.DebitCreditFlag ?? existingEntry.DebitCreditFlag;
        existingEntry.ReconcileStatusId = acctgTransEntry.ReconcileStatusId ?? existingEntry.ReconcileStatusId;
        existingEntry.Description = acctgTransEntry.Description ?? existingEntry.Description;
        existingEntry.LastUpdatedStamp = DateTime.UtcNow;

        // Update AcctgTrans last modified info
        if (acctgTrans != null)
        {
            acctgTrans.LastUpdatedStamp = DateTime.UtcNow;
        }

        return new AcctgTransEntryDto
        {
            AcctgTransId = existingEntry.AcctgTransId,
            AcctgTransEntrySeqId = existingEntry.AcctgTransEntrySeqId,
            AcctgTransEntryTypeId = existingEntry.AcctgTransEntryTypeId,
            PartyId = existingEntry.PartyId,
            RoleTypeId = existingEntry.RoleTypeId,
            ProductId = existingEntry.ProductId,
            GlAccountTypeId = existingEntry.GlAccountTypeId,
            GlAccountId = existingEntry.GlAccountId,
            OrganizationPartyId = existingEntry.OrganizationPartyId,
            Amount = existingEntry.Amount,
            OrigAmount = existingEntry.OrigAmount,
            OrigCurrencyUomId = existingEntry.OrigCurrencyUomId,
            CurrencyUomId = existingEntry.CurrencyUomId,
            DebitCreditFlag = existingEntry.DebitCreditFlag,
            ReconcileStatusId = existingEntry.ReconcileStatusId,
            Description = existingEntry.Description,
        };
    }

    public async Task<List<string>> UpdateAcctgTrans(AcctgTran parameters)
    {
        var messages = new List<string>();

        // 1) <entity-one entity-name="AcctgTrans" value-field="lookedUpValue"/>
        //    The PK is included in parameters (acctgTransId) as per <auto-attributes include="pk" ...>
        var lookedUpValue = await _context.AcctgTrans
            .FindAsync(parameters.AcctgTransId);

        if (lookedUpValue == null)
        {
            messages.Add($"AcctgTrans with ID {parameters.AcctgTransId} not found.");
            return messages;
        }

        // 2) <if-compare field="lookedUpValue.isPosted" operator="equals" value="Y">
        if (lookedUpValue.IsPosted == "Y")
        {
            // <add-error><fail-property ... AccountingTransactionHasBeenAlreadyPosted/>
            messages.Add("AccountingTransactionHasBeenAlreadyPosted");
            return messages; // <check-errors/>
        }

        // 3) <set-nonpk-fields map="parameters" value-field="lookedUpValue"/>
        //    We copy non-PK fields from our params object to `lookedUpValue`.
        //    In Ofbiz, <auto-attributes include="nonpk" mode="IN" optional="true"/>
        //    means we handle whichever fields are not PK.
        //    We'll do it field-by-field (or reflection-based if you prefer).
        if (parameters.AcctgTransTypeId != null)
            lookedUpValue.AcctgTransTypeId = parameters.AcctgTransTypeId;

        if (parameters.GlFiscalTypeId != null)
            lookedUpValue.GlFiscalTypeId = parameters.GlFiscalTypeId;

        if (parameters.TransactionDate != null)
            lookedUpValue.TransactionDate = parameters.TransactionDate;

        if (parameters.PostedDate != null)
            lookedUpValue.PostedDate = parameters.PostedDate;

        if (parameters.IsPosted != null)
            lookedUpValue.IsPosted = parameters.IsPosted;

        if (parameters.GlJournalId != null)
            lookedUpValue.GlJournalId = parameters.GlJournalId;

        if (parameters.PartyId != null)
            lookedUpValue.PartyId = parameters.PartyId;

        if (parameters.RoleTypeId != null)
            lookedUpValue.RoleTypeId = parameters.RoleTypeId;

        if (parameters.InvoiceId != null)
            lookedUpValue.InvoiceId = parameters.InvoiceId;

        if (parameters.PaymentId != null)
            lookedUpValue.PaymentId = parameters.PaymentId;

        if (parameters.ShipmentId != null)
            lookedUpValue.ShipmentId = parameters.ShipmentId;

        if (parameters.WorkEffortId != null)
            lookedUpValue.WorkEffortId = parameters.WorkEffortId;
        
        if (parameters.Description != null)
            lookedUpValue.Description = parameters.Description;

        return messages; // If no error, messages is empty => success
    }
    
    public async Task DeleteAcctgTransEntry(string acctgTransId, string acctgTransEntrySeqId)
    {
        // Retrieve the associated AcctgTrans
        var acctgTrans = await _context.AcctgTrans
            .FirstOrDefaultAsync(t => t.AcctgTransId == acctgTransId);

        if (acctgTrans?.IsPosted == "Y")
        {
            throw new Exception("Cannot delete an entry from a posted accounting transaction.");
        }

        // Retrieve the AcctgTransEntry by primary key
        var entry = await _context.AcctgTransEntries
            .FirstOrDefaultAsync(e => e.AcctgTransId == acctgTransId &&
                                      e.AcctgTransEntrySeqId == acctgTransEntrySeqId);

        if (entry == null)
        {
            throw new Exception("Accounting transaction entry not found.");
        }

        // Remove the entry
        _context.AcctgTransEntries.Remove(entry);

        // Update AcctgTrans last modified info
        if (acctgTrans != null)
        {
            acctgTrans.LastUpdatedStamp = DateTime.UtcNow;
        }
    }

}