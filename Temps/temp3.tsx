// In PartyFinancialHistoryExcel.tsx

// REFACTOR: Reverse balance coloring and final clarity text to match new balance sign
// - Green (>0): debit impact dominant → vendor owes us (لنا عند الطرف)
// - Red (<0): credit impact dominant → we owe vendor (علينا للطرف)
ledgerItems.forEach(item => {
    // ...
    const balanceCell = row.getCell(8);
    if (item.balance > 0) {
        balanceCell.font = { ...balanceCell.font, color: { argb: 'FF006400' }, bold: true }; // Green = vendor owes us
    } else if (item.balance < 0) {
        balanceCell.font = { ...balanceCell.font, color: { argb: 'FF8B0000' }, bold: true }; // Red = we owe vendor
    }
});

// Final Balance Row (unchanged logic, but colors now match reversed sign)
const finalBalance = ledgerItems[ledgerItems.length - 1]?.balance || 0;
// ...
if (finalBalance > 0) {
    finalRow.getCell(8).font = { color: { argb: 'FF006400' }, bold: true }; // Green
} else if (finalBalance < 0) {
    finalRow.getCell(8).font = { color: { argb: 'FF8B0000' }, bold: true }; // Red
}

// REFACTOR: Update clarity text direction and meaning for reversed balance
const clarityText = finalBalance > 0
    ? '← للطرف عندنا (دائنون لنا)'
    : finalBalance < 0
        ? '← علينا للطرف (مدينون له)'
        : 'لا يوجد رصيد متبقي';
worksheet.addRow(['', clarityText, '', '', '', '', '', Math.abs(finalBalance), '']);