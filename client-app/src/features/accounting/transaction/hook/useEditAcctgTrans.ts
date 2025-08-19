import React, {useState} from "react";

import {toast} from "react-toastify";
import {AcctgTrans} from "../../../../app/models/accounting/acctgTrans";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import {usePostAcctgTransMutation, useUpdateAcctgTransMutation} from "../../../../app/store/apis";
import {useNavigate} from "react-router";
import {CreateAcctgTransParams} from "../../../../app/models/accounting/createAcctgTransParams";

type UseAcctgTransProps = {
    selectedMenuItem?: string;
    selectedAcctgTrans?: AcctgTrans;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
};
const useEditAcctgTrans = ({
                               selectedMenuItem, selectedAcctgTrans, setIsLoading
                           }: UseAcctgTransProps) => {
    
    const companyId = useAppSelector(state => state.accountingSharedUi.selectedAccountingCompanyId);


    const [updateAcctgTransTrigger, { data: updateAcctgTransResults }] = useUpdateAcctgTransMutation();
    const [postAcctgTransTrigger, { data: postAcctgTransResults }] = usePostAcctgTransMutation(); // REFACTORED: Use post mutation

    const [acctgTrans, setAcctgTrans] = useState<AcctgTrans | undefined>(() => {
        return selectedAcctgTrans;
    });


    async function handleUpdate(data: any) {
        if (!data || !data.values) {
            toast.error("Invalid form data");
            setIsLoading(false);
            return;
        }

        const newAcctgTrans: CreateAcctgTransParams = {
            acctgTransId: acctgTrans?.acctgTransId || "",
            transactionDate: data.values.transactionDate,
            acctgTransTypeId: data.values.acctgTransTypeId,
            glFiscalTypeId: data.values.glFiscalTypeId,
            partyId: data.values.fromPartyId,
            roleTypeId: data.values.roleTypeId,
            invoiceId: data.values.invoiceId,
            paymentId: data.values.paymentId,
            shipmentId: data.values.shipmentId,
            workEffortId: data.values.workEffortId,
            description: data.values.description,
            isPosted: acctgTrans?.isPosted || "N",
            organizationPartyId: companyId,
        };

        try {
            setIsLoading(true);
            if (newAcctgTrans.acctgTransId) {
                const updatedAcctgTrans = await updateAcctgTransTrigger(newAcctgTrans).unwrap();
                if (updatedAcctgTrans) {
                    setAcctgTrans({
                        ...newAcctgTrans,
                        acctgTransId: updatedAcctgTrans.acctgTransId,
                    });
                    toast.success("Accounting Transaction Updated Successfully");
                }
            } else {
                toast.error("No transaction ID provided for update");
            }
        } catch (error: any) {
            console.error(error);
            toast.error("Failed to update Accounting Transaction");
        } finally {
            setIsLoading(false);
        }
    }

    async function handlePostTransaction(acctgTransId: string) {
        if (!acctgTransId) {
            toast.error("No transaction ID provided for posting");
            setIsLoading(false);
            return;
        }

        try {
            setIsLoading(true);
            const messages = await postAcctgTransTrigger({ acctgTransId, verifyOnly: false }).unwrap();
            if (messages.length === 0) {
                setAcctgTrans({
                    ...acctgTrans!,
                    isPosted: "Y",
                    postedDate: new Date(),
                });
                toast.success("Accounting Transaction Posted Successfully");
            } else {
                // Display all messages as errors or warnings
                messages.forEach((message: string) => {
                    if (message.includes("Error Journal")) {
                        toast.warn(message); // Warning for Error Journal case
                    } else {
                        toast.error(message); // Error for other cases
                    }
                });
            }
        } catch (error: any) {
            console.error(error);
            toast.error("Failed to post Accounting Transaction");
        } finally {
            setIsLoading(false);
        }
    }


    return {
        acctgTrans,
        setAcctgTrans,
        handleUpdate,
        handlePostTransaction,
    };
};
export default useEditAcctgTrans;

