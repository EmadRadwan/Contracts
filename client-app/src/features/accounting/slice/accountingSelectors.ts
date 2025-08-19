import {createSelector} from "@reduxjs/toolkit";
import {acctgTransEntriesEntities} from "./accountingSharedUiSlice";

export const nonDeletedAcctgTransEntriesSelector = createSelector(acctgTransEntriesEntities, (acctgTransEntries) => {
    return Object.values(acctgTransEntries)
        .filter((acctgTransEntry) => {
            if (!acctgTransEntry!.isAcctgTransEntryDeleted) return acctgTransEntry;
        });
});