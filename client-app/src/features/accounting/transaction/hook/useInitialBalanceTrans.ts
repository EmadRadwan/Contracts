// src/features/accounting/gl/hook/useInitialBalanceTrans.ts
import { useState } from "react";
import { toast } from "react-toastify";
import { useAppDispatch } from "../../../../app/store/configureStore";
import {
    useCreateInitialBalanceTransMutation,
    usePostAcctgTransMutation,
} from "../../../../app/store/apis";

export interface CreateInitialBalanceTransParams {
    CreateInitialBalanceTransParams: {
        AcctgTransTypeId: string;
        TransactionDate: Date | string;
        OrganizationPartyId: string;
        HeaderDescription: string;
        GlFiscalTypeId: string;
        IsPosted: string;
    };
    Entry: {
        glAccountId: string;
        partyId?: string;          // ← ADD: optional (only for AR/AP accounts typically)
        amount: number;
        description: string;
        debitCreditFlag: "D" | "C";
    };
}

export interface CreateInitialBalanceTransResponse {
    acctgTransId: string;
}

const useInitialBalanceTrans = () => {
    const [createInitialBalanceTrans] = useCreateInitialBalanceTransMutation();
    const [postAcctgTransTrigger] = usePostAcctgTransMutation();
    const [isLoading, setIsLoading] = useState(false);

    const saveInitialBalanceTrans = async (params: CreateInitialBalanceTransParams) => {
        setIsLoading(true);
        try {
            const result = await createInitialBalanceTrans(params).unwrap();
            toast.success("Initial balance transaction created");
            return result as CreateInitialBalanceTransResponse;
        } catch (error: any) {
            toast.error(error?.data?.message || "Failed to save initial balance");
            throw error;
        } finally {
            setIsLoading(false);
        }
    };

    const postTransaction = async (acctgTransId: string): Promise<string[] | void> => {
        setIsLoading(true);
        try {
            const messages = await postAcctgTransTrigger({
                acctgTransId,
                verifyOnly: false,
            }).unwrap();
            return messages;
        } catch (error: any) {
            toast.error("Failed to post transaction");
            throw error;
        } finally {
            setIsLoading(false);
        }
    };

    return { isLoading, saveInitialBalanceTrans, postTransaction };
};

export default useInitialBalanceTrans;