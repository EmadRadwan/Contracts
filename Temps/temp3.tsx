data.data.forEach((payment: any) => {
    const row = ws.addRow([
        utils.rtlEmbed(utils.safeString(payment.paymentId)),                    // A
        utils.rtlEmbed(utils.safeString(payment.paymentRefNum ?? '')),         // B
        utils.rtlEmbed(utils.safeString(payment.paymentTypeDescription)),      // C
        utils.rtlEmbed(utils.safeString(payment.dueStatusArabic ?? '')),       // D ← Added
        utils.rtlEmbed(utils.safeString(payment.orderId ?? '')),               // E
        utils.rtlEmbed(utils.safeString(payment.productId ?? '')),             // F
        utils.rtlEmbed(utils.safeString(payment.buildingNumber ?? '')),        // G
        utils.rtlEmbed(utils.safeString(payment.certificateNumber ?? '')),     // H
        utils.rtlEmbed(utils.safeString(payment.projectName ?? '')),           // I
        utils.rtlEmbed(utils.safeString(payment.costCenterDescription ?? '')), // J
        utils.rtlEmbed(utils.safeString(payment.partyIdFromName)),             // K
        utils.rtlEmbed(utils.safeString(payment.partyIdToName)),               // L
        utils.formatDate(payment.effectiveDate),                               // M
        utils.rtlEmbed(utils.safeString(payment.statusDescription)),           // N
        utils.formatNumber(payment.amount),                                    // O
        utils.rtlEmbed(utils.safeString(payment.comments ?? '')),              // P
    ]);

    row.font = { name: 'Amiri', size: 10 };
    row.alignment = { horizontal: 'right', wrapText: true };
    row.height = 22;
});