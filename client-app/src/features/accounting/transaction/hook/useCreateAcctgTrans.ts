import React, {useState} from "react";

import {toast} from "react-toastify";
import {AcctgTrans} from "../../../../app/models/accounting/acctgTrans";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import {useCreateAcctgTransMutation, useCreateAcctgTransQuickMutation} from "../../../../app/store/apis";
import {CreateAcctgTransAndEntriesParams} from "../../../../app/models/accounting/createAcctgTransAndEntriesParams";
import {useNavigate} from "react-router";
import {CreateAcctgTransParams} from "../../../../app/models/accounting/createAcctgTransParams";

type UseAcctgTransProps = {
    selectedMenuItem?: string;
    editMode?: number;
    selectedAcctgTrans?: AcctgTrans;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
};
const useCreateAcctgTrans = ({
                                 selectedMenuItem, editMode, setIsLoading
                             }: UseAcctgTransProps) => {

    const dispatch = useAppDispatch();
    const navigate = useNavigate();

    const [
        createAcctgTransQuickTrigger,
        {data: CreateAcctgTransQuickResults},
    ] = useCreateAcctgTransQuickMutation();
    
    const [
        createAcctgTransTrigger,
        {data: CreateAcctgTransResults},
    ] = useCreateAcctgTransMutation();
    
    
    const companyId = useAppSelector(state => state.accountingSharedUi.selectedAccountingCompanyId);


    const [formEditMode, setFormEditMode] = useState(editMode);
    const [acctgTrans, setAcctgTrans] = useState<AcctgTrans | undefined>(() => {
        return {
            acctgTransId: "",
            transactionDate: new Date()
        };
    });

    async function createAcctgTransQuick(newAcctgTrans: CreateAcctgTransAndEntriesParams) {
        try {
            let createdAcctgTrans: any;
            try {
                createdAcctgTrans = await createAcctgTransQuickTrigger(newAcctgTrans).unwrap();
            } catch (error) {
                toast.error("Failed to create AcctgTrans");
            }
            if (createdAcctgTrans) {
                // update the newAcctgTrans object with the created acctgTransId
                // and pass it to the EditAcctgTrans
                newAcctgTrans.acctgTransId = createdAcctgTrans.acctgTransId;
                newAcctgTrans.partyId = newAcctgTrans.fromPartyId;

                // Navigate to the EditAcctgTrans with the selectedAcctgTrans object
                navigate(`/editAcctgTrans/${createdAcctgTrans.acctgTransId}`, {state: {selectedAcctgTrans: newAcctgTrans}});

                toast.success("AcctgTrans Created Successfully");
            }

        } catch (error: any) {
            console.log(error);
        }
        setIsLoading(false);
    }

async function createAcctgTrans(newAcctgTrans: CreateAcctgTransParams) {
        try {
            let createdAcctgTrans: any;
            try {
                createdAcctgTrans = await createAcctgTransTrigger(newAcctgTrans).unwrap();
            } catch (error) {
                toast.error("Failed to create AcctgTrans");
            }
            if (createdAcctgTrans) {
                // update the newAcctgTrans object with the created acctgTransId
                // and pass it to the EditAcctgTrans
                newAcctgTrans.acctgTransId = createdAcctgTrans.acctgTransId;
                newAcctgTrans.partyId = newAcctgTrans.fromPartyId;

                // Navigate to the EditAcctgTrans with the selectedAcctgTrans object
                navigate(`/editAcctgTrans/${createdAcctgTrans.acctgTransId}`, {state: {selectedAcctgTrans: newAcctgTrans}});

                toast.success("AcctgTrans Created Successfully");
            }

        } catch (error: any) {
            console.log(error);
        }
        setIsLoading(false);
    }


    async function handleCreateQuick(acctgTrans: any) {

        let newAcctgTrans: CreateAcctgTransAndEntriesParams;
        newAcctgTrans = {
            acctgTransId: acctgTrans!.acctgTransId,
            transactionDate: acctgTrans.transactionDate,
            acctgTransTypeId: acctgTrans.acctgTransTypeId,
            partyId: acctgTrans.fromPartyId?.fromPartyId,
            fromPartyId: acctgTrans.fromPartyId,
            productId: acctgTrans.productId,
            roleTypeId: acctgTrans.roleTypeId,
            shipmentId: acctgTrans.shipmentId,
            invoiceId: acctgTrans.invoiceId,
            debitGlAccountId: acctgTrans.debitGlAccountId,
            paymentId: acctgTrans.paymentId,
            creditGlAccountId: acctgTrans.creditGlAccountId,
            description: acctgTrans.description,
            isPosted: "N",
            organizationPartyId: companyId,
            amount: acctgTrans.amount,
        };
        // create the debit entry and assign the glAccountId
        const debitEntry = {
            debitCreditFlag: "D",
            amount: acctgTrans.amount,
            glAccountId: acctgTrans.debitGlAccountId,
            productId: acctgTrans.productId?.productId,
            partyId: acctgTrans.fromPartyId?.fromPartyId,
            description: acctgTrans.description,
            organizationPartyId: companyId,
        };
        // create the credit entry and assign the glAccountId
        const creditEntry = {
            debitCreditFlag: "C",
            amount: acctgTrans.amount,
            glAccountId: acctgTrans.creditGlAccountId,
            productId: acctgTrans.productId?.productId,
            partyId: acctgTrans.fromPartyId?.fromPartyId,
            description: acctgTrans.description,
            organizationPartyId: companyId,
        };
        newAcctgTrans.acctgTransEntries = [debitEntry, creditEntry];


        if (!acctgTrans.acctgTransId) {
            await createAcctgTransQuick(newAcctgTrans);
        }
    }

    async function handleCreate(acctgTrans: any) {
        // Map input to CreateAcctgTransParams
        const newAcctgTrans: CreateAcctgTransParams = {
            // If an ID already exists, it may indicate an update or existing transaction.
            acctgTransId: acctgTrans.acctgTransId,
            transactionDate: acctgTrans.transactionDate,
            acctgTransTypeId: acctgTrans.acctgTransTypeId,
            glFiscalTypeId: acctgTrans.glFiscalTypeId,
            partyId: acctgTrans.partyId,
            roleTypeId: acctgTrans.roleTypeId,
            invoiceId: acctgTrans.invoiceId,
            paymentId: acctgTrans.paymentId,
            shipmentId: acctgTrans.shipmentId,
            workEffortId: acctgTrans.workEffortId,
            description: acctgTrans.description,
            // Default to "N" since the transaction is not yet posted
            isPosted: "N",
            // Additional optional fields can be mapped here if available
            // e.g., scheduledPostingDate, glJournalId, voucherRef, voucherDate, etc.
        };

        // If no transaction ID exists, call the service to create the new transaction
        if (!acctgTrans.acctgTransId) {
            await createAcctgTrans(newAcctgTrans);
        }
    }


    return {
        formEditMode,
        setFormEditMode,
        acctgTrans,
        setAcctgTrans,
        handleCreateQuick, handleCreate
    };
};
export default useCreateAcctgTrans;

