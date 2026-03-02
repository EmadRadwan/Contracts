<Grid container item spacing={2} sx={{
    display: 'flex',
    flexDirection: 'row',
    justifyContent: 'flex-start',
    alignItems: 'center',          // ← add this
    mt: 2
}}>
    {/* Submit / Update button - only when editable */}
    {(editMode === 1 || (editMode === 2 && certificate?.currentStatusId !== "WEPR_APPROVED")) && (
        <Grid item>
            <Button type="submit" variant="contained" ... >
            {getTranslatedLabel(...)}
        </Button>
        </Grid>
        )}

    {/* Excel + Cancel – always show together when certificate exists */}
    {(editMode === 2 || editMode === 3) && certificate && (
        <>
        <Grid item>
        <MultiPaymentCertificateExcel
        companyName={companyName}
      certificate={certificate}
      items={items}
      transactions={acctTransEntryData || []}
      getTranslatedLabel={getTranslatedLabel}
      isFetching={isFetchingTransactions}
      language={language}
/>
</Grid>
<Grid item>
    <Button
        onClick={handleCancelForm}
        color="error"
        variant="contained"
        disabled={apiLoading}
    >
        {getTranslatedLabel("general.cancel", "Cancel")}
    </Button>
</Grid>
</>
)}

{/* If you still want Cancel in create mode – keep separate or merge logic */}
</Grid>