// Add near the existing Excel button
const { data: subLedgerData, isFetching: isSubLedgerFetching } = useGetPartySubLedgerQuery(partyId, {
    skip: !partyId,
});

<Button
    variant="outlined"
    color="primary"
    disabled={isSubLedgerFetching || !subLedgerData?.SubLedgers?.length}
    onClick={() => {
        // You can also open a modal to preview, but for now just trigger download
    }}
>
    تصدير دفتر الأستاذ الفرعي (Excel)
</Button>

// Then pass to new component:
{subLedgerData && subLedgerData.SubLedgers.length > 0 && (
    <PartySubLedgerExcel
        party={{ partyId, partyName: displayName }}
        subLedgers={subLedgerData.SubLedgers}
        getTranslatedLabel={getTranslatedLabel}
        isFetching={isSubLedgerFetching}
        currency={subLedgerData.CurrencyUomId}
    />
)}

{subLedgerData && subLedgerData.SubLedgers.length > 0 && (
    <PartySubLedgerExcel
        party={{ partyId, partyName: displayName }}
        subLedgers={subLedgerData.SubLedgers}
        getTranslatedLabel={getTranslatedLabel}
        isFetching={isSubLedgerFetching}
        currency={subLedgerData.CurrencyUomId}
    />
)}