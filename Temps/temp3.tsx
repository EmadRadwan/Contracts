// ─────────────────────────────────────────────
// 1. Common columns (before amount)
const commonStartColumns = [
    {
        field: "paymentId",
        title: getTranslatedLabel(`${localizationKey}.paymentId`, "Payment Number"),
        cell: PaymentDescriptionCell,
        width: 150,
        locked: !show,
    },
    {
        field: "paymentTypeDescription",
        title: getTranslatedLabel(`${localizationKey}.paymentType`, "Payment Type"),
        width: 150,
    },
    {
        field: "amount",
        title: getTranslatedLabel(`${localizationKey}.amount`, "Amount"),
        width: 130,
        filter: "numeric",
    },
];

// ─────────────────────────────────────────────
// 2. Outgoing PRIORITY columns (must come after amount)
const outgoingPriorityColumns = [
    {
        field: "partyIdToName",
        title: getTranslatedLabel(`${localizationKey}.to`, "To Party"),
        width: 180,
    },
    {
        field: "effectiveDate",
        title: getTranslatedLabel(`${localizationKey}.date`, "Payment Date"),
        width: 150,
        format: "{0: dd/MM/yyyy}",
        filter: "date",
    },
    {
        field: "statusDescription",
        title: getTranslatedLabel(`${localizationKey}.status`, "Status"),
        width: 120,
    },
    {
        field: "paymentRefNum",
        title: getTranslatedLabel(`${localizationKey}.paymentRefNum`, "Ref Number"),
        width: 140,
    },
];

// ─────────────────────────────────────────────
// 3. Common middle columns (after amount / priority)
const commonMiddleColumns = [
    {
        field: "paymentMethodTypeDescription",
        title: getTranslatedLabel(`${localizationKey}.paymentMethodTypeDescription`, "Payment Method Type"),
        width: 150,
    },
    {
        field: "isBankTransfer",
        title: getTranslatedLabel(`${localizationKey}.isBankTransfer`, "Bank Transfer"),
        width: 120,
        filter: "boolean",
        cell: BankTransferCell,
    },
    {
        field: "chequeNumber",
        title: getTranslatedLabel(`${localizationKey}.chequeNumber`, "Cheque Number"),
        width: 150,
    },
    {
        field: "daysUntilDue",
        title: getTranslatedLabel(`${localizationKey}.dueStatus`, "Due Status"),
        width: 260,
        cell: DueStatusCell,
    },
];

// ─────────────────────────────────────────────
// 4. Incoming-only columns
const incomingColumns = [
    {
        field: "buildingNumber",
        title: getTranslatedLabel(`${localizationKey}.buildingNumber`, "Building Number"),
        width: 140,
    },
    {
        field: "productId",
        title: getTranslatedLabel(`${localizationKey}.productId`, "Product ID"),
        width: 130,
    },
    {
        field: "partyIdFromName",
        title: getTranslatedLabel(`${localizationKey}.from`, "From Party"),
        width: 180,
    },
    {
        field: "effectiveDate",
        title: getTranslatedLabel(`${localizationKey}.date`, "Payment Date"),
        width: 150,
        format: "{0: dd/MM/yyyy}",
        filter: "date",
    },
    {
        field: "statusDescription",
        title: getTranslatedLabel(`${localizationKey}.status`, "Status"),
        width: 120,
    },
    {
        field: "comments",
        title: getTranslatedLabel(`${localizationKey}.comments`, "Comments"),
        width: 280,
    },
    {
        field: "projectName",
        title: getTranslatedLabel(`${localizationKey}.projectName`, "Project / Comments"),
        width: 180,
    },
    {
        field: "costCenterDescription",
        title: getTranslatedLabel(`${localizationKey}.costCenterDescription`, "Cost Center"),
        width: 160,
    },
    {
        field: "paymentRefNum",
        title: getTranslatedLabel(`${localizationKey}.paymentRefNum`, "Ref Number"),
        width: 140,
    },
];

// ─────────────────────────────────────────────
// 5. Outgoing remaining columns (EXCLUDING priority ones)
const outgoingOtherColumns = [
    {
        field: "orderId",
        title: getTranslatedLabel(`${localizationKey}.orderId`, "Order ID"),
        width: 140,
    },
    {
        field: "certificateNumber",
        title: getTranslatedLabel(`${localizationKey}.certificateNumber`, "Certificate No"),
        width: 150,
    },
    {
        field: "partyIdFromName",
        title: getTranslatedLabel(`${localizationKey}.from`, "From Party"),
        width: 180,
    },
    {
        field: "projectName",
        title: getTranslatedLabel(`${localizationKey}.projectName`, "Project"),
        width: 180,
    },
    {
        field: "costCenterDescription",
        title: getTranslatedLabel(`${localizationKey}.costCenterDescription`, "Cost Center"),
        width: 160,
    },
    {
        field: "salesRequestId",
        title: getTranslatedLabel(`${localizationKey}.salesRequestId`, "Sales Request ID"),
        width: 150,
    },
    {
        field: "comments",
        title: getTranslatedLabel(`${localizationKey}.comments`, "Comments"),
        width: 280,
    },
];

// ─────────────────────────────────────────────
// 6. Final columns (always last)
const finalColumns = [
    {
        field: "approvedByPartyName",
        title: getTranslatedLabel(`${localizationKey}.approvedByPartyName`, "Approved By"),
        width: 150,
    },
    {
        field: "createdByPartyName",
        title: getTranslatedLabel(`${localizationKey}.createdByPartyName`, "Created By"),
        width: 150,
    },
    {
        title: getTranslatedLabel(`${localizationKey}.actions`, "Actions"),
        width: 220,
        cell: ActionsCell,
    },
];

// ─────────────────────────────────────────────
// 7. Assemble final gridColumns

let gridColumns: any[] = [];

if (isOutgoing) {
    gridColumns = [
        ...commonStartColumns,
        ...outgoingPriorityColumns,   // 👈 inserted right after amount
        ...commonMiddleColumns,
        ...outgoingOtherColumns,
        ...finalColumns,
    ];
} else {
    gridColumns = [
        ...commonStartColumns,
        ...commonMiddleColumns,
        ...incomingColumns,
        ...finalColumns,
    ];
}