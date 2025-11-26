import { useEffect, useState } from 'react';
import { useCalculateInvoiceTotalMutation } from '../../../../app/store/apis/invoice/invoicesApi';

interface InvoiceTotalResponse {
    invoiceId: string;
    total: number;
    outstandingAmount: number;
}

// REFACTOR: Renamed and expanded return type to reflect new API shape
// Purpose: Accurately represent both total and outstanding amount from CalculateInvoiceTotal API
// Improvement: Enables precise financial display (total vs. still owed), critical for payment tracking
export const useInvoiceTotal = (invoiceId: string | undefined) => {
    const [calculateInvoiceTotal, { isLoading, error }] = useCalculateInvoiceTotalMutation();

    // REFACTOR: Split state into total and outstandingAmount instead of just iTotal
    // Purpose: Prevent stale or mismatched data when only one value is needed
    // Improvement: Better performance, clearer intent, and easier debugging
    const [total, setTotal] = useState<number | null>(null);
    const [outstandingAmount, setOutstandingAmount] = useState<number | null>(null);

    useEffect(() => {
        if (!invoiceId) {
            // REFACTOR: Reset both values when no invoiceId
            // Purpose: Prevent displaying stale data from previous invoices
            setTotal(null);
            setOutstandingAmount(null);
            return;
        }

        // REFACTOR: Trigger calculation and handle response with proper typing
        // Purpose: Safely extract both total and outstandingAmount from API
        // Improvement: Uses .unwrap() for clean error handling and avoids side effects in render
        calculateInvoiceTotal(invoiceId)
            .unwrap()
            .then((result: InvoiceTotalResponse) => {
                // Validate and sanitize numeric values
                const safeTotal = typeof result.total === 'number' && !isNaN(result.total) ? result.total : 0;
                const safeOutstanding =
                    typeof result.outstandingAmount === 'number' && !isNaN(result.outstandingAmount)
                        ? result.outstandingAmount
                        : safeTotal; // fallback: if outstanding missing, assume full amount is outstanding

                setTotal(safeTotal);
                setOutstandingAmount(safeOutstanding);
            })
            .catch((err) => {
                console.error('Failed to calculate invoice total:', err);
                // REFACTOR: Set to null on error instead of 0 — this allows UI to show error state clearly
                // Purpose: Distinguish between "zero balance" and "failed to load"
                setTotal(null);
                setOutstandingAmount(null);
            });
    }, [invoiceId, calculateInvoiceTotal]);

    // REFACTOR: Return both values + loading/error for full control in component
    // Improvement: Consumer can now show loading spinners, errors, or conditional rendering accurately
    return {
        total,                    // e.g., 20040.00
        outstandingAmount,        // e.g., 40.00
        isLoading,
        error,
        isSuccess: !isLoading && !error && total !== null,
    };
};