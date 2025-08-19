import { useEffect, useState } from 'react';
import { useCalculateInvoiceTotalMutation } from '../../../../app/store/apis/invoice/invoicesApi';

// REFACTOR: Updated hook to handle loading and error states more robustly
// Ensures iTotal is only updated with valid data and provides clear error feedback
export const useInvoiceTotal = (invoiceId: string | undefined) => {
  const [calculateInvoiceTotal, { data, isLoading, error }] = useCalculateInvoiceTotalMutation();
  const [iTotal, setITotal] = useState<number>(0);

  useEffect(() => {
    if (invoiceId) {
      calculateInvoiceTotal(invoiceId)
          .unwrap()
          .then((result) => {
          
            const total = typeof result.total === 'number' ? result.total : 0;
            setITotal(total);
          })
          .catch((err) => {
            console.error('Failed to fetch invoice total:', err);
            setITotal(0); // Fallback to 0 on error
          });
    } else {
      setITotal(0); // Reset iTotal if no invoiceId
    }
  }, [invoiceId, calculateInvoiceTotal]);

  return { iTotal, isLoading, error };
};