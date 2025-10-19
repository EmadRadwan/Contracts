import { useCallback, useState } from "react";
import { setUiMultiPaymentCertificate } from "../slice/multiPaymentUiSlice";
import {MultiPaymentCertificate} from "../../../app/models/project/MultiPaymentCertificate";
import {useAppDispatch} from "../../../app/store/configureStore";

// REFACTOR: Create useMultiPaymentCertificate hook to manage certificate CRUD, mirroring useProjectCertificate for consistency.
export default function useMultiPaymentCertificate({
                                                       selectedCertificate,
                                                       editMode,
                                                       setFormKey,
                                                   }: {
    selectedCertificate?: MultiPaymentCertificate;
    editMode: number;
    setFormKey: (key: number) => void;
}) {
    const dispatch = useAppDispatch();
    const [certificate, setCertificate] = useState<MultiPaymentCertificate | undefined>(selectedCertificate);
    const [isLoading, setIsLoading] = useState(false);

    // REFACTOR: Handle certificate creation, dispatching to Redux store without API call, similar to useCertificateItem.
    const handleCreate = useCallback(
        ({ values, isValid, menuItem }: { values: MultiPaymentCertificate; isValid: boolean; menuItem: string }) => {
            if (!isValid) return;
            setIsLoading(true);
            try {
                dispatch(setUiMultiPaymentCertificate(values));
                setFormKey((prev) => prev + 1);
            } finally {
                setIsLoading(false);
            }
        },
        [dispatch, setFormKey]
    );

    // REFACTOR: Handle certificate updates, updating Redux store state.
    const handleUpdate = useCallback(
        ({ values, isValid, menuItem }: { values: MultiPaymentCertificate; isValid: boolean; menuItem: string }) => {
            if (!isValid) return;
            setIsLoading(true);
            try {
                dispatch(setUiMultiPaymentCertificate(values));
                setFormKey((prev) => prev + 1);
            } finally {
                setIsLoading(false);
            }
        },
        [dispatch, setFormKey]
    );

    return {
        certificate,
        setCertificate,
        handleCreate,
        handleUpdate,
        isLoading,
    };
}