<DialogContentText
    id="reset-invoice-dialog-description"
    dir={isArabic ? "rtl" : "ltr"}
    sx={{
        textAlign: isArabic ? "right" : "left",
        whiteSpace: "pre-line"
    }}
>
    {getTranslatedLabel(
        `${localizationKey}.reset.dialogMessage`,
        `Are you sure you want to reset invoice {invoiceId}?

This will:
• Return status to 'In Process'
• Delete all payment applications
• Delete related accounting transactions

This action cannot be undone.`
    ).replace("{invoiceId}", invoice?.invoiceId || "")}
</DialogContentText>