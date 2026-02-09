// ... rest of your imports and component code ...

export default function PaymentsList({ paymentType }: PaymentsListProps) {
    // ... existing hooks, states, handlers ...

    const isIncoming = paymentType === "incoming";
    // const isOutgoing = paymentType === "outgoing";  // you can keep if needed

    // Define columns as array
    const gridColumns = [
        // ── Always visible (common) ──
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
            field: "paymentMethodTypeDescription",
            title: getTranslatedLabel(`${localizationKey}.paymentMethodTypeDescription`, "Payment Method Type"),
            width: 150,
        },

        // ── Incoming-only columns ──
        ...(isIncoming
            ? [
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
                    field: "amount",
                    title: getTranslatedLabel(`${localizationKey}.amount`, "Amount"),
                    width: 130,
                    filter: "numeric",
                },
                {
                    field: "comments",
                    title: getTranslatedLabel(`${localizationKey}.comments`, "Comments"),
                    width: 180,
                },
                {
                    field: "projectName",
                    title: getTranslatedLabel(`${localizationKey}.projectName`, "Project / Comments"),
                    width: 180,
                },
                // extra ones you added in your last version
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
                    field: "paymentRefNum",
                    title: getTranslatedLabel(`${localizationKey}.paymentRefNum`, "Ref Number"),
                    width: 140,
                },
            ]
            : []),

        // ── Outgoing-only columns ──
        ...(isIncoming
            ? [] // nothing extra for incoming beyond what's above
            : [
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
                {
                    field: "amount",
                    title: getTranslatedLabel(`${localizationKey}.amount`, "Amount"),
                    width: 130,
                    filter: "numeric",
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
                    width: 180,
                },
            ]),

        // ── Always last ──
        {
            title: getTranslatedLabel(`${localizationKey}.actions`, "Actions"),
            width: 220,
            cell: ActionsCell,
            locked: true,
        },
    ];

    // ── Now render ──
    return (
        <>
            {/* ... AccountingMenu, Paper, etc ... */}

            <KendoGrid
                style={{ flex: 1 }}
                data={payments ? payments : { data: [], total: 0 }}
                resizable={true}
                filterable={true}
                sortable={true}
                pageable={true}
                {...dataState}
                onDataStateChange={dataStateChange}
            >
                <GridToolbar>
                    {/* ... your buttons ... */}
                </GridToolbar>

                {gridColumns.map((col, index) => (
                    <Column key={index} {...col} />
                ))}
            </KendoGrid>

            {/* ... Dialog, ModalContainer, Loading ... */}
        </>
    );
}