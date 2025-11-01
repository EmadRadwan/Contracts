import { toast } from "react-toastify";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import {useCreatePaymentApplicationMutation} from "../../../../app/store/apis";

interface UseApplyPaymentProps {
    paymentId: string;
    onSuccess?: () => void;   // e.g. close modal, refresh UI
}

/**
 * Hook that encapsulates the “apply payment” flow.
 * Returns a submit handler that matches the shape expected by AddPaymentApplicationForm.
 */
export const useApplyPayment = ({
                                    paymentId,
                                    onSuccess,
                                }: UseApplyPaymentProps) => {
    const [createApplication, { isLoading }] = useCreatePaymentApplicationMutation();
    const { getTranslatedLabel } = useTranslationHelper();
    const loc = "accounting.payments.applications";

    // REFACTOR: Centralise error handling + toast messages
    // Purpose: Keeps UI components thin, guarantees consistent UX
    const handleError = (err: any) => {
        const msg =
            err?.data?.title ||
            getTranslatedLabel(`${loc}.applyError`, "Failed to apply payment");
        toast.error(msg);
        console.error(err);
    };

    const handleSubmit = async (values: {
        invoiceId: string;
        amountApplied: number;
    }) => {
        if (isLoading) return;

        try {
            const payload: PaymentApplicationParam = {
                paymentId,
                invoiceId: values.invoiceId.invoiceId,
                amountApplied: values.amountApplied,
               
            };

            await createApplication(payload).unwrap();

            toast.success(
                getTranslatedLabel(`${loc}.applySuccess`, "Payment applied successfully")
            );
            onSuccess?.();
        } catch (err) {
            handleError(err);
        }
    };

    return { handleSubmit, isLoading };
};