import { useState } from "react";
import { toast } from "react-toastify";
import { useAppDispatch } from "../../../../app/store/configureStore";
import {useCreateMultiAcctgTransWithEntriesMutation, usePostAcctgTransMutation} from "../../../../app/store/apis";

// REFACTOR: Update interface to match backend Command structure
export interface CreateMultiAcctgTransWithEntriesParams {
    CreateMultiAcctgTransParams: {
        AcctgTransTypeId: string;
        TransactionDate: Date | string;
        OrganizationPartyId: string;
        HeaderDescription: string; // REFACTOR: Ensure HeaderDescription is included
        Description: string;
        IsPosted?: string;
        GlFiscalTypeId: string;
    };
    Entries: MultiAcctgTransEntryParams[];
}

export interface MultiAcctgTransEntryParams {
    debitGlAccountId?: string;
    creditGlAccountId?: string;
    amount: number;
    description: string;
    debitCreditFlag: "D" | "C";
}

// REFACTOR: Define response interface for clarity
export interface CreateMultiAcctgTransResponse {
    transactionId: string;
}

const useMultiAcctgTrans = () => {
    const dispatch = useAppDispatch();
    const [createMultiAcctgTransWithEntries] = useCreateMultiAcctgTransWithEntriesMutation();
    const [postAcctgTransTrigger] = usePostAcctgTransMutation(); // REFACTOR: Post mutation

    const [isLoading, setIsLoading] = useState(false);

    // REFACTOR: Ensure transactionId is returned from API call
    async function saveMultiAcctgTransWithEntries(params: CreateMultiAcctgTransWithEntriesParams) {
        setIsLoading(true);
        try {
            const result = await createMultiAcctgTransWithEntries(params).unwrap() as CreateMultiAcctgTransResponse;
            toast.success("Transaction and entries saved successfully");
            return result; // REFACTOR: Return result containing transactionId
        } catch (error: any) {
            toast.error(`Failed to save transaction: ${error?.message || "Unknown error"}`);
            throw error;
        } finally {
            setIsLoading(false);
        }
    }

    async function postTransaction(acctgTransId: string): Promise<string[] | void> {
        if (!acctgTransId) {
            toast.error("No transaction ID provided for posting");
            return;
        }

        setIsLoading(true);
        try {
            const messages = await postAcctgTransTrigger({
                acctgTransId,
                verifyOnly: false,
            }).unwrap();

            return messages;
        } catch (error: any) {
            console.error(error);
            toast.error("Failed to post Accounting Transaction");
            throw error;
        } finally {
            setIsLoading(false);
        }
    }


    return { isLoading, saveMultiAcctgTransWithEntries, postTransaction };
};


export default useMultiAcctgTrans;