// === COLUMN WIDTHS ===
ws.columns = [
    { width: 15 }, // Payment ID
    { width: 22 }, // Type
    { width: 15 }, // Order ID
    { width: 20 }, // Certificate
    { width: 30 }, // Project (NEW)
    { width: 28 }, // Cost Center (NEW)
    { width: 28 }, // From
    { width: 28 }, // To
    { width: 15 }, // Date
    { width: 15 }, // Status
    { width: 16 }, // Amount
    { width: 35 }  // Comments
];

ws.getColumn(11).numFmt = '#,##0.00'; // Amount is now column 11 (1-indexed)