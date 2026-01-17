import {useCallback, useEffect, useState} from 'react';
import { useCalculateInvoiceTotalMutation } from '../../../../app/store/apis/invoice/invoicesApi';

interface InvoiceTotalResponse {
    invoiceId: string;
    total: number;
    outstandingAmount: number;
}


export const useInvoiceTotal = (invoiceId: string | undefined) => {
    const [calculateInvoiceTotal, { isLoading, error }] = useCalculateInvoiceTotalMutation();

    const [total, setTotal] = useState<number | null>(null);
    const [outstandingAmount, setOutstandingAmount] = useState<number | null>(null);

    // In useInvoiceTotal hook
    const refreshTotal = useCallback(async () => {
        if (!invoiceId) {
            return;
        }


        try {
            const result = await calculateInvoiceTotal(invoiceId).unwrap();

            const safeTotal = Number(result.total) || 0;
            const safeOutstanding = Number(result.outstandingAmount) ?? safeTotal;

            setTotal(safeTotal);
            setOutstandingAmount(safeOutstanding);
        } catch (err) {
            console.error("[useInvoiceTotal] refresh failed", err);
            setTotal(null);
            setOutstandingAmount(null);
        }
    }, [invoiceId, calculateInvoiceTotal]);   // ← invoiceId MUST be in deps

    useEffect(() => {
        refreshTotal();
    }, [refreshTotal]);

    return {
        total,
        outstandingAmount,
        isLoading,
        error,
        isSuccess: !isLoading && !error && total !== null,
        refreshTotal,           // ← expose this
    };
};