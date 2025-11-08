// REFACTOR: Added all new disbursement payment types to outgoing filter
// Purpose: Include newly created payment types (from JSON records) in the UI filter
// Improves: Completeness — users can now select all valid outgoing payment types
// Context: These IDs were added via new PaymentType + PaymentTypeGlAccount records
//         All have PARENT_TYPE_ID = "DISBURSEMENT" and are expense-related

const PAYMENT_TYPE_FILTERS = {
    incoming: [
        "CUSTOMER_PAYMENT",
        "CUSTOMER_DEPOSIT",
        "INTEREST_RECEIPT",
        "GC_DEPOSIT",
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

        // Existing disbursement types
        "ADVANCE_TO_VENDOR_CONTRACTOR",
        "CONTRACTOR_INSTALLMENT",
        "PERMANENT_CUSTODY",
        "TEMP_ADVANCE",
        "VENDOR_INVOICE_PAYMENT",
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

        // === NEW PAYMENT TYPES ADDED BELOW ===
        // REFACTOR: Integrated new expense-based disbursement types
        // Purpose: Enable selection in NewPaymentOut form
        // Improves: UX consistency and data integrity
        // Context: All map to OPERATING_EXPENSE or OTHER_EXPENSE in GL
        "ADVERTISING_EXPENSES",
        "VEHICLE_FUEL",
        "ALLOWANCES_BONUSES",
        "TRANSPORTATION",
        "MAINTENANCE_REPAIR",
        "VEHICLE_OIL_CHANGE",
        "CLEANING_SUPPLIES",
        "BUFFET_HOSPITALITY",
        "HOSPITALITY_PR",
        "GOV_LICENSE_FEES",
        "PHOTOCOPIER_SUPPLIES",
        "BANK_FEES",
        "FINANCING_EXPENSES",
        "LOAN_INTEREST",
        "COMMUNICATIONS_INTERNET",
        "OFFICE_SUPPLIES",
    ],
};