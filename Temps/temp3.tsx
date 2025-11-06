// ... (previous imports remain unchanged)

const PAYMENT_TYPE_FILTERS = {
    incoming: [
        "CUSTOMER_PAYMENT",
        "CUSTOMER_DEPOSIT",
        "INTEREST_RECEIPT",
        "GC_DEPOSIT",
        // New incoming types that exist in the system
        "RECEIPT_ADVANCE_PAYMENT",
        "RECEIPT_CHECK_REPLACEMENT",
        "RECEIPT_DUE_INSTALLMENT",
        "RECEIPT_MAINTENANCE_AMOUNT",
        "RECEIPT_PARTIAL_PAYMENT",
        "RECEIPT_RETURNED_CHECK",
    ],
    outgoing: [
        "TAX_PAYMENT",
        "SALES_TAX_PAYMENT",
        "PAYROLL_TAX_PAYMENT",
        "INCOME_TAX_PAYMENT" as const,
        "VENDOR_PAYMENT",
        "VENDOR_PREPAY",
        "PAY_CHECK",
        "PAYROL_PAYMENT",
        "CUSTOMER_REFUND",
        "GC_WITHDRAWAL",
        "COMMISSION_PAYMENT",
        // New outgoing types that exist in the system
        "ADVANCE_TO_VENDOR_CONTRACTOR",
        "CONTRACTOR_INSTALLMENT",
        "PERMANENT_CUSTODY",
        "TEMP_ADVANCE",
        "VENDOR_INVOICE_PAYMENT",
        // REFACTOR: Added newly introduced disbursement types from the provided list
        // Purpose: Expand the outgoing filter to include all valid disbursement-based payment types
        // Improves: Completeness — ensures users can select any active system payment type
        // Context: These IDs were confirmed to have PARENT_TYPE_ID = "DISBURSEMENT"
        "CHECK_REPLACEMENT",
        "DEBTORS_ADVANCE",
        "DUE_INSTALLMENT",
        "EMPLOYEE_ADVANCE",
        "EQUIPMENT_EXPENSES",
        "LABOR_WAGES",
        "LAND_PURCHASE",
        "MATERIAL_PURCHASE",
        "MISC_EXPENSES",
        "PARTIAL_PAYMENT",
    ],
};

// ... (rest of the component remains unchanged)