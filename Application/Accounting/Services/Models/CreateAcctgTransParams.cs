namespace Application.Accounting.Services.Models
{
    public class CreateAcctgTransParams
    {
        // Primary Transaction Fields
        public string? AcctgTransId { get; set; }              // ACCTG_TRANS_ID
        public string? GlFiscalTypeId { get; set; }            // GL_FISCAL_TYPE_ID
        public string? AcctgTransTypeId { get; set; }          // ACCTG_TRANS_TYPE_ID
        public string? IsPosted { get; set; }                  // IS_POSTED
        public string? InvoiceId { get; set; }                 // INVOICE_ID
        public string? PaymentId { get; set; }                 // PAYMENT_ID
        public string? ShipmentId { get; set; }                // SHIPMENT_ID
        public string? WorkEffortId { get; set; }              // WORK_EFFORT_ID
        public string? PartyId { get; set; }                   // PARTY_ID
        public string? RoleTypeId { get; set; }                // ROLE_TYPE_ID
        public string? Description { get; set; }               // DESCRIPTION
        public DateTime? TransactionDate { get; set; }         // TRANSACTION_DATE
        public DateTime? PostedDate { get; set; }              // POSTED_DATE

        // Additional Transaction Details
        public DateTime? ScheduledPostingDate { get; set; }    // SCHEDULED_POSTING_DATE
        public string? GlJournalId { get; set; }               // GL_JOURNAL_ID
        public string? VoucherRef { get; set; }                // VOUCHER_REF
        public DateTime? VoucherDate { get; set; }             // VOUCHER_DATE
        public string? GroupStatusId { get; set; }             // GROUP_STATUS_ID
        public string? FixedAssetId { get; set; }              // FIXED_ASSET_ID
        public string? InventoryItemId { get; set; }           // INVENTORY_ITEM_ID
        public string? PhysicalInventoryId { get; set; }       // PHYSICAL_INVENTORY_ID
        public string? FinAccountTransId { get; set; }         // FIN_ACCOUNT_TRANS_ID
        public string? ReceiptId { get; set; }                 // RECEIPT_ID
        public string? TheirAcctgTransId { get; set; }         // THEIR_ACCTG_TRANS_ID

        // Audit Fields
        public DateTime? CreatedDate { get; set; }             // CREATED_DATE
        public string? CreatedByUserLogin { get; set; }        // CREATED_BY_USER_LOGIN
        public DateTime? LastModifiedDate { get; set; }        // LAST_MODIFIED_DATE
        public string? LastModifiedByUserLogin { get; set; }   // LAST_MODIFIED_BY_USER_LOGIN
        public DateTime? LastUpdatedStamp { get; set; }        // LAST_UPDATED_STAMP
        public DateTime? LastUpdatedTxStamp { get; set; }      // LAST_UPDATED_TX_STAMP
        public DateTime? CreatedStamp { get; set; }            // CREATED_STAMP
        public DateTime? CreatedTxStamp { get; set; }          // CREATED_TX_STAMP
    }
}
