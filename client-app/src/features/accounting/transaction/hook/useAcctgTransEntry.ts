import React, {useState} from "react";
import {useSelector, useDispatch} from "react-redux";
import {toast} from "react-toastify";
import {
    acctTransApi, useCreateAcctgTransEntryMutation,
    useUpdateAcctgTransEntryMutation
} from "../../../../app/store/apis";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import {nonDeletedAcctgTransEntriesSelector} from "../../slice/accountingSelectors";

interface AcctgTransEntry {
    acctgTransId: string;
    acctgTransEntrySeqId?: string;
    organizationPartyId: string;
    glAccountTypeId: string;
    glAccountId: string;
    debitCreditFlag: string;
    partyId?: string;
    origAmount: number;
    origCurrencyUomId: string;
    purposeEnumId?: string;
    voucherRef?: string;
    productId?: string;
    reconcileStatusId: string;
    settlementTermId?: string;
    isSummary?: string;
    description?: string;
}

type UseAcctgTransEntryProps = {
    selectedMenuItem: string;
    formRef: any;
    editMode: number;
    selectedEntry: AcctgTransEntry | undefined;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
};

const useAcctgTransEntry = ({
                                selectedMenuItem,
                                formRef,
                                editMode,
                                selectedEntry,
                                setIsLoading,
                            }: UseAcctgTransEntryProps) => {
    const [entry, setEntry] = useState<AcctgTransEntry | undefined>(selectedEntry);
    const [formEditMode, setFormEditMode] = useState(editMode);
    const dispatch = useAppDispatch();
    const existingEntries = useAppSelector(nonDeletedAcctgTransEntriesSelector);

    const generateUniqueSeqId = (acctgTransId: string): string => {
        // Filter entries for the given acctgTransId
        const relevantEntries = existingEntries.filter(
            (entry) => entry.acctgTransId === acctgTransId
        );
        // Extract numeric parts of acctgTransEntrySeqId and find the maximum
        const maxSeqId = relevantEntries
            .map((entry) => parseInt(entry.acctgTransEntrySeqId || "0", 10))
            .reduce((max, curr) => Math.max(max, curr), 0);
        // Generate the next ID, padded to 3 digits (e.g., "001", "002")
        return (maxSeqId + 1).toString().padStart(3, "0");
    };
    
    const [addAcctgTransEntry, {data: entryResults, error, isLoading: isAddEntryLoading}] =
        useCreateAcctgTransEntryMutation();
    const [updateAcctgTransEntry, {isLoading: isUpdateEntryLoading}] =
        useUpdateAcctgTransEntryMutation();

    async function createEntry(newEntry: AcctgTransEntry) {
        setIsLoading(true);
        try {
            if (!newEntry.glAccountId || !newEntry.debitCreditFlag) {
                toast.error("GL Account Type, GL Account ID, and Debit/Credit Flag are required.");
                return;
            }

            // REFACTORED: Check if the acctgTransId + acctgTransEntrySeqId combination is unique
            const isDuplicate = existingEntries.some(
                (entry) =>
                    entry.acctgTransId === newEntry.acctgTransId &&
                    entry.acctgTransEntrySeqId === newEntry.acctgTransEntrySeqId
            );
            if (isDuplicate) {
                toast.error("Transaction Entry ID already exists for this transaction.");
                return;
            }

            const createdEntry = await addAcctgTransEntry(newEntry).unwrap();

            // REFACTORED: Update entry state with the created entry, including client-generated acctgTransEntrySeqId
            setEntry({
                acctgTransId: createdEntry.acctgTransId,
                acctgTransEntrySeqId: createdEntry.acctgTransEntrySeqId || newEntry.acctgTransEntrySeqId,
                acctgTransEntryTypeId: '_NA_',
                organizationPartyId: createdEntry.organizationPartyId,
                glAccountTypeId: createdEntry.glAccountTypeId,
                glAccountId: createdEntry.glAccountId,
                debitCreditFlag: createdEntry.debitCreditFlag,
                //partyId: createdEntry.partyId,
                origAmount: createdEntry.origAmount,
                amount: createdEntry.origAmount,
                //origCurrencyUomId: createdEntry.origCurrencyUomId,
                //purposeEnumId: createdEntry.purposeEnumId,
                voucherRef: createdEntry.voucherRef,
                //productId: createdEntry.productId,
                //reconcileStatusId: createdEntry.reconcileStatusId,
                //settlementTermId: createdEntry.settlementTermId,
                isSummary: createdEntry.isSummary,
                description: createdEntry.description,
            });

            setFormEditMode(2); // Transition to edit mode
            formRef.current = !formRef.current; // Trigger form update
            toast.success("Transaction Entry Created Successfully");
            console.log("Invalidating tag for acctgTransId:", newEntry.acctgTransId);
            dispatch(acctTransApi.util.invalidateTags(["ITransactions"]));
        } catch (error: any) {
            const message =
                error?.message ||
                error?.data?.errorMessage ||
                error?.data?.title ||
                "Failed to create transaction entry. Please try again.";
            toast.error(message);
            console.error("Error during entry creation:", error);
        } finally {
            setIsLoading(false);
        }
    }
    
    async function updateEntry(newEntry: AcctgTransEntry) {
        setIsLoading(true);
        try {
            if (!newEntry.glAccountId || !newEntry.debitCreditFlag) {
                toast.error("GL Account Type, GL Account ID, and Debit/Credit Flag are required.");
                return;
            }

            const updatedEntry = await updateAcctgTransEntry(newEntry).unwrap();

            setEntry({
                acctgTransId: updatedEntry.acctgTransId,
                acctgTransEntrySeqId: updatedEntry.acctgTransEntrySeqId,
                organizationPartyId: updatedEntry.organizationPartyId,
                glAccountTypeId: updatedEntry.glAccountTypeId,
                glAccountId: updatedEntry.glAccountId,
                debitCreditFlag: updatedEntry.debitCreditFlag,
                partyId: updatedEntry.partyId,
                origAmount: updatedEntry.origAmount,
                origCurrencyUomId: updatedEntry.origCurrencyUomId,
                purposeEnumId: updatedEntry.purposeEnumId,
                voucherRef: updatedEntry.voucherRef,
                productId: updatedEntry.productId,
                reconcileStatusId: updatedEntry.reconcileStatusId,
                settlementTermId: updatedEntry.settlementTermId,
                isSummary: updatedEntry.isSummary,
                description: updatedEntry.description,
            });

            formRef.current = !formRef.current;
            setFormEditMode(2);
            toast.success("Transaction Entry Updated Successfully");
            // REFACTORED: Invalidate specific tag for acctgTransId
            console.log("Invalidating tag for acctgTransId:", newEntry.acctgTransId);
            console.log("Invalidating tag for acctgTransId:", newEntry.acctgTransId);
            dispatch(acctTransApi.util.invalidateTags(["ITransactions"]));
        } catch (error: any) {
            const message =
                error?.message ||
                error?.data?.errorMessage ||
                error?.data?.title ||
                "Failed to update transaction entry. Please try again.";
            toast.error(message);
            console.error("Error during entry update:", error);
        } finally {
            setIsLoading(false);
        }
    }

    async function handleCreate(data: { values: AcctgTransEntry }) {
        const newEntry: AcctgTransEntry = {
            acctgTransId: data.acctgTransId,
            // REFACTORED: Generate unique acctgTransEntrySeqId for new entries
            acctgTransEntrySeqId: selectedMenuItem === "Create Entry"
                ? generateUniqueSeqId(data.acctgTransId)
                : data.acctgTransEntrySeqId,
            organizationPartyId: data.organizationPartyId,
            glAccountTypeId: data.glAccountTypeId,
            glAccountId: data.glAccountId,
            debitCreditFlag: data.debitCreditFlag,
            //partyId: data.partyId,
            origAmount: data.origAmount,
            amount: data.origAmount,
            //origCurrencyUomId: data.origCurrencyUomId,
            //purposeEnumId: data.purposeEnumId,
            voucherRef: data.voucherRef,
            //productId: data.productId,
            //reconcileStatusId: data.reconcileStatusId,
            //settlementTermId: data.settlementTermId,
            isSummary: data.isSummary,
            description: data.description,
        };

        if (selectedMenuItem === "Create Entry") {
            await createEntry(newEntry);
        } else if (selectedMenuItem === "Update Entry") {
            await updateEntry(newEntry);
        }
    }

    return {
        entryResults,
        error,
        isAddEntryLoading,
        isUpdateEntryLoading,
        formEditMode,
        setFormEditMode,
        entry,
        setEntry,
        handleCreate,
    };
};

export default useAcctgTransEntry;